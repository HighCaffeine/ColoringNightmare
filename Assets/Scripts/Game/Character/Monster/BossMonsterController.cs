using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossMonsterController : Character, OnReturnPool<BossMonsterController>
{
    [SerializeField] private BossMonsterData bossData;
    public OnReturnPoolEvent<BossMonsterController> OnReturnPoolEvent;

    [Header("Pattern Areas")]
    [SerializeField] private AreaData p1Area; // 공 떨어지는 구역 (전체 맵)
    [SerializeField] private AreaData p2Area; // 세로 패턴 구역 (가로 범위)
    [SerializeField] private AreaData p3Area; // 가로 패턴 구역 (세로 범위)

    // [★신규★] 패턴에 사용할 프리팹 및 이펙트 연결
    [Header("Pattern Assets")]
    [Tooltip("사각형 경고 장판 (P2, P3용)")]
    [SerializeField] private GameObject warningBoxPrefab;

    [Tooltip("원형 경고 장판 (P1용)")]
    [SerializeField] private GameObject warningCirclePrefab;

    [Header("P1 Assets (Summon)")]
    [Tooltip("하늘에서 떨어질 공 프리팹")]
    [SerializeField] private GameObject p1BallPrefab;
    [Tooltip("공이 터질 때 소환될 몬스터 데이터 리스트")]
    [SerializeField] private List<MonsterData> p1SummonMonsters;

    [Header("P2 Assets (Vertical)")]
    [Tooltip("양쪽에서 날아올 심벌즈 프리팹")]
    [SerializeField] private GameObject p2CymbalPrefab;

    [Header("P3 Assets (Horizontal)")]
    [Tooltip("주먹이 나갈 상자 프리팹")]
    [SerializeField] private GameObject p3BoxPrefab;
    [Tooltip("상자에서 튀어나올 주먹 프리팹 (또는 이펙트)")]
    [SerializeField] private GameObject p3FistPrefab;

    [Header("Boss Effects")]
    [Tooltip("무적 상태일 때 피격 시 나올 이펙트 데이터")]
    [SerializeField] private EffectVisualData invincibleHitEffect;

    // 애니메이션 상수
    private const string ANIM_IDLE = "idle";
    private const string ANIM_CAST = "cast";
    private const string ANIM_GROGGY = "groggy";
    private const string ANIM_HIT = "hit";

    // 상태 관리
    private int currentGroggyCoin;
    private bool isInvincible = true;

    // 패턴 관련
    private float idleTimer = 0f;
    private float currentIdleDuration = 2f;

    public void Setup(BossMonsterData data)
    {
        bossData = data;
        info = data;
        currentHP = info.maxHp;

        currentGroggyCoin = bossData.groggyCoinMax;
        isInvincible = true;
        state = StateType.Idle;

        SetSpineAnimation(ANIM_IDLE, true);
        SetIdleDuration();
    }

    private void SetIdleDuration()
    {
        currentIdleDuration = Random.Range(bossData.idleDurationMin, bossData.idleDurationMax);
        idleTimer = 0f;
    }

    void Update()
    {
        if (isDead) return;

        switch (state)
        {
            case StateType.Idle: Idle(); break;
            case StateType.Attack: break;
            case StateType.Skill: break;
        }
    }

    protected override void Idle()
    {
        if (currentGroggyCoin <= 0)
        {
            StartCoroutine(GroggyRoutine());
            return;
        }

        idleTimer += Time.deltaTime;
        if (idleTimer >= currentIdleDuration)
        {
            StartCoroutine(SelectAndExecutePattern());
        }
    }

    private IEnumerator SelectAndExecutePattern()
    {
        state = StateType.Attack;

        int totalWeight = bossData.p1Weight + bossData.p2Weight + bossData.p3Weight;
        int randomValue = Random.Range(0, totalWeight);

        if (randomValue < bossData.p1Weight)
        {
            yield return StartCoroutine(Pattern1_Summon());
        }
        else if (randomValue < bossData.p1Weight + bossData.p2Weight)
        {
            yield return StartCoroutine(Pattern2_Vertical());
        }
        else
        {
            yield return StartCoroutine(Pattern3_Horizontal());
        }

        state = StateType.Idle;
        SetIdleDuration();
        SetSpineAnimation(ANIM_IDLE, true);
    }

    // --- P1: 몬스터 소환 (원형 범위) ---
    private IEnumerator Pattern1_Summon()
    {
        SetSpineAnimation(ANIM_CAST, false);
        yield return new WaitForSeconds(bossData.castingDuration);

        // 1. 위치 선정 및 예고
        Vector2 spawnPos = GetRandomPosInArea(p1Area);

        // [★수정★] 원형(Circle) 프리팹 사용
        GameObject warning = Instantiate(warningCirclePrefab, spawnPos, Quaternion.identity);
        warning.transform.localScale = Vector3.one * 3.0f; // 크기 조절

        WarningArea warningScript = warning.GetComponent<WarningArea>();
        if (warningScript != null)
        {
            // 중앙에서 퍼지도록 설정
            warningScript.Setup(bossData.warningDuration, WarningFillType.CenterExpand);
        }

        Destroy(warning, bossData.warningDuration);

        yield return new WaitForSeconds(bossData.warningDuration);

        // 2. 공 낙하 연출
        if (p1BallPrefab != null)
        {
            Vector2 startPos = spawnPos + Vector2.up * 10f;
            GameObject ball = Instantiate(p1BallPrefab, startPos, Quaternion.identity);

            float fallDuration = 0.5f;
            float elapsed = 0f;
            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;
                ball.transform.position = Vector2.Lerp(startPos, spawnPos, elapsed / fallDuration);
                yield return null;
            }
            Destroy(ball);
        }

        // 3. 폭발 및 데미지
        if (effectController != null && bossData.p1ExplosionEffect != null)
        {
            // EffectController에 3번째 인자(isFacingLeft)는 false(오른쪽)으로 가정
            effectController.PlayHitEffectAt(spawnPos, bossData.p1ExplosionEffect, false);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(spawnPos, bossData.p1ExplosionRadius, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player1"))
            {
                hit.GetComponent<Character>()?.TakeDamage(info.dmg);
            }
        }

        //몬스터 소환
        if (p1SummonMonsters != null && p1SummonMonsters.Count > 0)
        {
            MonsterData randomMonster = p1SummonMonsters[Random.Range(0, p1SummonMonsters.Count)];
            MonsterManager.Instance.SpawnMonster(randomMonster, spawnPos);
        }

        TakeFixedDamage();
        DecreaseGroggyCoin();
    }

    //세로 패턴
    private IEnumerator Pattern2_Vertical()
    {
        SetSpineAnimation(ANIM_CAST, false);
        yield return new WaitForSeconds(bossData.castingDuration);

        float randomX = Random.Range(p2Area.pos.x - p2Area.size.x / 2, p2Area.pos.x + p2Area.size.x / 2);
        Vector2 centerPos = new Vector2(randomX, p2Area.pos.y);

        float yOffset = p2Area.size.y * 0.25f;
        Vector2 topPos = new Vector2(randomX, p2Area.pos.y + yOffset);
        Vector2 bottomPos = new Vector2(randomX, p2Area.pos.y - yOffset);

        // 위쪽 장판 
        GameObject warningTop = Instantiate(warningBoxPrefab, topPos, Quaternion.identity);
        warningTop.transform.localScale = new Vector3(2f, p2Area.size.y * 0.5f, 1f);

        //아래쪽 장판
        GameObject warningBottom = Instantiate(warningBoxPrefab, bottomPos, Quaternion.identity);
        warningBottom.transform.localScale = new Vector3(2f, p2Area.size.y * 0.5f, 1f);

        WarningArea scriptTop = warningTop.GetComponent<WarningArea>();
        WarningArea scriptBottom = warningBottom.GetComponent<WarningArea>();

        if (scriptTop != null) scriptTop.Setup(bossData.warningDuration, WarningFillType.TopToBottom);
        if (scriptBottom != null) scriptBottom.Setup(bossData.warningDuration, WarningFillType.BottomToTop);

        Destroy(warningTop, bossData.warningDuration);
        Destroy(warningBottom, bossData.warningDuration);

        yield return new WaitForSeconds(bossData.warningDuration);

        //심벌즈 생성 및 돌진
        Vector2 cymbalTopStart = new Vector2(randomX, p2Area.pos.y + p2Area.size.y / 2);
        Vector2 cymbalBotStart = new Vector2(randomX, p2Area.pos.y - p2Area.size.y / 2);

        GameObject topObj = Instantiate(p2CymbalPrefab, cymbalTopStart, Quaternion.identity);
        GameObject bottomObj = Instantiate(p2CymbalPrefab, cymbalBotStart, Quaternion.identity);

        float distance = Vector2.Distance(cymbalTopStart, centerPos);
        float duration = distance / bossData.p2TravelSpeed;
        float elapsed = 0f;
        bool damageDealt = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector2 curTop = Vector2.Lerp(cymbalTopStart, centerPos, t);
            Vector2 curBot = Vector2.Lerp(cymbalBotStart, centerPos, t);

            if (topObj) topObj.transform.position = curTop;
            if (bottomObj) bottomObj.transform.position = curBot;

            // 충돌 체크
            if (!damageDealt)
            {
                Vector2 boxSize = new Vector2(bossData.p2DamageWidth, Vector2.Distance(curTop, curBot));
                Vector2 boxCenter = (curTop + curBot) * 0.5f;
                Collider2D hit = Physics2D.OverlapBox(boxCenter, boxSize, 0f, LayerMask.GetMask("Player"));
                if (hit != null && hit.CompareTag("Player1"))
                {
                    hit.GetComponent<Character>()?.TakeDamage(info.dmg);
                    damageDealt = true;
                }
            }
            yield return null;
        }

        if (topObj) Destroy(topObj);
        if (bottomObj) Destroy(bottomObj);
    }

    // 가로 패턴
    private IEnumerator Pattern3_Horizontal()
    {
        SetSpineAnimation(ANIM_CAST, false);
        yield return new WaitForSeconds(bossData.castingDuration);

        float randomY = Random.Range(p3Area.pos.y - p3Area.size.y / 2, p3Area.pos.y + p3Area.size.y / 2);
        bool isLeft = Random.value > 0.5f;

        float startX = isLeft ? p3Area.pos.x - p3Area.size.x / 2 : p3Area.pos.x + p3Area.size.x / 2;
        float endX = isLeft ? p3Area.pos.x + p3Area.size.x / 2 : p3Area.pos.x - p3Area.size.x / 2;

        Vector2 startPos = new Vector2(startX, randomY);
        Vector2 endPos = new Vector2(endX, randomY);
        Vector2 centerPos = new Vector2(p3Area.pos.x, randomY);

        // 경고 장판
        GameObject warning = Instantiate(warningBoxPrefab, centerPos, Quaternion.identity);
        warning.transform.localScale = new Vector3(p3Area.size.x, 2f, 1);

        WarningArea warningScript = warning.GetComponent<WarningArea>();
        if (warningScript != null)
        {
            // 중앙에서 좌우로 퍼지도록 설정
            warningScript.Setup(bossData.warningDuration, WarningFillType.Horizontal);
        }

        Destroy(warning, bossData.warningDuration);

        yield return new WaitForSeconds(bossData.warningDuration);

        // 오브젝트 발사
        if (p3BoxPrefab != null)
        {
            // 상자 생성 시작 위치
            GameObject boxObj = Instantiate(p3BoxPrefab, startPos, Quaternion.identity);
            if (isLeft) boxObj.transform.localScale = new Vector3(-1, 1, 1);
            Destroy(boxObj, 2.0f);
        }

        // 주먹 발사
        GameObject punchObj = Instantiate(p3FistPrefab, startPos, Quaternion.identity);
        if (isLeft) punchObj.transform.localScale = new Vector3(-1, 1, 1);

        float distance = Mathf.Abs(endX - startX);
        float duration = distance / bossData.p3PunchSpeed;
        float elapsed = 0f;
        bool damageDealt = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Vector2 curPos = Vector2.Lerp(startPos, endPos, t);

            if (punchObj) punchObj.transform.position = curPos;

            if (!damageDealt)
            {
                Collider2D hit = Physics2D.OverlapBox(curPos, new Vector2(1f, bossData.p3DamageHeight), 0f, LayerMask.GetMask("Player"));
                if (hit != null && hit.CompareTag("Player1"))
                {
                    hit.GetComponent<Character>()?.TakeDamage(info.dmg);
                    damageDealt = true;
                }
            }
            yield return null;
        }

        if (punchObj) Destroy(punchObj);
    }

    private IEnumerator GroggyRoutine()
    {
        state = StateType.Skill;
        isInvincible = false;

        SetSpineAnimation(ANIM_GROGGY, true);

        yield return new WaitForSeconds(bossData.groggyDuration);

        isInvincible = true;
        currentGroggyCoin = bossData.groggyCoinMax;
        state = StateType.Idle;
        SetSpineAnimation(ANIM_IDLE, true);
        SetIdleDuration();
    }

    public override void TakeDamage(int amount, EffectVisualData hitEffect)
    {
        if (isInvincible)
        {
            if (effectController != null && invincibleHitEffect != null)
            {
                effectController.PlayHitEffectAt(transform.position, invincibleHitEffect, false);
            }
            return;
        }
        base.TakeDamage(amount, hitEffect);
    }

    private void TakeFixedDamage()
    {
        int damage = Mathf.RoundToInt(info.maxHp * bossData.fixedDamageRatio);
        currentHP -= damage;
        if (currentHP <= 0) Dead();
    }

    private void DecreaseGroggyCoin()
    {
        if (currentGroggyCoin > 0) currentGroggyCoin--;
    }

    protected override void Dead()
    {
        base.Dead();
        OnReturnPoolEvent?.Invoke(this);
    }

    public void Init(OnReturnPoolEvent<BossMonsterController> onReturnPoolEvent)
    {
        OnReturnPoolEvent = onReturnPoolEvent;
    }

    private void SetSpineAnimation(string animName, bool loop)
    {
        if (skeleton != null && skeleton.skeletonDataAsset != null)
        {
            if (skeleton.AnimationName != animName)
            {
                skeleton.AnimationState.SetAnimation(0, animName, loop);
            }
        }
    }

    private Vector2 GetRandomPosInArea(AreaData area)
    {
        if (area == null) return transform.position;
        Bounds b = area.GetBounds();
        return new Vector2(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y));
    }
}