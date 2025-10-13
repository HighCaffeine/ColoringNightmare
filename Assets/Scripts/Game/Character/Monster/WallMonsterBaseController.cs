using UnityEngine;
using System.Collections;
using System;

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

    private float speed = 1.0f;

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

        transform.position = MonsterManager.Instance.GetCalibrationSpawnPos(transform, spriteRender);

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
        // Idle 상태에서도 RigidBody 속도를 0으로 설정
        StopMove();
    }

    protected new virtual void Move(Vector2 dir)
    {
        // Character.MoveCharacter를 호출하여 둔화 효과와 경계 처리를 모두 적용합니다.
        MoveCharacter(MonsterManager.Instance.GetAreaBound(), dir, null);
        Flip(dir.x > 0);

        // MoveCharacter에서 이미 속도가 적용되므로 여기서는 추가적인 속도 계산을 하지 않습니다.

        // 경계 체크 로직: 오른쪽 경계에 닿으면 자폭
        Vector2 spriteHalfSize = Vector2.zero;
        SpriteRenderer render = GetComponent<SpriteRenderer>();
        if (render != null) spriteHalfSize = render.bounds.extents;

        // 현재 위치를 기준으로 다음 위치를 예측하는 대신, 현재 위치가 경계 근처인지 확인
        var bounds = MonsterManager.Instance.GetAreaBound();
        if (transform.position.x + spriteHalfSize.x >= bounds.max.x - 0.1f) // 0.1f는 오차 범위
        {
            StartCoroutine(SelfDestructCoroutine(2f)); // 2초 후 폭발
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("WallMonster : OntriggerEnter2D");
        if (collision.gameObject.CompareTag("Player1"))
        {
            PlayerController player = collision.transform.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(monsterData.dmg);

                Vector2 pushDirection = (collision.transform.position - transform.position).normalized;

                Rigidbody2D playerRb = collision.transform.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.AddForce(pushDirection * monsterData.PushForce, ForceMode2D.Impulse);
                }
            }
        }
    }


    private IEnumerator SelfDestructCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        //폭발 이펙트 추가 필요
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
