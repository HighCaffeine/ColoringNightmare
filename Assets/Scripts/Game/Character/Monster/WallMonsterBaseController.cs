using UnityEngine;
using System.Collections;
using System;

public class WallMonsterBaseController<T> : Character, OnReturnPool<WallMonsterBaseController<T>>
    where T : MonsterData
{
    OnReturnPoolEvent<WallMonsterBaseController<T>> OnReturnPoolEvent;

    [SerializeField] private SpriteRenderer spriteRender;
    //[SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private PolygonCollider2D polygonCollider;
    [SerializeField] private Animator ani;

    protected T monsterData;
    protected float idleTime = 1f;
    protected float idleTimer = 0f;

    protected bool isAttacking = false;

    // void UpdateCollider()
    // {
    //     if (spriteRender == null) spriteRender = GetComponent<SpriteRenderer>();
    //     if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
    //     if (spriteRender.sprite == null) return;

    //     Bounds spriteBounds = spriteRender.sprite.bounds;
    //     Vector3 scale = transform.localScale;
    //     boxCollider.size = new Vector2(
    //         spriteBounds.size.x * scale.x,
    //         spriteBounds.size.y * scale.y
    //     );
    //     boxCollider.offset = spriteBounds.center;
    // }

    public virtual void Setup(T data)
    {
        spriteRender.sprite = data.sprite;
        UpdatePolygonCollider();

        monsterData = data;
        info = data;
        currentHP = info.maxHp;
        state = StateType.Idle;
        transform.position = MonsterManager.Instance.GetCalibrationSpawnPos(transform, spriteRender);
        //UpdateCollider();
        Flip(true);
    }

    private void UpdatePolygonCollider()
    {
        if (polygonCollider == null || spriteRender.sprite == null) return;

        polygonCollider.pathCount = 0;
        polygonCollider.pathCount = spriteRender.sprite.GetPhysicsShapeCount();

        System.Collections.Generic.List<Vector2> path = new System.Collections.Generic.List<Vector2>();
        for (int i = 0; i < polygonCollider.pathCount; i++)
        {
            path.Clear();
            spriteRender.sprite.GetPhysicsShape(i, path);
            polygonCollider.SetPath(i, path.ToArray());
        }
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
        StopMove();
    }

    protected override void Move(Vector2 dir)
    {
        MoveCharacter(MonsterManager.Instance.GetAreaBound(), dir, null);

        Vector2 spriteHalfSize = Vector2.zero;
        if (spriteRender != null) spriteHalfSize = spriteRender.bounds.extents;

        var bounds = MonsterManager.Instance.GetAreaBound();
        if (transform.position.x + spriteHalfSize.x >= bounds.max.x - 0.1f)
        {
            StartCoroutine(SelfDestructCoroutine(2f));
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player1"))
        {
            PlayerController player = other.transform.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(monsterData.dmg);
            }
        }
    }

    private IEnumerator SelfDestructCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ani != null) ani.SetTrigger("Explode");
        MonsterManager.Instance.SubWorldHpEvent();
        ChangeState(StateType.Dead);
    }

    protected override void Attack()
    {
        base.Attack();
        if (isAttacking || !CanAttack()) return;
        StartCoroutine(WallAttackCoroutine());
    }

    private IEnumerator WallAttackCoroutine()
    {
        isAttacking = true;
        if (ani != null) ani.SetTrigger("Attack");
        float attackDuration = 0.3f;
        yield return new WaitForSeconds(attackDuration);
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
}