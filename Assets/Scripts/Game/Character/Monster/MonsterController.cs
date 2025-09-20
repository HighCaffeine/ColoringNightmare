using UnityEngine;
using UnityEngine.EventSystems;

public class MonsterController : Character, OnReturnPool<MonsterController>
{
    OnReturnPoolEvent<MonsterController> OnReturnPoolEvent;
    [SerializeField] private SpriteRenderer spriteRender;

    protected MonsterData monsterData;
    protected float idleTime = 1f;
    protected float idleTimer = 0f;
    public virtual void Setup(MonsterData data)
    {
        spriteRender.sprite = data.sprite;
        monsterData = data;
        info = data;
        currentHP = info.maxHp;
        state = StateType.Idle;

        Flip(true);
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
        }
    }

    public void Init(OnReturnPoolEvent<MonsterController> onReturnPoolEvent)
    {
        OnReturnPoolEvent = onReturnPoolEvent;

        Flip(true);
    }

    protected override void Idle()
    {
        base.Idle();
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTime)
        {
            idleTimer = 0f;
            ChangeState(StateType.Move);
        }
    }

    protected override void Move()
    {
        base.Move();

        Vector2 dir = Vector2.right;
        MoveCharacter(MonsterManager.Instance.GetAreaBound(), dir, null);
    }

    protected override void Attack()
    {
        base.Attack();
        Debug.Log($"{info.characterName} hits wall and explodes!");

    }

    protected override void Dead()
    {
        base.Dead();
        OnReturnPoolEvent?.Invoke(this);
    }
}
