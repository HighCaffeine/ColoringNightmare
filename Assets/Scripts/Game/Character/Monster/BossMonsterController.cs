using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;

public class BossMonsterController : Character, OnReturnPool<BossMonsterController>
{
    private MonsterManager.OnEndingEvent OnDeadEvent;

    [SerializeField] private BossMonsterData bossData;
    public OnReturnPoolEvent<BossMonsterController> OnReturnPoolEvent;

    [Header("Components")]
    [SerializeField] private BossSpineController bossSpine;

    [Header("Pattern Areas")]
    [SerializeField] private AreaData p1Area;
    [SerializeField] private AreaData p2Area;
    [SerializeField] private AreaData p3Area;

    // 상태 관리
    private int currentGroggyCoin;
    private bool isInvincible = true;
    private float idleTimer = 0f;
    private float currentIdleDuration = 2f;

    // UI 컨트롤러
    private BossUIController bossUI => BossUIController.Instance;

    protected override void Awake()
    {
        base.Awake();
        if (bossSpine == null) bossSpine = GetComponent<BossSpineController>();
    }

    public void Setup(BossMonsterData data, MonsterManager.OnEndingEvent OnDeadEvent)
    {
        bossData = data;
        info = data;
        currentHP = info.maxHp;

        currentGroggyCoin = bossData.groggyCoinMax;
        isInvincible = true;

        if (bossUI != null) bossUI.Init(info.maxHp, currentGroggyCoin);
        this.OnDeadEvent = OnDeadEvent;

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        state = StateType.None;

        if (bossSpine != null)
        {
            bossSpine.PlayEnter();

            float duration = bossSpine.GetAnimationDuration(BossSpineController.ENTER);
            if (duration > 0) yield return new WaitForSeconds(duration);
            else yield return new WaitForSeconds(1.0f);
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }

        state = StateType.Idle;
        if (bossSpine != null) bossSpine.PlayIdle();
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
        if (state == StateType.Idle) Idle();
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
        if (bossSpine != null) bossSpine.PlayIdle();
    }

