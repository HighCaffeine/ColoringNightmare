using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossMonsterController : Character, OnReturnPool<BossMonsterController>
{
    [SerializeField] private BossMonsterData bossData;
    public OnReturnPoolEvent<BossMonsterController> OnReturnPoolEvent;

    [Header("Pattern Areas (Scene Objects)")]
    [SerializeField] private AreaData p1Area; // 공 떨어지는 구역
    [SerializeField] private AreaData p2Area; // 세로 패턴 구역
    [SerializeField] private AreaData p3Area; // 가로 패턴 구역

    // [★삭제★] 중복되는 프리팹/이펙트 변수들을 모두 제거했습니다.
    // 이제 bossData 안의 변수를 직접 사용합니다.

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

    // --- P1: 몬스터 소환 ---
    private IEnumerator Pattern1_Summon()
    {
        SetSpineAnimation(ANIM_CAST, false);
        yield return new WaitForSeconds(bossData.castingDuration);

        Vector2 spawnPos = GetRandomPosInArea(p1Area);

        // [★수정★] bossData의 프리팹 사용
        GameObject warning = Instantiate(bossData.warningCirclePrefab, spawnPos, Quaternion.identity);
        warning.transform.localScale = Vector3.one * 3.0f;

        WarningArea warningScript = warning.GetComponent<WarningArea>();
        if (warningScript != null)
        {
            warningScript.Setup(bossData.warningDuration, WarningFillType.CenterExpand);
        }
        Destroy(warning, bossData.warningDuration);

        yield return new WaitForSeconds(bossData.warningDuration);

        if (bossData.p1BallPrefab != null)
        {
            Vector2 startPos = spawnPos + Vector2.up * 10f;
            GameObject ball = Instantiate(bossData.p1BallPrefab, startPos, Quaternion.identity);

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

        if (effectController != null && bossData.p1ExplosionEffect != null)
        {
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

        if (bossData.p1SummonMonsters != null && bossData.p1SummonMonsters.Count > 0)
        {
            MonsterData randomMonster = bossData.p1SummonMonsters[Random.Range(0, bossData.p1SummonMonsters.Count)];
            MonsterManager.Instance.SpawnMonster(randomMonster, spawnPos);
        }

        TakeFixedDamage();
        DecreaseGroggyCoin();
    }

    // --- P2: 세로 패턴 ---
    private IEnumerator Pattern2_Vertical()
    {
        SetSpineAnimation(ANIM_CAST, false);
        yield return new WaitForSeconds(bossData.castingDuration);

        float randomX = Random.Range(p2Area.pos.x - p2Area.size.x / 2, p2Area.pos.x + p2Area.size.x / 2);
        Vector2 centerPos = new Vector2(randomX, p2Area.pos.y);
        float yOffset = p2Area.size.y * 0.25f;
        Vector2 topPos = new Vector2(randomX, p2Area.pos.y + yOffset);
        Vector2 bottomPos = new Vector2(randomX, p2Area.pos.y - yOffset);

        // [★수정★] bossData의 프리팹 사용
        GameObject warningTop = Instantiate(bossData.warningBoxPrefab, topPos, Quaternion.identity);
        warningTop.transform.localScale = new Vector3(2f, p2Area.size.y * 0.5f, 1f);

        GameObject warningBottom = Instantiate(bossData.warningBoxPrefab, bottomPos, Quaternion.identity);
        warningBottom.transform.localScale = new Vector3(2f, p2Area.size.y * 0.5f, 1f);

        WarningArea scriptTop = warningTop.GetComponent<WarningArea>();
        WarningArea scriptBottom = warningBottom.GetComponent<WarningArea>();

        if (scriptTop != null) scriptTop.Setup(bossData.warningDuration, WarningFillType.TopToBottom);
        if (scriptBottom != null) scriptBottom.Setup(bossData.warningDuration, WarningFillType.BottomToTop);

        Destroy(warningTop, bossData.warningDuration);
        Destroy(warningBottom, bossData.warningDuration);

        yield return new WaitForSeconds(bossData.warningDuration);

        Vector2 cymbalTopStart = new Vector2(randomX, p2Area.pos.y + p2Area.size.y / 2);
        Vector2 cymbalBotStart = new Vector2(randomX, p2Area.pos.y - p2Area.size.y / 2);

        GameObject topObj = Instantiate(bossData.p2CymbalPrefab, cymbalTopStart, Quaternion.identity);
        GameObject bottomObj = Instantiate(bossData.p2CymbalPrefab, cymbalBotStart, Quaternion.identity);

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

    // --- P3: 가로 패턴 ---
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

        // [★수정★] bossData의 프리팹 사용
        GameObject warning = Instantiate(bossData.warningBoxPrefab, centerPos, Quaternion.identity);
        warning.transform.localScale = new Vector3(p3Area.size.x, 2f, 1);

        WarningArea warningScript = warning.GetComponent<WarningArea>();
        if (warningScript != null)
        {
            warningScript.Setup(bossData.warningDuration, WarningFillType.Horizontal);
        }

        Destroy(warning, bossData.warningDuration);

        yield return new WaitForSeconds(bossData.warningDuration);

        if (bossData.p3BoxPrefab != null)
        {
            GameObject boxObj = Instantiate(bossData.p3BoxPrefab, startPos, Quaternion.identity);
            if (isLeft) boxObj.transform.localScale = new Vector3(-1, 1, 1);
            Destroy(boxObj, 2.0f);
        }

        GameObject punchObj = Instantiate(bossData.p3FistPrefab, startPos, Quaternion.identity);
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
            if (effectController != null && bossData.invincibleHitEffect != null)
            {
                effectController.PlayHitEffectAt(transform.position, bossData.invincibleHitEffect, false);
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