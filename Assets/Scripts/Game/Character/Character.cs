using System;
using UnityEngine;

public enum StateType { None = -1, Idle, Attack, Move, Dead, Count }

[Serializable]
[CreateAssetMenu(menuName = "Character/CharacterInfo")]
public class CharacterData : ScriptableObject
{
    public Sprite sprite;
    [Header("기본 스텟")]
    public string characterName;
    public int maxHp = 5;
    public int dmg = 1;
    public float speed = 5.0f;
    public float attackDelay = 1.0f;

    [Space(5f)]
    [Header("Spine일 때만 체크해주세요")]
    public bool isSpine = false;
}

public class Character : MonoBehaviour
{
    [Header("현재 상태")]
    public CharacterData info;
    protected StateType state;
    [SerializeField] protected int currentHP;

    protected bool isDead => currentHP <= 0;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigid;
    public Spine.Unity.SkeletonAnimation skeleton;

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
    }

    protected virtual void Idle() { Debug.Log($"[State] Idle : {info.characterName}"); }
    protected virtual void Attack() { Debug.Log($"[State] Attack : {info.characterName}"); }
    protected virtual void Dead() { Debug.Log($"[State] Dead : {info.characterName}"); }
    protected virtual void Move() { Debug.Log($"[State] Move : {info.characterName}"); }

    protected void ChangeState(StateType newState)
    {
        if (state == newState) return;

        state = newState;

        switch (state)
        {
            case StateType.Idle: Idle(); break;
            case StateType.Move: Move(); break;
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

            FlipEffectPivot(isRight);
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.flipX = isRight;
        }
    }

    protected virtual void TakeDamage(int amount)
    {
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
        if (dir == Vector2.zero) return;

        Vector2 spriteHalfSize = Vector2.zero;
        SpriteRenderer render = GetComponent<SpriteRenderer>();
        if (render != null) spriteHalfSize = render.bounds.extents;

        Vector2 newPos = (Vector2)transform.position + dir * info.speed * Time.deltaTime;

        if (!area.Contains(newPos + spriteHalfSize))
        {
            rigid.linearVelocity = Vector2.zero;
            return;
        }

        rigid.linearVelocity = dir * info.speed;
        OnMoveAction?.Invoke(newPos);

        if (dir.x != 0) Flip(dir.x > 0);
    }
}