    // --- P1: 몬스터 소환 ---
    private IEnumerator Pattern1_Summon()
    {
        if (bossSpine != null)
        {
            yield return StartCoroutine(bossSpine.PlayStartAndMiddle("spawn_skill", bossData.warningDuration));
        }
        else
        {
            yield return new WaitForSeconds(bossData.warningDuration);
        }

        Vector2 spawnPos = GetRandomPosInArea(p1Area);
        SpawnWarning(bossData.warningCirclePrefab, spawnPos, Vector3.one * 3.0f, WarningFillType.CenterExpand);

        if (bossSpine != null)
        {
            yield return StartCoroutine(bossSpine.PlayEndAndWaitForEvent("spawn_skill", "spawn"));
        }

        if (bossData.p1BallPrefabs.Count > 0)
        {
            GameObject ball = Instantiate(bossData.p1BallPrefabs[UnityEngine.Random.Range(0, bossData.p1BallPrefabs.Count)], spawnPos + Vector2.up * 10f, Quaternion.identity);
            float fallTime = 0.5f;
            float t = 0;
            Vector2 start = ball.transform.position;
            while (t < fallTime)
            {
                t += Time.deltaTime;
                ball.transform.position = Vector2.Lerp(start, spawnPos, t / fallTime);
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
            if (hit.CompareTag("Player1")) hit.GetComponent<Character>()?.TakeDamage(info.dmg);
        }

        if (bossData.p1SummonMonsters != null && bossData.p1SummonMonsters.Count > 0)
        {
            MonsterData randomMonster = bossData.p1SummonMonsters[Random.Range(0, bossData.p1SummonMonsters.Count)];

            Character summonedMonster = MonsterManager.Instance.SpawnMonster(randomMonster, spawnPos);

            if (summonedMonster != null)
            {
                summonedMonster.OnDeathCallback += DecreaseGroggyCoin;
            }
        }
    }

    // --- P2: 세로 패턴 ---
    private IEnumerator Pattern2_Vertical()
    {
        if (bossSpine != null)
        {
            yield return StartCoroutine(bossSpine.PlayStartAndMiddle("cymbals_skill", bossData.warningDuration));
        }

        float randomX = Random.Range(p2Area.pos.x - p2Area.size.x / 2, p2Area.pos.x + p2Area.size.x / 2);
        Vector2 centerPos = new Vector2(randomX, p2Area.pos.y);
        float yOffset = p2Area.size.y * 0.25f;

        SpawnWarning(bossData.warningBoxPrefab, new Vector2(randomX, p2Area.pos.y + yOffset), new Vector3(2f, p2Area.size.y * 0.5f, 1f), WarningFillType.TopToBottom);
        SpawnWarning(bossData.warningBoxPrefab, new Vector2(randomX, p2Area.pos.y - yOffset), new Vector3(2f, p2Area.size.y * 0.5f, 1f), WarningFillType.BottomToTop);
        yield return new WaitForSeconds(bossData.warningDuration);

        if (bossSpine != null) yield return StartCoroutine(bossSpine.PlayEndAndWaitForEvent("cymbals_skill", "attack_start"));


        Vector2 topStart = new Vector2(randomX, p2Area.pos.y + p2Area.size.y / 2);
        Vector2 botStart = new Vector2(randomX, p2Area.pos.y - p2Area.size.y / 2);

        GameObject topObj = Instantiate(bossData.p2CymbalPrefab, topStart, Quaternion.identity);
        GameObject botObj = Instantiate(bossData.p2CymbalPrefab, botStart, Quaternion.identity);

        botObj.transform.localScale = new Vector3(botObj.transform.localScale.x, -botObj.transform.localScale.y, botObj.transform.localScale.z);

        float duration = Vector2.Distance(topStart, centerPos) / bossData.p2TravelSpeed;
        float elapsed = 0f;
        bool damageDealt = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Vector2 curTop = Vector2.Lerp(topStart, centerPos, t);
            Vector2 curBot = Vector2.Lerp(botStart, centerPos, t);

            if (topObj) topObj.transform.position = curTop;
            if (botObj) botObj.transform.position = curBot;

            if (!damageDealt)
            {
                Vector2 boxSize = new Vector2(bossData.p2DamageWidth, Vector2.Distance(curTop, curBot));
                Collider2D hit = Physics2D.OverlapBox((curTop + curBot) * 0.5f, boxSize, 0f, LayerMask.GetMask("Player"));
                if (hit != null && hit.CompareTag("Player1"))
                {
                    hit.GetComponent<Character>()?.TakeDamage(info.dmg);
                    if (effectController != null && bossData.p2HitEffect != null)
                        effectController.PlayHitEffectAt(centerPos, bossData.p2HitEffect, false);
                    damageDealt = true;
                }
            }
            yield return null;
        }
        if (topObj) Destroy(topObj);
        if (botObj) Destroy(botObj);
    }

    // --- P3: 가로 패턴 ---
    private IEnumerator Pattern3_Horizontal()
    {
        if (bossSpine != null) yield return StartCoroutine(bossSpine.PlayStartAndMiddle("box_spawn", 0f));
        if (bossSpine != null) yield return StartCoroutine(bossSpine.PlayEndAndWaitForEvent("box_spawn", "spawn"));

        float randomY = Random.Range(p3Area.pos.y - p3Area.size.y / 2, p3Area.pos.y + p3Area.size.y / 2);
        bool isLeft = true;
        float startX = isLeft ? p3Area.pos.x - p3Area.size.x / 2 : p3Area.pos.x + p3Area.size.x / 2;
        float endX = isLeft ? p3Area.pos.x + p3Area.size.x / 2 : p3Area.pos.x - p3Area.size.x / 2;
        Vector2 boxPos = new Vector2(startX, randomY);

        GameObject boxObj = Instantiate(bossData.p3BoxPrefab, boxPos, Quaternion.identity);
        BoxObjectController boxCtrl = boxObj.GetComponentInChildren<BoxObjectController>();

        if (boxCtrl != null)
        {
            //boxCtrl.SetFlip(isLeft);
            boxCtrl.PlaySpawn();
        }

        SpawnWarning(bossData.warningBoxPrefab, new Vector2(p3Area.pos.x, randomY), new Vector3(p3Area.size.x, 2f, 1), WarningFillType.LeftToRight);
        yield return new WaitForSeconds(bossData.warningDuration);

        if (bossSpine != null) yield return StartCoroutine(bossSpine.PlayStartAndMiddle("box_skill", 0f));
        if (bossSpine != null) yield return StartCoroutine(bossSpine.PlayEndAndWaitForEvent("box_skill", "attack_start"));

        float attackDuration = 0f;
        if (boxCtrl != null)
        {
            attackDuration = boxCtrl.PlayAttack();
        }

        if (effectController != null && bossData.p3AttackEffect != null)
        {
            effectController.PlayHitEffectAt(boxPos, bossData.p3AttackEffect, isLeft);
        }

        yield return new WaitForSeconds(0.1f);

        Vector2 attackCenter = new Vector2(p3Area.pos.x, randomY);
        Vector2 attackSize = new Vector2(p3Area.size.x, bossData.p3DamageHeight);

        Collider2D[] hits = Physics2D.OverlapBoxAll(attackCenter, attackSize, 0f, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player1"))
            {
                hit.GetComponent<Character>()?.TakeDamage(info.dmg);

                if (effectController != null && bossData.p3HitEffect != null)
                {
                    effectController.PlayHitEffectAt(hit.transform.position, bossData.p3HitEffect, false);
                }
            }
        }

        yield return new WaitForSeconds(Mathf.Max(0, attackDuration - 0.1f));

        if (boxCtrl != null)
        {
            float endDuration = boxCtrl.PlayEnd();
            Destroy(boxObj, endDuration);
        }
        else Destroy(boxObj);
    }

