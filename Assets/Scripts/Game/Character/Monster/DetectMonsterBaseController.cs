using UnityEngine;
using System.Collections;
using System;

public class DetectMonsterBaseController : WallMonsterBaseController<DetectMonsterData>,
    OnReturnPool<DetectMonsterBaseController>
{
    OnReturnPoolEvent<DetectMonsterBaseController> OnReturnPoolEvent;

    [Header("Detect Settings")]
    public float detectionRange = 7f;
    [SerializeField] private Transform targetPlayer;
    private float detectInterval = 0.1f;
    private float detectTimer = 0f;
    private Vector2? lastKnownPlayerPos = null;

    private bool isDashing = false;
    MonsterManager.OnPlayerIsDead onPlayerIsDead;

    public override void Setup(DetectMonsterData data)
    {
        base.Setup(data);
        detectionRange = data.DetectRange;
        onPlayerIsDead += MonsterManager.Instance.IsPlayerDead;
    }

    private void FixedUpdate()
    {
        if (isDead || isDashing)
        {
            if (isDead) rigid.linearVelocity = Vector2.zero;
            return;
        }

        detectTimer += Time.fixedDeltaTime;
        if (detectTimer >= detectInterval)
        {
            detectTimer = 0f;
            DetectPlayer();
        }

        switch (state)
        {
            case StateType.Idle:
                base.Move(Vector2.right);
                break;
            case StateType.Move:
                ChaseOrSearch();
                break;
        }
    }

    private void ChaseOrSearch()
    {
        Vector2 targetPosition;

        if (targetPlayer != null)
        {
            targetPosition = targetPlayer.position;
            lastKnownPlayerPos = targetPosition;
        }
        else if (lastKnownPlayerPos.HasValue)
        {
            targetPosition = lastKnownPlayerPos.Value;
        }
        else
        {
            ChangeState(StateType.Idle);
            return;
        }

        float distance = Vector2.Distance(transform.position, targetPosition);

        if (targetPlayer != null && distance <= monsterData.AttackRange)
        {
            StopMove();
            ChangeState(StateType.Attack);
            return;
        }

        if (targetPlayer == null && distance < 0.2f)
        {
            lastKnownPlayerPos = null;
            ChangeState(StateType.Idle);
            return;
        }

        Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;
        MoveCharacter(MonsterManager.Instance.GetAreaBound(), dir, null);
    }

    private void DetectPlayer()
    {
        if (onPlayerIsDead?.Invoke() == true)
        {
            targetPlayer = null;
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange, LayerMask.GetMask("Player"));
        Transform potentialTarget = null;
        float closestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player1"))
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    potentialTarget = hit.transform;
                }
            }
        }
        targetPlayer = potentialTarget;

        if (targetPlayer != null && state == StateType.Idle)
        {
            ChangeState(StateType.Move);
        }
    }

    protected override void Attack()
    {
        base.Attack();
        if (isDashing) return;

        if (targetPlayer != null)
        {
            Vector2 dashDir = (targetPlayer.position - transform.position).normalized;
            Flip(dashDir.x > 0);
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

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, monsterData.DashSpeed * 0.2f, LayerMask.GetMask("Player"));
        if (hit.collider != null && hit.collider.CompareTag("Player1"))
        {
            hit.collider.GetComponent<Character>()?.TakeDamage(monsterData.dmg);
        }

        rigid.linearVelocity = dir * monsterData.DashSpeed;
        yield return new WaitForSeconds(0.2f);
        rigid.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(info.attackDelay);
        isDashing = false;

        ChangeState(StateType.Move);
    }

    public void Init(OnReturnPoolEvent<DetectMonsterBaseController> onReturnPoolEvent)
    {
        OnReturnPoolEvent = onReturnPoolEvent;
        Flip(true);
        lastKnownPlayerPos = null;
        targetPlayer = null;
        ChangeState(StateType.Idle);
    }

    protected override void Dead()
    {
        base.Dead();
        StopAllCoroutines();
        OnReturnPoolEvent?.Invoke(this);
    }
}