using UnityEngine;
using System.Collections;

public class DetectMonsterBaseController : WallMonsterBaseController<DetectMonsterData>,
    OnReturnPool<DetectMonsterBaseController>
{
    private OnReturnPoolEvent<DetectMonsterBaseController> OnReturnPoolEvent;

    [Header("Detect Settings")]
    private Transform targetPlayer;
    private Vector2? lastKnownPosition;

    private float detectInterval = 0.1f;
    private float detectTimer = 0f;
    private bool isDashing = false;

    [Header("Lunge Settings")]
    [SerializeField] private float lungeDistance = 1.2f; // 돌진 거리
    [SerializeField] private float lungeDuration = 0.15f;
    [SerializeField] private float lungePause = 0.1f;
    [SerializeField] private float returnDuration = 0.25f;

    private const string ANIM_MOVE = "animation";

    // 기본 Wall 몬스터 동작을 위한 변수들
    private float baseIdleTimer = 0f;
    private float baseIdleTime = 1f;
    private bool isMovingRight = false;

    protected override void Awake()
    {
        base.Awake();
        enabled = false;
    }
    public override void Setup(DetectMonsterData data)
    {
        if (data == null)
        {
            return;
        }

        // DetectMonster는 Spine을 사용하므로 isSpine을 강제로 true로 설정
        data.isSpine = true;

        base.Setup(data);

        // HP 강제 설정 (부모에서 제대로 안 되는 경우 대비)
        info = data;
        currentHP = data.maxHp;


        isDashing = false;
        lastKnownPosition = null;
        targetPlayer = null;
        baseIdleTimer = 0f;
        isMovingRight = false;
        detectTimer = 0f;
        state = StateType.Idle;

        // 초기 애니메이션 설정
        SetSpineAnimation(ANIM_MOVE, true);

        // Setup 완료 후 FixedUpdate 활성화
        enabled = true;
    }

    protected override void HandleUpdate()
    {
        return;
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            rigid.linearVelocity = Vector2.zero;
            return;
        }

        if (isDashing)
        {
            return;
        }

        // 플레이어 감지
        detectTimer += Time.fixedDeltaTime;
        if (detectTimer >= detectInterval)
        {
            detectTimer = 0f;
            DetectPlayer();
        }

        if (targetPlayer != null)
        {
            ChaseAndAttackPlayer();
        }
        else if (lastKnownPosition != null)
        {
            MoveToLastKnownPosition();
        }
        else
        {
            DefaultWallMonsterBehavior();
        }
    }

    private void ChaseAndAttackPlayer()
    {
        SetSpineAnimation(ANIM_MOVE, true);

        // 플레이어 감지 즉시 돌진 공격
        StopMove();
        Attack();
    }

    private void MoveToLastKnownPosition()
    {
        SetSpineAnimation(ANIM_MOVE, true);

        Vector2 dir = (lastKnownPosition.Value - (Vector2)transform.position).normalized;
        Flip(dir.x < 0);
        MoveCharacter(MonsterManager.Instance.GetAreaBound(), dir, null);

        if (Vector2.Distance(transform.position, lastKnownPosition.Value) < 0.1f)
        {
            lastKnownPosition = null;
        }
    }

    private void DefaultWallMonsterBehavior()
    {
        // 기본 Wall 몬스터처럼 동작: Idle -> 오른쪽 이동
        if (!isMovingRight)
        {
            // Idle 상태
            StopMove();
            baseIdleTimer += Time.fixedDeltaTime;

            if (baseIdleTimer >= baseIdleTime)
            {
                baseIdleTimer = 0f;
                isMovingRight = true;
                SetSpineAnimation(ANIM_MOVE, true);
            }
        }
        else
        {
            // 오른쪽으로 이동
            SetSpineAnimation(ANIM_MOVE, true);
            Flip(false); // 오른쪽 보기
            MoveCharacter(MonsterManager.Instance.GetAreaBound(), Vector2.right, null);

            // 벽 체크
            Vector2 spriteHalfSize = (characterCollider != null) ? characterCollider.bounds.extents : Vector2.zero;
            var bounds = MonsterManager.Instance.GetAreaBound();

            if (transform.position.x + spriteHalfSize.x >= bounds.max.x - 0.1f)
            {
                StartCoroutine(SelfDestructCoroutine(2f));
            }
        }
    }

    private IEnumerator SelfDestructCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        MonsterManager.Instance.SubWorldHpEvent();
        Dead();
    }

    private void DetectPlayer()
    {
        if (MonsterManager.Instance.IsPlayerDead())
        {
            targetPlayer = null;
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, monsterData.DetectRange, LayerMask.GetMask("Player"));
        Transform potentialTarget = null;
        float closestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player1"))
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    potentialTarget = hit.transform;
                }
            }
        }
        targetPlayer = potentialTarget;

        if (targetPlayer != null)
        {
            lastKnownPosition = targetPlayer.position;
        }
    }

    protected override void Attack()
    {
        if (isDashing || MonsterManager.Instance.IsPlayerDead()) return;

        Vector2 targetPosition;
        if (targetPlayer != null)
        {
            targetPosition = targetPlayer.position;
            lastKnownPosition = targetPosition;
        }
        else if (lastKnownPosition != null)
        {
            targetPosition = lastKnownPosition.Value;
        }
        else
        {
            return;
        }

        Vector2 startPosition = transform.position;
        Vector2 dashDir = (targetPosition - startPosition).normalized;

        Flip(dashDir.x < 0);
        SetSpineAnimation(ANIM_MOVE, true);

        StartCoroutine(LungeAndReturn(startPosition, targetPosition, dashDir));
    }

    private IEnumerator LungeAndReturn(Vector2 startPos, Vector2 targetPos, Vector2 dir)
    {
        isDashing = true;

        // 돌진할 목표 지점 계산 (고정 거리만큼)
        Vector2 lungeEndPos = startPos + dir * lungeDistance;

        // 돌진 (데미지는 OnTriggerStay2D에서 자동 처리)
        float elapsed = 0f;
        while (elapsed < lungeDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / lungeDuration;
            Vector2 newPos = Vector2.Lerp(startPos, lungeEndPos, t);
            rigid.MovePosition(newPos);
            yield return new WaitForFixedUpdate();
        }
        rigid.MovePosition(lungeEndPos);

        yield return new WaitForSeconds(lungePause);

        // 원래 위치로 복귀
        elapsed = 0f;
        Vector2 currentPos = transform.position;
        while (elapsed < returnDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / returnDuration;
            Vector2 newPos = Vector2.Lerp(currentPos, startPos, t);
            rigid.MovePosition(newPos);
            yield return new WaitForFixedUpdate();
        }
        rigid.MovePosition(startPos);

        yield return new WaitForSeconds(info.attackDelay);

        SetSpineAnimation(ANIM_MOVE, true);
        isDashing = false;
    }

    protected void SetSpineAnimation(string animName, bool loop)
    {
        if (skeleton != null && skeleton.skeletonDataAsset != null)
        {
            if (skeleton.AnimationName != animName)
            {
                skeleton.AnimationState.SetAnimation(0, animName, loop);
            }
        }
    }

    public void Init(OnReturnPoolEvent<DetectMonsterBaseController> onReturnPoolEvent)
    {
        this.OnReturnPoolEvent = onReturnPoolEvent;
    }

    protected override void Dead()
    {
        base.Dead();
        OnReturnPoolEvent?.Invoke(this);
    }
}