    private IEnumerator GroggyRoutine()
    {
        state = StateType.Groggy;
        TakeFixedDamage();

        isInvincible = false;
        if (bossSpine != null) bossSpine.PlayGroggy();

        if (bossUI != null) bossUI.UpdateCoins(0);

        yield return new WaitForSeconds(bossData.groggyDuration);

        isInvincible = true;
        currentGroggyCoin = bossData.groggyCoinMax;

        if (bossUI != null) bossUI.UpdateCoins(currentGroggyCoin);

        state = StateType.Idle;
        if (bossSpine != null) bossSpine.PlayIdle();
        SetIdleDuration();
    }

    private void SpawnWarning(GameObject prefab, Vector2 pos, Vector3 scale, WarningFillType type)
    {
        GameObject warning = Instantiate(prefab, pos, Quaternion.identity);
        warning.transform.localScale = scale;
        WarningArea ws = warning.GetComponent<WarningArea>();
        if (ws != null) ws.Setup(bossData.warningDuration, type);
        Destroy(warning, bossData.warningDuration);
    }

    public override void TakeDamage(int amount, EffectVisualData hitEffect)
    {
        if (isInvincible)
        {
            if (effectController != null && bossData.invincibleHitEffect != null)
            {
                effectController.PlayHitEffectAt(transform.position, bossData.invincibleHitEffect, FlipX());
            }
            return;
        }

        base.TakeDamage(amount, hitEffect);
        if (bossUI != null) bossUI.UpdateHP(currentHP, info.maxHp);
    }

    private void TakeFixedDamage()
    {
        int dmg = Mathf.RoundToInt(info.maxHp * bossData.fixedDamageRatio);
        currentHP -= dmg;
        if (bossUI != null) bossUI.UpdateHP(currentHP, info.maxHp);
        if (currentHP <= 0) Dead();
    }

    private void DecreaseGroggyCoin()
    {
        if (currentGroggyCoin > 0)
        {
            currentGroggyCoin--;
            if (bossUI != null) bossUI.UpdateCoins(currentGroggyCoin);
        }
    }

    protected override void Dead()
    {
        base.Dead();
        if (bossUI != null) bossUI.SetActiveBossHP(false);

        if (GameManager.Instance != null)
        {
            Invoke(nameof(End), 3f);
        }

        OnReturnPoolEvent?.Invoke(this);
    }

    private void End()
    {
        GameManager.Instance.OnEndingPanel();
    }

    public void Init(OnReturnPoolEvent<BossMonsterController> onReturnPoolEvent) { OnReturnPoolEvent = onReturnPoolEvent; }
    private Vector2 GetRandomPosInArea(AreaData area)
    {
        if (area == null) return transform.position;
        Bounds b = area.GetBounds();
        return new Vector2(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y));
    }
}