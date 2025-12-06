using UnityEngine;
using TMPro;
using System.Collections;

public class DreamCottonEnhance : GenericSingleton<DreamCottonEnhance>
{
    [Header("Player Reference")]
    [SerializeField] private PlayerController player;

    [Header("Currencies")]
    [SerializeField] private int currentCottonCount = 0;
    [SerializeField] private int currentBlackInkCount = 0;

    [Header("Upgrade Settings")]
    [SerializeField] private int maxLevel = 5;
    [SerializeField] private int hpPerLevel = 3;
    [SerializeField] private float speedPercent = 0.1f;
    [SerializeField] private float dmgPercent = 0.2f;

    [Header("Costs")]
    [SerializeField] private int[] upgradeCosts = { 1, 3, 5, 10, 20 };
    [SerializeField] private int skillCostDmg = 5;
    [SerializeField] private int skillCostHeal = 5;
    [SerializeField] private int skillCostInk = 5;

    [Header("Exchange Cost (Black Ink)")]
    [SerializeField] private int inkExchangeCost = 1;

    // 가챠 비용 (검은 잉크 1개 고정)
    private const int GACHA_COST_BLACK_INK = 1;

    [Header("UI - Cotton Count")]
    [SerializeField] private TextMeshProUGUI[] cottonCountTexts;

    [Header("UI - Black Ink Count")]
    [SerializeField] private TextMeshProUGUI blackInkCountText;

    [Header("UI - Enhance Panel")]
    [SerializeField] private TextMeshProUGUI statText;
    [SerializeField] private TextMeshProUGUI hpCostTxt;
    [SerializeField] private TextMeshProUGUI speedCostTxt;
    [SerializeField] private TextMeshProUGUI dmgCostTxt;
    [SerializeField] private UnityEngine.UI.Image hpBarImage;
    [SerializeField] private UnityEngine.UI.Image speedBarImage;
    [SerializeField] private UnityEngine.UI.Image dmgBarImage;
    [SerializeField] private Sprite[] levelSprites;

    [Header("UI - Skill Panel")]
    [SerializeField] private TextMeshProUGUI dmgSkillCostTxt;
    [SerializeField] private TextMeshProUGUI hpSkillCostTxt;
    [SerializeField] private TextMeshProUGUI inkSkillCostTxt;

    [Header("UI - Gacha Panel")]
    [SerializeField] private TextMeshProUGUI gachaCostTxt;

    [Header("Skill Settings")]
    [SerializeField] private float dmgBuffDuration = 15f;
    [SerializeField] private float dmgBuffRatio = 0.5f;
    [SerializeField] private int worldHealAmount = 3;

    [SerializeField] private TextMeshProUGUI exchangeCostTxt;

