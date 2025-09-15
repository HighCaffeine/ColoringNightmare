using System;
using UnityEngine;

public enum StateType
{
    None = -1,
    Idle,
    Attack,
    Move,
    Dead,
    //Interactive,
    Count,
}

[CreateAssetMenu(menuName = "Character/CharacterInfo")]
public class CharacterData : ScriptableObject
{
    public Sprite sprite;
    [Header("기본 스텟")]
    public string characterName;
    public int maxHp = 5;
    public int dmg = 1;
    public float speed = 5.0f;
    public int def = 0;

    [Space(5f)]
    [Header("Spine일 때만 체크해주세요")]
    public bool isSpine = false;
}

public class Character : MonoBehaviour
{
    [Header("현재 상태")]
    public CharacterData info;
    protected StateType state;
    protected int currentHP;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigid;

    protected virtual void Awake()
    {
        currentHP = info.maxHp;
        state = StateType.Idle;

        spriteRenderer = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
    }

    protected virtual void Idle() { Debug.Log($"[State] Idle : {info.name}"); }
    protected virtual void Attack() { Debug.Log($"[State] Attack : {info.characterName}"); }
    protected virtual void Dead() { Debug.Log($"[State] Dead : {info.characterName}"); }
    protected virtual void Move() { Debug.Log($"[State] Move : {info.characterName}"); }

    protected void ChangeState(StateType newState)
    {
        if (state == newState) return;

        Debug.Log($"{info.characterName} Change State: {state} -> {newState}");
        state = newState;

        switch (state)
        {
            case StateType.Idle:
                Idle();
                break;
            case StateType.Attack:
                Attack();
                break;
            case StateType.Move:
                break;
            case StateType.Dead:
                Dead();
                break;
        }
    }
    protected virtual void Flip(bool isRight)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = isRight;
        }
        else
        {
            //spine은 mesh기반
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

    protected virtual void MoveCharacter(AreaData area, Vector2 dir, Action<Vector2> OnMoveAction)
    {
        if (dir == Vector2.zero) return;

        Vector2 spriteHalfSize = Vector2.zero;
        SpriteRenderer render = GetComponent<SpriteRenderer>();
        if (render != null)
        {
            spriteHalfSize = render.bounds.extents;
        }

        Vector2 newPos = (Vector2)transform.position + dir * info.speed * Time.deltaTime;

        var areaBounds = area.GetBounds();
        if (!areaBounds.Contains(newPos + spriteHalfSize) || !areaBounds.Contains(newPos - spriteHalfSize))
        {
            rigid.linearVelocity = Vector2.zero;
            return;
        }

        rigid.linearVelocity = dir * info.speed;

        OnMoveAction?.Invoke(newPos);

        if (dir.x != 0) Flip(dir.x > 0);
    }
}