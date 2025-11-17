using UnityEngine;
using System.Collections;

public class DetectMonsterBaseController : WallMonsterBaseController<DetectMonsterData>,
    OnReturnPool<DetectMonsterBaseController>
{
    private OnReturnPoolEvent<DetectMonsterBaseController> OnReturnPoolEvent;

    [Header("Detect Settings")]
    private Transform targetPlayer;
    private Vector2? lastKnownPosition; // 플레이어의 마지막 위치 기억

    private float detectInterval = 0.1f;
    private float detectTimer = 0f;
    private bool isDashing = false;

    [Header("Lunge Settings")]
    [SerializeField] private float lungeDuration = 0.15f;
    [SerializeField] private float lungePause = 0.1f;
    [SerializeField] private float returnDuration = 0.25f;

    private const string ANIM_MOVE = "animation";
    private const string ANIM_IDLE = "idle";
    private const string ANIM_ATTACK = "Attack";

    public override void Setup(DetectMonsterData data)
    {
        base.Setup(data);
        lastKnownPosition = null;
    }

    private void FixedUpdate()
    {
        if (isDead || isDashing)
        {
            if (isDead) rigid.linearVelocity = Vector2.zero;
            return;
        }

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
            SetSpineAnimation(ANIM_MOVE, true);

            Vector2 dir = (lastKnownPosition.Value - (Vector2)transform.position).normalized;
            MoveCharacter(MonsterManager.Instance.GetAreaBound(), dir, null);

            if (Vector2.Distance(transform.position, lastKnownPosition.Value) < 0.1f)
            {
                lastKnownPosition = null;
            }
        }
        else
        {
            base.Move(Vector2.right);
        }
    }

    private void ChaseAndAttackPlayer()
    {
        float distance = Vector2.Distance(transform.position, targetPlayer.position);

        if (distance <= monsterData.AttackRange)
        {
            //SetSpineAnimation(ANIM_IDLE, true); // 정지 애니메이션
            SetSpineAnimation(ANIM_MOVE, true);
            StopMove();
            ChangeState(StateType.Attack);
        }
        else
        {
            SetSpineAnimation(ANIM_MOVE, true);
            Vector2 dir = (targetPlayer.position - transform.position).normalized;
            MoveCharacter(MonsterManager.Instance.GetAreaBound(), dir, null);
        }
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
        if (isDashing) return;

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
            ChangeState(StateType.Idle);
            return;
        }

        Vector2 startPosition = transform.position;
        Vector2 dashDir = (targetPosition - startPosition).normalized;

        Flip(dashDir.x < 0);

        //SetSpineAnimation(ANIM_ATTACK, false);
        SetSpineAnimation(ANIM_MOVE, true); // 이동 애니메이션

        StartCoroutine(LungeAndReturn(startPosition, targetPosition, dashDir));
    }

    private IEnumerator LungeAndReturn(Vector2 startPos, Vector2 targetPos, Vector2 dir)
    {
        isDashing = true;
        state = StateType.Attack;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, monsterData.DashSpeed * 0.2f, LayerMask.GetMask("Player"));
        if (hit.collider != null && hit.collider.CompareTag("Player1"))
        {
            hit.collider.GetComponent<Character>()?.TakeDamage(monsterData.dmg);
        }

        float elapsed = 0f;
        while (elapsed < lungeDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / lungeDuration;
            Vector2 newPos = Vector2.Lerp(startPos, targetPos, t);
            rigid.MovePosition(newPos);
            yield return new WaitForFixedUpdate();
        }
        rigid.MovePosition(targetPos);

        yield return new WaitForSeconds(lungePause);

        elapsed = 0f;
        Vector2 lungeEndPos = transform.position;
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

        SetSpineAnimation(ANIM_IDLE, true);
        //SetSpineAnimation(ANIM_MOVE, true); // 이동 애니메이션

        isDashing = false;
        ChangeState(StateType.Idle);
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