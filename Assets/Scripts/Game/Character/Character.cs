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
    public Spine.Unity.SkeletonAnimation skeleton;

    private bool canAttack = true;
    private float attackTimer = 0f;

    private bool isDamageImmune = false;

    protected virtual void Awake()
    {
        //currentHP = info.maxHp;
        state = StateType.Idle;

        spriteRenderer = GetComponent<SpriteRenderer>();
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

    protected virtual void MoveCharacter(Bounds area, Vector2 dir, Action<Vector2> OnMoveAction)
    {
        if (dir == Vector2.zero)
        {
            rigid.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 moveVelocity = dir.normalized * info.speed;
        rigid.linearVelocity = moveVelocity;

        if (OnMoveAction != null)
        {
            OnMoveAction.Invoke(transform.position);
        }

        Vector2 spriteHalfSize = Vector2.zero;
        SpriteRenderer render = GetComponent<SpriteRenderer>();
        if (render != null)
        {
            spriteHalfSize = render.bounds.extents;
        }

        Vector2 currentPos = (Vector2)transform.position;
        Bounds characterBounds = new Bounds(currentPos, spriteHalfSize * 2);

        if (!area.Intersects(characterBounds))
        {
            rigid.linearVelocity = Vector2.zero;
        }

        if (dir.x != 0) Flip(dir.x > 0);
    }
}
