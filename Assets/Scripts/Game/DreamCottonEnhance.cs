using UnityEngine;
using TMPro;
using System.Collections;

public class DreamCottonEnhance : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerController player;
    [SerializeField] private int currentCottonCount = 0;

    [Header("Upgrade Settings")]
    [SerializeField] private int maxLevel = 5; // 최대 5단계

    // [기획 반영] 단계별 상승치
    [SerializeField] private int hpPerLevel = 3;        // 체력 +3
    [SerializeField] private float speedPercent = 0.1f; // 이속 +10%
    [SerializeField] private float dmgPercent = 0.2f;   // 공격력 +20%

    [Header("Skill Settings")]
    [Tooltip("공격력 버프 지속 시간")]
    [SerializeField] private float dmgBuffDuration = 15f;
    [Tooltip("공격력 증폭 비율 (0.5 = 50%)")]
    [SerializeField] private float dmgBuffRatio = 0.5f;
    [Tooltip("월드 HP 회복량")]
    [SerializeField] private int worldHealAmount = 3;

    [Header("Costs")]
    [SerializeField] private int[] upgradeCosts = { 1, 3, 5, 10, 20 }; // 단계별 비용
    [SerializeField] private int skillCost_Dmg = 5;
    [SerializeField] private int skillCost_Heal = 5;
    [SerializeField] private int skillCost_Ink = 5;
    [SerializeField] private int gachaCost = 3; // 색상 교환 비용

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI cottonText; // 보유 코튼 표시
    [SerializeField] private TextMeshProUGUI statText;   // 현재 스탯 표시

    // 강화 UI (레벨/비용 표시용)
    [SerializeField] private TextMeshProUGUI hpCostTxt;
    [SerializeField] private TextMeshProUGUI speedCostTxt;
    [SerializeField] private TextMeshProUGUI dmgCostTxt;
    [SerializeField] private UnityEngine.UI.Image hpBarImage; // 단계 이미지(스프라이트)
    [SerializeField] private UnityEngine.UI.Image speedBarImage;
    [SerializeField] private UnityEngine.UI.Image dmgBarImage;
    [SerializeField] private Sprite[] levelSprites; // 0~5단계 이미지

    [Header("Gacha & Skill Data")]
    [SerializeField] private ItemDropTable inkGachaTable;
    [SerializeField] private Transform spawnPoint;
    // 색상 교환용 아이템 데이터들 (인스펙터에서 할당)
    [SerializeField] private ItemData inkRed;
    [SerializeField] private ItemData inkBlue;
    [SerializeField] private ItemData inkYellow;
    [SerializeField] private ItemData inkBlack;

    // 내부 상태
    private int hpLevel = 0;
    private int speedLevel = 0;
    private int dmgLevel = 0;

    private void Start()
    {
        UpdateUI();
    }

    // --- [1. 강화 (Enhance)] ---

    public void OnClickUpgradeHP()
    {
        if (hpLevel >= maxLevel) return;
        int cost = upgradeCosts[hpLevel];

        if (TryConsumeCotton(cost))
        {
            hpLevel++;
            player.info.maxHp += hpPerLevel; // 체력 +3
                                             // 현재 체력도 같이 채워줄지 여부 (선택)
                                             // player.Heal(hpPerLevel); 

            Debug.Log($"HP Upgrade Lv.{hpLevel}: MaxHP {player.info.maxHp}");
            UpdateUI();
        }
    }

    public void OnClickUpgradeSpeed()
    {
        if (speedLevel >= maxLevel) return;
        int cost = upgradeCosts[speedLevel];

        if (TryConsumeCotton(cost))
        {
            speedLevel++;
            // 기존 속도의 10% 만큼 증가
            float increase = player.info.speed * speedPercent;
            player.info.speed += increase;

            Debug.Log($"Speed Upgrade Lv.{speedLevel}: Speed {player.info.speed}");
            UpdateUI();
        }
    }

    public void OnClickUpgradeDamage()
    {
        if (dmgLevel >= maxLevel) return;
        int cost = upgradeCosts[dmgLevel];

        if (TryConsumeCotton(cost))
        {
            dmgLevel++;
            // 공격력 20% 증가 (최소 1 증가 보장)
            int increase = Mathf.Max(1, Mathf.RoundToInt(player.info.dmg * dmgPercent));
            player.info.dmg += increase;

            Debug.Log($"Damage Upgrade Lv.{dmgLevel}: Dmg {player.info.dmg}");
            UpdateUI();
        }
    }

    // --- [2. 스킬 (Skill)] ---

    public void Skill_DamageBuff()
    {
        if (!TryConsumeCotton(skillCost_Dmg)) return;
        StartCoroutine(DmgBuffRoutine());
    }

    private IEnumerator DmgBuffRoutine()
    {
        // 50% 증폭
        int buffAmount = Mathf.Max(1, Mathf.RoundToInt(player.info.dmg * dmgBuffRatio));
        player.info.dmg += buffAmount;
        Debug.Log($"<color=red>Attack Buff! (+{buffAmount})</color>");

        UpdateUI();

        yield return new WaitForSeconds(dmgBuffDuration);

        player.info.dmg -= buffAmount;
        Debug.Log("Attack Buff End.");
        UpdateUI();
    }

    public void Skill_WorldHeal()
    {
        if (!TryConsumeCotton(skillCost_Heal)) return;

        // 월드 HP 회복
        if (WorldHpController.Instance != null)
        {
            WorldHpController.Instance.RecoverHP(worldHealAmount);
            Debug.Log($"World HP Recovered (+{worldHealAmount})");
        }
    }

    public void Skill_InkRandom()
    {
        if (!TryConsumeCotton(skillCost_Ink)) return;

        // 랜덤 생성 (DropTable)
        Vector3 pos = (spawnPoint != null) ? spawnPoint.position : player.transform.position;
        MonsterManager.Instance.SpawnItemsFromTable(inkGachaTable, pos);
    }

    // --- [3. 뽑기/교환 (Gacha)] ---
    // UI 버튼에서 색상을 지정해서 호출 (Red, Blue, Yellow, Black)
    public void ExchangeInk(string colorName)
    {
        if (!TryConsumeCotton(gachaCost)) return;

        ItemData targetItem = null;
        switch (colorName)
        {
            case "Red": targetItem = inkRed; break;
            case "Blue": targetItem = inkBlue; break;
            case "Yellow": targetItem = inkYellow; break;
            case "Black": targetItem = inkBlack; break;
        }

        if (targetItem != null && DropManager.Instance != null)
        {
            // DropManager의 SpawnInk (또는 SpawnItem) 활용
            // (DropManager.cs에 public SpawnItem이 있어야 함. 없으면 SpawnInk 사용)
            // 여기서는 편의상 몬스터 매니저의 콜백을 재사용하거나 직접 구현
            // 간단하게 DropManager에 직접 요청:
            Vector3 pos = (spawnPoint != null) ? spawnPoint.position : player.transform.position;
            // DropManager.Instance.SpawnItem(targetItem, pos, ...); // 이 함수가 private이라면 public으로 변경 필요
            // 대안: MonsterManager 콜백 활용
            MonsterManager.Instance.SpawnItemsFromTable(CreateSingleItemTable(targetItem), pos);
        }
    }

    // 단일 아이템 테이블을 임시로 만드는 헬퍼 (교환용)
    private ItemDropTable CreateSingleItemTable(ItemData item)
    {
        ItemDropTable table = ScriptableObject.CreateInstance<ItemDropTable>();
        table.itemDropTable = new System.Collections.Generic.List<ItemDropData>();
        table.itemDropTable.Add(new ItemDropData { itemData = item, dropChance = 1f, minDropAmount = 1, maxDropAmount = 1 });
        return table;
    }

    // --- [유틸리티] ---

    public void AddCotton(int amount)
    {
        currentCottonCount += amount;
        UpdateUI();
    }

    private bool TryConsumeCotton(int amount)
    {
        if (currentCottonCount < amount) return false;
        currentCottonCount -= amount;
        UpdateUI();
        return true;
    }

    private void UpdateUI()
    {
        if (cottonText != null) cottonText.text = currentCottonCount.ToString();

        // 스탯 표시
        if (statText != null && player != null)
            statText.text = $"HP: {player.info.maxHp}\nATK: {player.info.dmg}\nSPD: {player.info.speed:F1}";

        // 강화 비용 및 단계 이미지 업데이트
        UpdateEnhanceUI(hpLevel, hpCostTxt, hpBarImage);
        UpdateEnhanceUI(speedLevel, speedCostTxt, speedBarImage);
        UpdateEnhanceUI(dmgLevel, dmgCostTxt, dmgBarImage);
    }

    private void UpdateEnhanceUI(int level, TextMeshProUGUI costTxt, UnityEngine.UI.Image barImage)
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
}