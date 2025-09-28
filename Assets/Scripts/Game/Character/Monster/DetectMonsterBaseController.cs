using UnityEngine;
using System.Collections;

public class DetectMonsterBaseController : WallMonsterBaseController<DetectMonsterData>,
    OnReturnPool<DetectMonsterBaseController>
{
    OnReturnPoolEvent<DetectMonsterBaseController> OnReturnPoolEvent;

    [Header("Detect Settings")]
    public float detectionRange = 5f;           // 플레이어 탐지 범위
    private Transform targetPlayer;            // 현재 타겟 플레이어
    private float detectInterval = 0.1f;       // 탐지 주기
    private float detectTimer = 0f;

    private bool isDashing = false;            // 플레이어 공격 중 여부

    MonsterManager.OnPlayerIsDead onPlayerIsDead;

    public override void Setup(DetectMonsterData data)
    {
        base.Setup(data);
        detectionRange = data.AttackRange;

        onPlayerIsDead += MonsterManager.Instance.IsPlayerDead;
    }

    protected void Update()
    {
        if (isDead) return;

        // 플레이어가 죽으면 모든 행동을 중단합니다.
        if (onPlayerIsDead?.Invoke() == true)
        {
            StopMove();
            return;
        }

        switch (state)
        {
            case StateType.Idle:
                // Idle 상태에서 플레이어 감지 후 추적 상태로 전환
                IdleAndDetect();
                break;
            case StateType.Move:
                // Move 상태는 이제 '추적' 역할을 합니다.
                ChasePlayer();
                break;
            case StateType.Attack:
                Attack();
                break;
            case StateType.Dead:
                Dead();
                break;
        }
    }

    protected void IdleAndDetect()
    {
        // 탐지 주기 타이머
        detectTimer += Time.deltaTime;
        if (detectTimer >= detectInterval)
        {
            detectTimer = 0f;
            DetectPlayer();
        }

        // 플레이어 탐지 시, 추적 상태로 전환
        if (targetPlayer != null)
        {
            ChangeState(StateType.Move);
            return;
        }

        base.Idle();
    }

    private void ChasePlayer()
    {
        if (targetPlayer == null)
        {
            ChangeState(StateType.Idle);
            return;
        }

        float distance = Vector2.Distance(transform.position, targetPlayer.position);

        // 탐지 범위를 벗어나면 Idle 상태로 복귀
        if (distance > detectionRange)
        {
            targetPlayer = null;
            ChangeState(StateType.Idle);
            return;
        }

        // 공격 범위 내에 있으면 공격 상태로 전환
        if (distance <= monsterData.AttackRange)
        {
            ChangeState(StateType.Attack);
            return;
        }

        // 플레이어 방향으로 이동
        Vector2 dir = (targetPlayer.position - transform.position).normalized;
        MoveCharacter(MonsterManager.Instance.GetAreaBound(), dir, null);
        Flip(dir.x > 0);
    }

    private void DetectPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange);

        targetPlayer = null;

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;

            if (hits[i].CompareTag("Player1"))
            {
                targetPlayer = hits[i].transform;
                break;
            }
        }
    }

    protected override void Attack()
    {
        // 이미 돌진 중이면 무시
        if (isDashing) return;

        if (targetPlayer != null)
        {
            // 플레이어 공격 돌진
            Vector2 dashDir = (targetPlayer.position - transform.position).normalized;
            StartCoroutine(DashTowardsTarget(dashDir));
        }
        else
        {
            // 타겟 없으면 부모 Attack 호출  벽 공격
            base.Attack();
        }
    }

    private IEnumerator DashTowardsTarget(Vector2 dir)
    {
        isDashing = true;

        float dashTime = 0.2f;
        float elapsed = 0f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + (Vector3)dir * monsterData.DashSpeed * dashTime;


        Flip(dir.x < 0);

        while (elapsed <= dashTime)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / dashTime);

            // 플레이어 충돌 감지
            Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.3f);
            if (hit != null && hit.CompareTag("Player1"))
            {
                PlayerController player = hit.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(monsterData.dmg); // 몬스터 대미지 적용
                    isDashing = false;
                    ChangeState(StateType.Move);
                    yield break; // 대미지 주면 dash 종료
                }
            }

            // 맵 경계 체크
            var bounds = MonsterManager.Instance.GetAreaBound();
            Vector3 clamped = transform.position;
            clamped.x = Mathf.Clamp(clamped.x, bounds.min.x, bounds.max.x);
            clamped.y = Mathf.Clamp(clamped.y, bounds.min.y, bounds.max.y);
            transform.position = clamped;

            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        targetPlayer = null;
        ChangeState(StateType.Idle);
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
