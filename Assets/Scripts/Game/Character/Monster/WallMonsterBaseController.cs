using UnityEngine;
using System.Collections;
using System;

public class WallMonsterBaseController<T> : Character, OnReturnPool<WallMonsterBaseController<T>>
    where T : MonsterData
{
    OnReturnPoolEvent<WallMonsterBaseController<T>> OnReturnPoolEvent;

    [SerializeField] private Animator ani;

    protected T monsterData;
    protected float idleTime = 1f;
    protected float idleTimer = 0f;

    protected bool isAttacking = false;

    private float damageCooldown = 1.0f;
    private float lastDamageTime = -1.0f;

    public virtual void Setup(T data)
    {
        if (!data.isSpine)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = data.sprite;
                UpdatePolygonCollider();
            }
        }


        monsterData = data;
        info = data;
        currentHP = info.maxHp;
        state = StateType.Idle;

        transform.position = MonsterManager.Instance.GetCalibrationSpawnPos(transform, characterCollider);

        Flip(false);
    }

    private void UpdatePolygonCollider()
    {
        if (characterCollider == null || spriteRenderer == null || spriteRenderer.sprite == null) return;

        PolygonCollider2D polygonCollider = characterCollider as PolygonCollider2D;
        if (polygonCollider == null) return;

        polygonCollider.pathCount = 0;
        polygonCollider.pathCount = spriteRenderer.sprite.GetPhysicsShapeCount();

        System.Collections.Generic.List<Vector2> path = new System.Collections.Generic.List<Vector2>();
        for (int i = 0; i < polygonCollider.pathCount; i++)
        {
            path.Clear();
            spriteRenderer.sprite.GetPhysicsShape(i, path);
            polygonCollider.SetPath(i, path.ToArray());
        }
    }

    void Update()
    {
        HandleUpdate();
    }

    protected virtual void HandleUpdate()
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

        if (skeleton != null && skeleton.AnimationName != "animation")
        {
            skeleton.AnimationState.SetAnimation(0, "animation", true);
        }

        Vector2 spriteHalfSize = (characterCollider != null) ? characterCollider.bounds.extents : Vector2.zero;

        var bounds = MonsterManager.Instance.GetAreaBound();
        if (transform.position.x + spriteHalfSize.x >= bounds.max.x - 0.1f)
        {
            if (skeleton != null && skeleton.AnimationName != "animation")
            {
                skeleton.AnimationState.ClearTrack(0);
            }
            StartCoroutine(SelfDestructCoroutine(2f));
        }
    }


    void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < lastDamageTime + damageCooldown)
        {
            return;
        }

        if (other.gameObject.CompareTag("Player1"))
        {
            PlayerController player = other.transform.GetComponent<PlayerController>();
            if (player != null)
            {
                lastDamageTime = Time.time;
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
        MonsterManager.Instance.NotifyMonsterDeath(this, monsterData);
        OnReturnPoolEvent?.Invoke(this);
    }

    public void Init(OnReturnPoolEvent<WallMonsterBaseController<T>> onReturnPoolEvent)
    {
        OnReturnPoolEvent = onReturnPoolEvent;
        Flip(true);
    }
}