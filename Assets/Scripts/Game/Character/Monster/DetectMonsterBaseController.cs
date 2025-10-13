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

    private void FixedUpdate()
    {
        if (isDead) // 대시 중 velocity는 그대로 유지
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
        Vector2 targetPos;

        if (targetPlayer != null)
        {
            targetPos = targetPlayer.position;
            lastKnownPlayerPos = targetPos;
        }
        else if (lastKnownPlayerPos != null)
        {
            targetPos = lastKnownPlayerPos.Value;
        }
        else
        {
            ChangeState(StateType.Idle);
            return;
        }

        float distance = Vector2.Distance(transform.position, targetPos);
        if (distance <= monsterData.AttackRange)
        {
            ChangeState(StateType.Attack);
            return;
        }

        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        MoveCharacter(MonsterManager.Instance.GetAreaBound(), dir, null);
        Flip(dir.x > 0);

        // 마지막 위치에 도착하면 targetPlayer null이면 Idle로
        if (targetPlayer == null && distance <= 0.1f)
        {
            ChangeState(StateType.Idle);
            lastKnownPlayerPos = null;
        }
    }

    private Vector2? lastKnownPlayerPos = null;

    private void DetectPlayer()
    {
        if (onPlayerIsDead?.Invoke() == true)
        {
            detectTimer = 0.0f;
            targetPlayer = null;
            lastKnownPlayerPos = null;
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        targetPlayer = null;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] != null && hits[i].CompareTag("Player1"))
            {
                targetPlayer = hits[i].transform;
                lastKnownPlayerPos = targetPlayer.position;
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
        float dashTime = 0.2f;
        float elapsed = 0f;

        rigid.linearVelocity = dir * monsterData.DashSpeed;

        while (elapsed <= dashTime)
        {
            elapsed += Time.deltaTime;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, monsterData.DashSpeed * Time.deltaTime, LayerMask.GetMask("Player"));
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                Character player = hit.collider.GetComponent<Character>();
                if (player != null)
                {
                    player.TakeDamage(monsterData.dmg);

                    rigid.linearVelocity = Vector2.zero;
                    isDashing = false;

                    yield return new WaitForSeconds(info.attackDelay);

                    DetectPlayer();
                    if (targetPlayer != null)
                        ChangeState(StateType.Move);
                    else
                        ChangeState(StateType.Idle);

                    yield break;
                }
            }
            yield return null;
        }

        rigid.linearVelocity = Vector2.zero;
        isDashing = false;

        DetectPlayer();
        if (targetPlayer != null)
            ChangeState(StateType.Move);
        else
        {
            detectTimer = 0f;
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
