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
            Vector2 dashDir = (targetPlayer.position - transform.position).normalized;
            Flip(dashDir.x < 0);
            StartCoroutine(DashTowardsTarget(dashDir));
        }
        else
        {
            ChangeState(StateType.Idle);
        }
    }

    private IEnumerator DashTowardsTarget(Vector2 dir)
    {
        isDashing = true;
        state = StateType.Attack;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, monsterData.DashSpeed * 0.2f, LayerMask.GetMask("Player"));
        if (hit.collider != null && hit.collider.CompareTag("Player1"))
        {
            hit.collider.GetComponent<Character>()?.TakeDamage(monsterData.dmg);
        }

        rigid.linearVelocity = dir * monsterData.DashSpeed;
        yield return new WaitForSeconds(0.2f);
        rigid.linearVelocity = Vector2.zero;

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