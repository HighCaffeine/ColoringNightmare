using UnityEngine;
using System.Collections;

public class BossMonsterController : Character, OnReturnPool<BossMonsterController>
{
    [SerializeField] private BossMonsterData bossData;
    private Transform targetPlayer;
    [SerializeField] private SpriteRenderer spriteRender;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private Animator ani;

    public OnReturnPoolEvent<BossMonsterController> OnReturnPoolEvent;


    private float skillTimer = 0f;

    public void Setup(BossMonsterData data)
    {
        bossData = data;
        info = data;
        currentHP = info.maxHp;
        state = StateType.Idle;
    }

    void Update()
    {
        if (isDead) return;

        switch (state)
        {
            case StateType.Idle: Idle(); break;
            case StateType.Move: Move(); break;
            case StateType.Attack: Attack(); break;
            case StateType.Dead: Dead(); break;
            case StateType.Skill: Skill(); break;
        }
    }

    protected new void Idle()
    {
        // 대기
        skillTimer += Time.deltaTime;

        if (skillTimer >= bossData.skillCooldown)
        {
            skillTimer = 0f;
            ChangeState(StateType.Skill);
            return;
        }

        ChangeState(StateType.Move);
    }

    protected void Move()
    {
        // 플레이어 방향으로 이동
        if (targetPlayer == null) return;

        Vector2 dir = (targetPlayer.position - transform.position).normalized;
        transform.position += (Vector3)dir * bossData.speed * Time.deltaTime;

        // 공격 범위 도달시 Attack
        if (Vector2.Distance(transform.position, targetPlayer.position) <= bossData.attackRange)
        {
            ChangeState(StateType.Attack);
        }
    }

    protected new void Attack()
    {
        // 단일 공격 로직


        // 공격 후 Idle 복귀
        ChangeState(StateType.Idle);
    }

    protected void Skill()
    {
        // 보스 패턴/스킬 실행



        // 스킬 후 Idle로 복귀
        ChangeState(StateType.Idle);
    }

    protected override void Dead()
    {
        Debug.Log($"{info.characterName} is dead!");
        OnReturnPoolEvent?.Invoke(this);
    }

    public void Init(OnReturnPoolEvent<BossMonsterController> onReturnPoolEvent)
    {
        OnReturnPoolEvent = onReturnPoolEvent;
    }
}
