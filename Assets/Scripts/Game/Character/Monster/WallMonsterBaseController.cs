using UnityEngine;
using System.Collections;

public class WallMonsterBaseController<T> : Character, OnReturnPool<WallMonsterBaseController<T>>
    where T : MonsterData
{
    OnReturnPoolEvent<WallMonsterBaseController<T>> OnReturnPoolEvent;

    [SerializeField] private SpriteRenderer spriteRender;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private Animator ani;

    protected T monsterData;
    protected float idleTime = 1f;
    protected float idleTimer = 0f;

    protected bool isAttacking = false;

    void UpdateCollider()
    {
        if (spriteRender == null) spriteRender = GetComponent<SpriteRenderer>();
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();

        Bounds spriteBounds = spriteRender.sprite.bounds;

        Vector3 scale = transform.localScale;
        boxCollider.size = new Vector2(
            spriteBounds.size.x * scale.x,
            spriteBounds.size.y * scale.y
        );
        boxCollider.offset = spriteBounds.center;
    }

    public virtual void Setup(T data)
    {
        spriteRender.sprite = data.sprite;
        monsterData = data;
        info = data;
        currentHP = info.maxHp;
        state = StateType.Idle;

        UpdateCollider();
        Flip(true);
    }

    void Update()
    {
        if (isDead) return;

        switch (state)
        {
            case StateType.Idle: Idle(); break;
            case StateType.Move: Move(Vector2.right); break;
            case StateType.Attack: Attack(); break;
            case StateType.Dead: Dead(); break;
        }
    }

    protected override void Idle()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTime)
        {
            idleTimer = 0f;
            ChangeState(StateType.Move);
        }
    }

    protected new virtual void Move(Vector2 dir)
    {
        // 이동
        MoveCharacter(MonsterManager.Instance.GetAreaBound(), dir, null);
        Flip(dir.x > 0);

        // 오른쪽 bound 끝 체크
        var bounds = MonsterManager.Instance.GetAreaBound();
        if (transform.position.x >= bounds.max.x)
        {
            ChangeState(StateType.Attack);
        }
    }

    protected override void Attack()
    {
        if (isAttacking) return;

        StartCoroutine(WallAttackCoroutine());
    }

    private IEnumerator WallAttackCoroutine()
    {
        isAttacking = true;

        // 애니메이션 트리거 (todo)
        if (ani != null) ani.SetTrigger("Attack");

        // 공격 효과 처리 todo

        // 공격 시간 대기
        float attackDuration = 0.3f;
        yield return new WaitForSeconds(attackDuration);

        // 공격 후 Dead 상태로 전환
        isAttacking = false;
        ChangeState(StateType.Dead);
    }

    protected override void Dead()
    {
        base.Dead();
        OnReturnPoolEvent?.Invoke(this);
    }

    public void Init(OnReturnPoolEvent<WallMonsterBaseController<T>> onReturnPoolEvent)
    {
        OnReturnPoolEvent = onReturnPoolEvent;
        Flip(true);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (spriteRender != null)
        {
            Gizmos.color = Color.blue;
            Bounds bounds = MonsterManager.Instance.GetAreaBound();
            Gizmos.DrawLine(new Vector3(bounds.max.x, bounds.min.y, 0),
                            new Vector3(bounds.max.x, bounds.max.y, 0));
        }
    }
#endif
}
