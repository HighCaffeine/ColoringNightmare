using UnityEngine;
using System.Collections;

public class DetectMonsterBaseController : WallMonsterBaseController<DetectMonsterData>,
    OnReturnPool<DetectMonsterBaseController>
{
    private OnReturnPoolEvent<DetectMonsterBaseController> OnReturnPoolEvent;

    [Header("Detect Settings")]
    private Transform targetPlayer;

    private float detectInterval = 0.1f;
    private float detectTimer = 0f;
    private bool isDashing = false;
    private bool hasAttackToken = false; // 토큰 보유 여부

    [Header("Lunge Settings")]
    [SerializeField] private float lungeDistance = 1.5f;
    [SerializeField] private float lungeDuration = 0.2f;
    [SerializeField] private float lungePause = 0.3f;
    [SerializeField] private float returnDuration = 0.5f;

    [Header("Warning Settings")]
    [SerializeField] private GameObject warningPrefab;
    [SerializeField] private float warningDuration = 0.6f; // 장판 지속 시간
    [SerializeField] private float warningWidth = 0.2f;    // 장판 너비

    private const string ANIM_MOVE = "animation";
    private const string ANIM_IDLE = "idle";
    private const string ANIM_ATTACK = "Attack";

    // 기본 Wall 몬스터 동작 변수
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
        if (data == null) return;

        // 가지고 있던 토큰 반납
        if (hasAttackToken)
        {
            MonsterManager.Instance.ReturnAttackToken();
            hasAttackToken = false;
        }

        data.isSpine = true;
        base.Setup(data);

        info = data;
        currentHP = data.maxHp;

        isDashing = false;
        targetPlayer = null;
        baseIdleTimer = 0f;
        isMovingRight = false;
        detectTimer = 0f;
        state = StateType.Idle;

        SetSpineAnimation(ANIM_MOVE, true);
        enabled = true;
    }

    protected override void HandleUpdate()
    {
        return;
    }

    private void FixedUpdate()
    {
        if (isDead || isDashing)
        {
            if (isDead) rigid.linearVelocity = Vector2.zero;
            return; // 돌진 중이거나 죽었으면 로직 중단
        }

        // 플레이어 감지 (주기적)
        detectTimer += Time.fixedDeltaTime;
        if (detectTimer >= detectInterval)
        {
            detectTimer = 0f;
            DetectPlayer();
        }

        if (targetPlayer != null)
        {
            if (hasAttackToken)
            {
                ChaseAndAttackPlayer();
            }
            else
            {
                // 권한 없음 -> 요청 시도
                if (MonsterManager.Instance.TryRequestAttackToken())
                {
                    hasAttackToken = true;
                    ChaseAndAttackPlayer();
                }
                else
                {
                    // 권한 거절됨 -> 못 본 척하고 갈 길 감 (Wall 행동)
                    DefaultWallMonsterBehavior();
                }
            }
        }
        else
        {
            // 플레이어 없음 -> 토큰 반납하고 갈 길 감
            if (hasAttackToken)
            {
                MonsterManager.Instance.ReturnAttackToken();
                hasAttackToken = false;
            }
            DefaultWallMonsterBehavior();
        }
    }

    private void ChaseAndAttackPlayer()
    {
        float distance = Vector2.Distance(transform.position, targetPlayer.position);

        // 공격 사거리 내 진입
        if (distance <= monsterData.AttackRange)
        {
            StopMove();
            Attack(); // 공격 코루틴 시작
        }
        else
        {
            // 추격 이동
            SetSpineAnimation(ANIM_MOVE, true);
            Vector2 dir = (targetPlayer.position - transform.position).normalized;
            Flip(dir.x < 0); // 플레이어 방향 보기
            MoveCharacter(MonsterManager.Instance.GetAreaBound(), dir, null);
        }
    }

    // 기본 벽 몬스터 패턴
    private void DefaultWallMonsterBehavior()
    {
        if (!isMovingRight)
        {
            StopMove();
            baseIdleTimer += Time.fixedDeltaTime;
            SetSpineAnimation(ANIM_IDLE, true);

            if (baseIdleTimer >= baseIdleTime)
            {
                baseIdleTimer = 0f;
                isMovingRight = true;
            }
        }
        else
        {
            SetSpineAnimation(ANIM_MOVE, true);
            Flip(false); // 오른쪽 보기
            MoveCharacter(MonsterManager.Instance.GetAreaBound(), Vector2.right, null);

            Vector2 spriteHalfSize = (characterCollider != null) ? characterCollider.bounds.extents : Vector2.zero;
            var bounds = MonsterManager.Instance.GetAreaBound();

            // 맵 끝에 도달하면 자폭
            if (transform.position.x + spriteHalfSize.x >= bounds.max.x - 0.1f)
            {
                StartCoroutine(SelfDestructCoroutine(2f));
            }
        }
    }

    private IEnumerator SelfDestructCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        isSelfDestruct = true;
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
    }

    protected override void Attack()
    {
        if (isDashing) return;

        // 타겟 위치 확정
        Vector2 targetPosition = (targetPlayer != null) ? (Vector2)targetPlayer.position : (Vector2)transform.position;
        Vector2 startPosition = transform.position;
        Vector2 dashDir = (targetPosition - startPosition).normalized;

        Flip(dashDir.x < 0);

        // 공격 코루틴 시작 (상태 변경 포함)
        StartCoroutine(PrepareAndLunge(startPosition, dashDir));
    }

    private IEnumerator PrepareAndLunge(Vector2 startPos, Vector2 dir)
    {
        isDashing = true;
        state = StateType.Attack;

        Vector2 lungeEndPos = startPos + dir * lungeDistance;

        if (warningPrefab != null)
        {
            Vector2 midPoint = (startPos + lungeEndPos) * 0.5f;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            GameObject warning = Instantiate(warningPrefab, midPoint, rotation);

            warning.transform.localScale = new Vector3(lungeDistance, warningWidth, 1f);

            WarningArea warningScript = warning.GetComponent<WarningArea>();
            if (warningScript != null)
            {
                warningScript.Setup(warningDuration, WarningFillType.LeftToRight);
            }

            yield return new WaitForSeconds(warningDuration);
            Destroy(warning);
        }
        else
        {
            yield return new WaitForSeconds(warningDuration);
        }

        SetSpineAnimation(ANIM_ATTACK, false);

        float elapsed = 0f;
        while (elapsed < lungeDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / lungeDuration;

            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            Vector2 newPos = Vector2.Lerp(startPos, lungeEndPos, t);
            rigid.MovePosition(newPos);
            yield return new WaitForFixedUpdate();
        }
        rigid.MovePosition(lungeEndPos);

        yield return new WaitForSeconds(lungePause);

        elapsed = 0f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / returnDuration;
            Vector2 newPos = Vector2.Lerp(lungeEndPos, startPos, t);
            rigid.MovePosition(newPos);
            yield return new WaitForFixedUpdate();
        }
        rigid.MovePosition(startPos);

        yield return new WaitForSeconds(info.attackDelay);

        if (hasAttackToken)
        {
            MonsterManager.Instance.ReturnAttackToken();
            hasAttackToken = false;
        }

        SetSpineAnimation(ANIM_IDLE, true);
        isDashing = false;
        ChangeState(StateType.Idle);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (isDead || !hasAttackToken) return;

        if (other.CompareTag("Player1"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(monsterData.dmg);
            }
        }
    }

    protected void SetSpineAnimation(string animName, bool loop)
    {
        if (animName == ANIM_IDLE || animName == ANIM_ATTACK)
        {
            skeleton.AnimationState.ClearTrack(0);
            return;
        }

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
        if (hasAttackToken)
        {
            MonsterManager.Instance.ReturnAttackToken();
            hasAttackToken = false;
        }
        OnReturnPoolEvent?.Invoke(this);
    }
}