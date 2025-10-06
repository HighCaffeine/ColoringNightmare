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
    }

    protected void Update()
    {
        if (isDead || isDashing) return;

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

    protected void IdleAndDetect()
    {
        base.Move(Vector2.right);

        detectTimer += Time.deltaTime;
        if (detectTimer >= detectInterval)
        {
            detectTimer = 0f;
            DetectPlayer();
        }

        if (targetPlayer != null)
        {
            detectTimer = 0.0f;
            ChangeState(StateType.Move);
        }
    }

    private void ChasePlayer()
    {
        detectTimer += Time.deltaTime;
        if (detectTimer >= detectInterval)
        {
            detectTimer = 0f;
            DetectPlayer();
        }

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
            StartCoroutine(DashTowardsTarget(base.Attack, dashDir));
        }
        else
        {
            ChangeState(StateType.Idle);
        }
    }

    private IEnumerator DashTowardsTarget(Action attackAction, Vector2 dir)
    {
        isDashing = true;

        float dashTime = 0.2f;
        float elapsed = 0f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + (Vector3)dir * monsterData.DashSpeed * dashTime;

        Flip(dir.x > 0);

        while (elapsed <= dashTime)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / dashTime);

            Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.3f);
            if (hit != null && hit.CompareTag("Player1"))
            {
                PlayerController player = hit.GetComponent<PlayerController>();
                if (player != null)
                {
                    attackAction?.Invoke();
                    player.TakeDamage(monsterData.dmg);
                }
            }

            var bounds = MonsterManager.Instance.GetAreaBound();
            Vector3 clamped = transform.position;
            clamped.x = Mathf.Clamp(clamped.x, bounds.min.x, bounds.max.x);
            clamped.y = Mathf.Clamp(clamped.y, bounds.min.y, bounds.max.y);
            transform.position = clamped;

            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;

        // 대쉬가 끝난 후, 즉시 플레이어를 다시 감지
        DetectPlayer();

        // 플레이어가 여전히 범위 안에 있다면 다시 Chase 상태로, 아니면 Idle 상태로
        if (targetPlayer != null)
        {
            ChangeState(StateType.Move);
        }
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