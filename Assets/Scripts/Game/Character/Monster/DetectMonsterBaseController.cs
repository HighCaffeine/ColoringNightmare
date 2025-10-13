using UnityEngine;
using System.Collections;
using System;

public class DetectMonsterBaseController : WallMonsterBaseController<DetectMonsterData>,
    OnReturnPool<DetectMonsterBaseController>
{
    OnReturnPoolEvent<DetectMonsterBaseController> OnReturnPoolEvent;

    [Header("Detect Settings")]
    public float detectionRange = 5f;
    [SerializeField] private Transform targetPlayer;
    private float detectInterval = 0.1f;
    [SerializeField] private float detectTimer = 0f;

    private bool isDashing = false;

    MonsterManager.OnPlayerIsDead onPlayerIsDead;

    public override void Setup(DetectMonsterData data)
    {
        base.Setup(data);
        detectionRange = data.AttackRange;
        onPlayerIsDead += MonsterManager.Instance.IsPlayerDead;
        rigid = GetComponent<Rigidbody2D>();
    }

    // FixedUpdate에서 Dash 중 velocity 덮어쓰지 않도록 처리
    private void FixedUpdate()
    {
        if (isDead || isDashing)
        {
            rigid.linearVelocity = Vector2.zero;
            return;
        }

        switch (state)
        {
            case StateType.Idle:
                IdleAndDetect();
                break;
            case StateType.Move:
                ChasePlayer();
                break;
        }
    }

    private void Update()
    {
        detectTimer += Time.deltaTime;
        if (detectTimer >= detectInterval)
        {
            detectTimer = 0f;
            DetectPlayer();
        }
    }

    protected void IdleAndDetect()
    {
        base.Move(Vector2.right);

        if (targetPlayer != null)
        {
            ChangeState(StateType.Move);
        }
    }

    private void ChasePlayer()
    {
        if (targetPlayer == null)
        {
            ChangeState(StateType.Idle);
            return;
        }

        float distance = Vector2.Distance(transform.position, targetPlayer.position);
        if (distance <= monsterData.AttackRange)
        {
            ChangeState(StateType.Attack);
            return;
        }

        Vector2 dir = (targetPlayer.position - transform.position).normalized;
        MoveCharacter(MonsterManager.Instance.GetAreaBound(), dir, null);
        Flip(dir.x > 0);
    }

    private void DetectPlayer()
    {
        if (onPlayerIsDead?.Invoke() == true)
        {
            detectTimer = 0.0f;
            targetPlayer = null;
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        targetPlayer = null;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] != null && hits[i].CompareTag("Player1"))
            {
                targetPlayer = hits[i].transform;
                break;
            }
        }
    }

    protected override void Attack()
    {
        base.Attack();
        if (isDashing) return;

        if (targetPlayer != null)
        {
            Vector2 dashDir = (targetPlayer.position - transform.position).normalized;
            StartCoroutine(DashTowardsTarget(dashDir));
        }
        else
        {
            ChangeState(StateType.Idle);
        }
    }

    private IEnumerator DashTowardsTarget(Vector2 dir)
    {
        isDashing = true;

        // Dash 중 다른 Move 충돌 방지
        rigid.linearVelocity = Vector2.zero;

        // Dash 전용 Layer로 변경 (옵션)
        int originalLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("Dash");

        float dashTime = 0.2f;
        float elapsed = 0f;

        // Raycast로 Dash 경로 체크
        Vector2 startPos = transform.position;
        Vector2 endPos = startPos + dir * statusEffectManager.GetSpeedMultiplier() * dashTime;

        while (elapsed <= dashTime)
        {
            elapsed += Time.deltaTime;

            // Dash 이동
            rigid.linearVelocity = dir * statusEffectManager.GetSpeedMultiplier();

            // Raycast로 경로상 플레이어 감지
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, monsterData.DashSpeed * Time.deltaTime, LayerMask.GetMask("Player"));
            if (hit.collider != null)
            {
                Character player = hit.collider.GetComponent<Character>();
                if (player != null)
                    player.TakeDamage(monsterData.dmg);
            }

            yield return null;
        }

        rigid.linearVelocity = Vector2.zero;
        gameObject.layer = originalLayer;
        isDashing = false;

        DetectPlayer();

        if (targetPlayer != null)
            ChangeState(StateType.Move);
        else
        {
            detectTimer = 0.0f;
            ChangeState(StateType.Idle);
        }
    }

    public void Init(OnReturnPoolEvent<DetectMonsterBaseController> onReturnPoolEvent)
    {
        OnReturnPoolEvent = onReturnPoolEvent;
        Flip(true);
    }

    protected override void Dead()
    {
        base.Dead();
        OnReturnPoolEvent?.Invoke(this);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
#endif
}
