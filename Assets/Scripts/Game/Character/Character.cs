using System;
using UnityEngine;
using System.Collections;
using Spine.Unity;

public enum StateType { None = -1, Idle, Attack, Move, Dead, Skill, Groggy, Count }

public class Character : MonoBehaviour
{
    [Header("현재 상태")]
    public CharacterData info;
    protected StateType state;
    [SerializeField] protected int currentHP;
    public int CurrentHP => currentHP;
    protected bool isDead => currentHP <= 0;

    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rigid;
    public Spine.Unity.SkeletonAnimation skeleton;
    protected StatusEffectManager statusEffectManager;
    protected EffectController effectController;

    protected Collider2D characterCollider;
    private bool isDamageImmune = false;
    private Coroutine hitEffectCoroutine;

    public bool FlipX() => spriteRenderer.flipX;
    public bool IsExistsSprite() => spriteRenderer != null;

    protected virtual void Awake()
    {
        state = StateType.Idle;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
        statusEffectManager = GetComponent<StatusEffectManager>();
        effectController = GetComponent<EffectController>();
        characterCollider = GetComponent<Collider2D>();
        if (statusEffectManager == null)
        {
            statusEffectManager = gameObject.AddComponent<StatusEffectManager>();
        }
    }

    protected void OnCharacterDataLoaded(CharacterData data)
    {
        info = data;
        currentHP = info.maxHp;
    }

    protected virtual void Idle() { }
    protected virtual void Dead() { }
    protected virtual void Move(Vector2 dir) { }

    protected void SetDamageImmune(bool isImmune) { isDamageImmune = isImmune; }

    protected virtual void Attack() { }
    private void InitCanAttack() { }
    protected bool CanAttack() { return true; }

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
            skeleton.skeleton.ScaleX = isRight ? -1f : 1f;
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !isRight;
        }
    }

    public virtual void TakeDamage(int amount)
    {
        TakeDamage(amount, null);
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > info.maxHp) currentHP = info.maxHp;
    }

    public virtual void TakeDamage(int amount, EffectVisualData hitEffect)
    {
        if (isDamageImmune || isDead) return;
        SoundManager.Instance.PlaySound(SoundManager.Effect.SFX_Weapon_Damaged_Yellow.ToString(), false);
        currentHP -= amount;

        if (hitEffectCoroutine != null) StopCoroutine(hitEffectCoroutine);

        hitEffectCoroutine = StartCoroutine(HitEffectCoroutine(hitEffect));

        if (currentHP <= 0)
        {
            currentHP = 0;
            ChangeState(StateType.Dead);
        }
    }

    private IEnumerator HitEffectCoroutine(EffectVisualData hitEffect)
    {
        if (effectController != null && hitEffect != null)
        {
            Vector3 hitPosition = (characterCollider != null) ? characterCollider.bounds.center : transform.position;

            bool isFacingLeft = true;
            if (skeleton != null && skeleton.skeleton != null)
            {
                isFacingLeft = skeleton.skeleton.ScaleX > 0;
            }
            else if (spriteRenderer != null)
            {
                isFacingLeft = !spriteRenderer.flipX;
            }

            effectController.PlayHitEffectAt(hitPosition, hitEffect, isFacingLeft);
        }

        // 빨간색 점멸 효과
        Color hitColor = Color.red;
        Color originalColor = Color.white;
        float duration = 0.1f;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(duration);
            spriteRenderer.color = originalColor;
        }
        else if (skeleton != null && skeleton.skeleton != null)
        {
            skeleton.skeleton.SetColor(hitColor);
            yield return new WaitForSeconds(duration);
            skeleton.skeleton.SetColor(originalColor);
        }
    }

    public void ApplyStatusEffect(IStatusEffect effect)
    {
        statusEffectManager?.ApplyEffect(effect);
    }

    protected void StopMove()
    {
        if (rigid != null) rigid.linearVelocity = Vector2.zero;
    }

    protected virtual void MoveCharacter(Bounds area, Vector2 dir, Action<Vector2> OnMoveAction)
    {
        if (dir == Vector2.zero)
        {
            StopMove();
            return;
        }

        float speedMultiplier = statusEffectManager?.GetSpeedMultiplier() ?? 1.0f;
        float finalSpeed = info.speed * speedMultiplier * Time.deltaTime * 50;

        Vector2 newPosition = rigid.position + dir.normalized * finalSpeed * Time.fixedDeltaTime;

        Vector2 characterExtents = GetComponent<Collider2D>()?.bounds.extents ?? Vector2.zero;
        newPosition.x = Mathf.Clamp(newPosition.x, area.min.x + characterExtents.x, area.max.x - characterExtents.x);
        newPosition.y = Mathf.Clamp(newPosition.y, area.min.y + characterExtents.y, area.max.y - characterExtents.y);

        rigid.MovePosition(newPosition);

        OnMoveAction?.Invoke(transform.position);

        if (dir.x != 0) Flip(dir.x < 0);
    }
}