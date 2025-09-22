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

    public override void Setup(DetectMonsterData data)
    {
        base.Setup(data);
        detectionRange = data.AttackRange;
    }

    protected override void Move(Vector2 dir)
    {
        // 탐지 주기 타이머
        detectTimer += Time.deltaTime;
        if (detectTimer >= detectInterval)
        {
            detectTimer = 0f;
            DetectPlayer();
        }

        // 플레이어 탐지 및 공격 사거리 체크
        if (targetPlayer != null)
        {
            float distance = Vector2.Distance(transform.position, targetPlayer.position);

            // 탐지 범위 벗어나면 초기화
            if (distance > detectionRange)
                targetPlayer = null;
            else
            {
                dir = (targetPlayer.position - transform.position).normalized;

                if (distance <= monsterData.AttackRange)
                {
                    ChangeState(StateType.Attack);
                    return;
                }
            }
        }

        base.Move(dir);
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

        while (elapsed <= dashTime)
        {
            transform.position += (Vector3)dir * monsterData.DashSpeed * Time.deltaTime;

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
