using System;
using UnityEngine;

public enum StateType { None = -1, Idle, Attack, Move, Dead, Skill, Count }

public class Character : MonoBehaviour
{
    [Header("현재 상태")]
    public CharacterData info;
    protected StateType state;
    [SerializeField] protected int currentHP;

    protected bool isDead => currentHP <= 0;

    private SpriteRenderer spriteRenderer;
    protected Rigidbody2D rigid;
    private BoxCollider2D boxCollider;
    public Spine.Unity.SkeletonAnimation skeleton;

    protected StatusEffectManager statusEffectManager;

    private bool canAttack = true;
    private float attackTimer = 0f;

    private bool isDamageImmune = false;

    protected virtual void Awake()
    {
        //currentHP = info.maxHp;
        state = StateType.Idle;
        statusEffectManager = GetComponent<StatusEffectManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        rigid = GetComponent<Rigidbody2D>();
    }

    [Header("Effect Rotate Pivot")]
    [SerializeField] private Transform effectPivot;

    private void FlipEffectPivot(bool isRight)
    {
        Vector3 s = effectPivot.transform.localScale;
        s.x = isRight ? -1 : 1;

        effectPivot.transform.localScale = s;
    }

    protected void OnCharacterDataLoaded(CharacterData data)
    {
        info = data;
        currentHP = info.maxHp;
        Debug.Log($"[Character] Loaded {info.characterName}");
    }

    protected virtual void Idle() { Debug.Log($"[State] Idle : {info.characterName}"); }
    protected virtual void Dead() { Debug.Log($"[State] Dead : {info.characterName}"); }
    protected virtual void Move(Vector2 dir) { Debug.Log($"[State] Move : {info.characterName}"); }

    protected void SetDamageImmune(bool isImmune) { isDamageImmune = isImmune; }
    protected virtual void Attack()
    {
        if (!canAttack) return;

        canAttack = false;
        Invoke(nameof(InitCanAttack), info.attackDelay);
    }
    private void InitCanAttack() { canAttack = true; }
    protected bool CanAttack() { return canAttack; }
    protected bool IsAllowAttack() { return canAttack; }

    protected void ChangeState(StateType newState)
    {
        if (state == newState) return;

        state = newState;

        switch (state)
        {
            case StateType.Idle: Idle(); break;
            case StateType.Move: Move(Vector2.right); break;
            case StateType.Attack: Attack(); break;
            case StateType.Dead: Dead(); break;
        }
    }


    protected virtual void Flip(bool isRight)
    {
        if (info != null && info.isSpine && skeleton != null)
        {
            float currentScaleX = skeleton.skeleton.ScaleX;
            float newScaleX = Mathf.Abs(currentScaleX) * (isRight ? -1f : 1f);
            skeleton.skeleton.ScaleX = newScaleX;

            if (effectPivot != null) FlipEffectPivot(isRight);
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.flipX = isRight;
        }
    }

    public virtual void TakeDamage(int amount)
    {
        if (isDamageImmune) return;

        Debug.Log($"[TakeDamage]{transform.name} : {amount} {currentHP}");

        currentHP -= amount;

        if (currentHP <= 0)
        {
            currentHP = 0;
            ChangeState(StateType.Dead);
        }
    }

    protected virtual void Interactive() { }

    protected void StopMove()
    {
        rigid.linearVelocity = Vector2.zero;
    }
    // MoveCharacter 메서드 수정 (이전 코드에서 호출되던 부분)
    protected void MoveCharacter(AreaData moveArea, Vector2 dir, System.Action<Vector2> onMove)
    {
        if (rigid == null) return;

        Vector2 movement = dir.normalized * statusEffectManager.GetSpeedMultiplier();
        rigid.linearVelocity = movement;
        onMove?.Invoke(transform.position);

        // 경계 처리
        if (moveArea != null)
        {
            // 경계 내에 있도록 위치 조정
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, moveArea.GetBounds().min.x, moveArea.GetBounds().max.x);
            pos.y = Mathf.Clamp(pos.y, moveArea.GetBounds().min.y, moveArea.GetBounds().max.y);
            transform.position = pos;
        }
    }

    protected virtual void MoveCharacter(Bounds area, Vector2 dir, Action<Vector2> OnMoveAction)
    {
        Vector2 currentPos = (Vector2)transform.position;
        Bounds characterBounds = new Bounds(currentPos, boxCollider.bounds.extents * 2);
        if (!area.Intersects(characterBounds))
        {
            rigid.linearVelocity = Vector2.zero;
        }

        Bounds bounds = area;
        float clampedX = Mathf.Clamp(transform.position.x, bounds.min.x, bounds.max.x);
        float clampedY = Mathf.Clamp(transform.position.y, bounds.min.y, bounds.max.y);
        transform.position = new Vector2(clampedX, clampedY);

        if (dir == Vector2.zero)
        {
            rigid.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 moveVelocity = dir.normalized * statusEffectManager.GetSpeedMultiplier();
        rigid.linearVelocity = moveVelocity;

        if (OnMoveAction != null)
        {
            OnMoveAction.Invoke(transform.position);
        }
        if (dir.x != 0) Flip(dir.x > 0);
    }
    public void ApplyStatusEffect(IStatusEffect effect)
    {
        if (statusEffectManager != null)
        {
            statusEffectManager.ApplyEffect(effect);
        }
    }
    public float GetSpeed()
    {
        float speedMultiplier = 1.0f;
        if (statusEffectManager != null)
        {
            speedMultiplier = statusEffectManager.GetSpeedMultiplier();
        }
        return info.speed * speedMultiplier;
    }
}
