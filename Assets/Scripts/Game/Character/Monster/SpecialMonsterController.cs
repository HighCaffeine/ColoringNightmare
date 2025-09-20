using UnityEngine;

public class SpecialMonsterController : MonsterController, OnReturnPool<SpecialMonsterController>
{
    OnReturnPoolEvent<SpecialMonsterController> OnReturnPoolEvent;

    public float detectionRange = 5f;               // 탐지 범위
    private Transform targetPlayer;                // 현재 타겟 플레이어

    protected override void Move()
    {
        base.Move();

        Vector2 dir = Vector2.right;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player1"))
            {
                targetPlayer = hit.transform;
                break;
            }
        }

        if (targetPlayer != null)
        {
            dir = (targetPlayer.position - transform.position).normalized;

            float distance = Vector2.Distance(transform.position, targetPlayer.position);
            if (distance <= monsterData.AttackRange)
            {
                ChangeState(StateType.Attack);
                return;
            }
        }

        MoveCharacter(MonsterManager.Instance.GetAreaBound(), dir, null);
        Flip(dir.x > 0);
    }

    public void Init(OnReturnPoolEvent<SpecialMonsterController> onReturnPoolEvent)
    {
        OnReturnPoolEvent = onReturnPoolEvent;

        Flip(true);
    }

    protected override void Dead()
    {
        base.Dead();
        OnReturnPoolEvent?.Invoke(this);
    }
}
