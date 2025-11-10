using UnityEngine;
using System.Collections;

public class DetectMonsterBaseController : WallMonsterBaseController<DetectMonsterData>,
    OnReturnPool<DetectMonsterBaseController>
{
    private OnReturnPoolEvent<DetectMonsterBaseController> OnReturnPoolEvent;

    [Header("Detect Settings")]
    [Tooltip("플레이어를 감지하는 시야 범위")]
    private Transform targetPlayer;

    private float detectInterval = 0.1f;
    private float detectTimer = 0f;
    private bool isDashing = false;

    // 돌진 공격을 위한 변수
    [Header("Lunge Settings")]
    [SerializeField] private float lungeDuration = 0.15f; // 목표 지점까지 돌진하는 시간
    [SerializeField] private float lungePause = 0.1f;     // 최대 거리에서 잠시 멈추는 시간
    [SerializeField] private float returnDuration = 0.25f; // 원래 위치로 돌아오는 시간

    public override void Setup(DetectMonsterData data)
    {
        base.Setup(data);
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
            StopMove();
            ChangeState(StateType.Attack);
        }
        else
        {
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
    }

    protected override void Attack()
    {
        if (isDashing) return;

        if (targetPlayer != null)
        {
            Vector2 targetPosition = targetPlayer.position;
            Vector2 startPosition = transform.position;
            Vector2 dashDir = (targetPosition - startPosition).normalized;

            Flip(dashDir.x < 0);

            StartCoroutine(LungeAndReturn(startPosition, targetPosition, dashDir));
        }
        else
        {
            ChangeState(StateType.Idle);
        }
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

        isDashing = false;
        ChangeState(StateType.Idle);
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