    [Header("Gacha Data")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private ItemDropTable inkSkillTable;

    [SerializeField] private ItemData inkRed;
    [SerializeField] private ItemData inkBlue;
    [SerializeField] private ItemData inkYellow;

    [Header("Spine")]
    [SerializeField] private DreamBearController dreamBear;

    // 내부 상태
    private int hpLevel = 0;
    private int speedLevel = 0;
    private int dmgLevel = 0;

    private void Start()
    {
        UpdateAllUI();
    }

    private void UpdateAllUI()
    {
        if (cottonCountTexts != null)
        {
            foreach (var txt in cottonCountTexts)
            {
                if (txt != null) txt.text = currentCottonCount.ToString();
            }
        }

        if (blackInkCountText != null)
        {
            blackInkCountText.text = currentBlackInkCount.ToString();
        }

        UpdateEnhanceSlot(hpLevel, hpCostTxt, hpBarImage);
        UpdateEnhanceSlot(speedLevel, speedCostTxt, speedBarImage);
        UpdateEnhanceSlot(dmgLevel, dmgCostTxt, dmgBarImage);

        if (exchangeCostTxt) exchangeCostTxt.text = inkExchangeCost.ToString();

#if UNITY_EDITOR_64
        Debug.Log($"HP: {player.info.maxHp}\nATK: {player.info.dmg}\nSPD: {player.info.speed:F1}");
#endif

        if (dmgSkillCostTxt) dmgSkillCostTxt.text = skillCostDmg.ToString();
        if (hpSkillCostTxt) hpSkillCostTxt.text = skillCostHeal.ToString();
        if (inkSkillCostTxt) inkSkillCostTxt.text = skillCostInk.ToString();
        if (gachaCostTxt) gachaCostTxt.text = GACHA_COST_BLACK_INK.ToString();
    }

    private void UpdateEnhanceSlot(int level, TextMeshProUGUI costTxt, UnityEngine.UI.Image barImage)
    {
        if (level < maxLevel)
        {
            if (costTxt) costTxt.text = upgradeCosts[level].ToString();
        }
        else
        {
            if (costTxt) costTxt.text = "MAX";
        }

        if (barImage != null && levelSprites != null && level < levelSprites.Length)
        {
            barImage.sprite = levelSprites[level];
        }
    }


    public void OnClickUpgradeHP()
    {
        if (hpLevel >= maxLevel) return;

        if (TryConsumeCotton(upgradeCosts[hpLevel]))
        {
            hpLevel++;
            player.info.maxHp += hpPerLevel;
            player.GetComponent<PlayerHPController>()?.UpdateHpGauge();

            UpdateAllUI();

            if (dreamBear != null) dreamBear.PlayResult();
        }
    }

    public void OnClickUpgradeSpeed()
    {
        if (speedLevel >= maxLevel) return;

        if (TryConsumeCotton(upgradeCosts[speedLevel]))
        {
            speedLevel++;
            player.info.speed += (player.info.speed * speedPercent);
            UpdateAllUI();

            if (dreamBear != null) dreamBear.PlayResult();
        }
    }

    public void OnClickUpgradeDamage()
    {
        if (dmgLevel >= maxLevel) return;

        if (TryConsumeCotton(upgradeCosts[dmgLevel]))
        {
            dmgLevel++;
            player.info.dmg += Mathf.Max(1, Mathf.RoundToInt(player.info.dmg * dmgPercent));
            UpdateAllUI();
        }
    }


    public void SkillDamageBuff()
    {
        if (TryConsumeCotton(skillCostDmg))
        {
            StartCoroutine(DmgBuffRoutine());
            UpdateAllUI();
            if (dreamBear != null) dreamBear.PlayResult();
        }
    }

    private IEnumerator DmgBuffRoutine()
    {
        int buffAmount = Mathf.Max(1, Mathf.RoundToInt(player.info.dmg * dmgBuffRatio));
        player.info.dmg += buffAmount;
        UpdateAllUI();

        yield return new WaitForSeconds(dmgBuffDuration);

        player.info.dmg -= buffAmount;
        UpdateAllUI();
    }

    public void PurchaseInk(string colorName)
    {
        if (currentBlackInkCount < inkExchangeCost)
        {
            return;
        }

        ItemData targetItem = null;
        switch (colorName)
        {
            case "Red": targetItem = inkRed; break;
            case "Blue": targetItem = inkBlue; break;
            case "Yellow": targetItem = inkYellow; break;
        }

        if (targetItem == null) return;

        currentBlackInkCount -= inkExchangeCost;
        UpdateAllUI();

        Vector3 pos = (spawnPoint != null) ? spawnPoint.position : player.transform.position;

        ItemDropTable tempTable = ScriptableObject.CreateInstance<ItemDropTable>();
        tempTable.itemDropTable = new System.Collections.Generic.List<ItemDropData>();
        tempTable.itemDropTable.Add(new ItemDropData { itemData = targetItem, dropChance = 1f, minDropAmount = 1, maxDropAmount = 1 });

        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.SpawnItemsFromTable(tempTable, pos);
        }

        if (dreamBear != null) dreamBear.PlayResult();
    }

    public void SkillWorldHeal()
    {
        if (TryConsumeCotton(skillCostHeal))
        {
            WorldHpController.Instance?.RecoverHP(worldHealAmount);
            UpdateAllUI();
            if (dreamBear != null) dreamBear.PlayResult();
        }
    }

    public void SkillInkRandom()
    {
        if (TryConsumeCotton(skillCostInk))
        {
            Vector3 pos = (spawnPoint != null) ? spawnPoint.position : player.transform.position;
            if (inkSkillTable != null) MonsterManager.Instance.SpawnItemsFromTable(inkSkillTable, pos);
            UpdateAllUI();
            if (dreamBear != null) dreamBear.PlayResult();
        }
    }

    public void AddCotton(int amount)
    {
        currentCottonCount += amount;

        UpdateAllUI();
    }

    public void AddBlackInk(int amount)
    {
        currentBlackInkCount += amount;

        UpdateAllUI();
    }

    private bool TryConsumeCotton(int amount)
    {
        if (currentCottonCount < amount) return false;

        currentCottonCount -= amount;
        return true;
    }
}
/*
(인스펙터)
DreamCottonUIManager: 씬에 빈 오브젝트를 만들고 붙인 뒤, Enhance, Skill, Gacha 패널 오브젝트를 연결합니다. (각 패널 닫기 버튼에 CloseAll 연결)

DreamCottonEnhance:

Upgrade Costs: 1, 3, 5, 10, 20 입력.

Level Sprites: 0단계~5단계 이미지를 리스트에 넣습니다.

Ink Gacha Table: 랜덤 잉크가 나올 ItemDropTable 에셋 연결.

Ink Red/Blue...: 색상 교환 시 줄 ItemData 에셋 연결.

UI 버튼 연결:

강화 버튼: OnClickUpgradeHP 등 연결.

스킬 버튼: Skill_DamageBuff 등 연결.

교환 버튼: ExchangeInk 연결 후 매개변수(String)에 "Red", "Blue" 등 입력.
*/
