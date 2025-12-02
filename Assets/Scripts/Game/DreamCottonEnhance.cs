using UnityEngine;
using TMPro;
using System.Collections;

public class DreamCottonEnhance : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerController player;

    [Header("Currencies")]
    [SerializeField] private int currentCottonCount = 0;
    [SerializeField] private int currentBlackInkCount = 0; // [★신규★] 검은 잉크 보유량

    [Header("Upgrade Settings")]
    [SerializeField] private int maxLevel = 5;
    [SerializeField] private int hpPerLevel = 3;
    [SerializeField] private float speedPercent = 0.1f;
    [SerializeField] private float dmgPercent = 0.2f;

    [Header("Costs")]
    [SerializeField] private int[] upgradeCosts = { 1, 3, 5, 10, 20 };
    [SerializeField] private int skillCost_Dmg = 5;
    [SerializeField] private int skillCost_Heal = 5;
    [SerializeField] private int skillCost_Ink = 5;

    // 가챠 비용 (검은 잉크 1개 고정)
    private const int GACHA_COST_BLACK_INK = 1;

    [Header("UI - Cotton Count")]
    [Tooltip("강화, 스킬 패널에 있는 '보유 코튼' 텍스트들")]
    [SerializeField] private TextMeshProUGUI[] cottonCountTexts;

    [Header("UI - Black Ink Count")]
    [Tooltip("가챠 패널에 있는 '보유 검은 잉크' 텍스트")]
    [SerializeField] private TextMeshProUGUI blackInkCountText; // [★신규★]

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
    [SerializeField] private TextMeshProUGUI gachaCostTxt; // 필요하다면 "1" 표시

    [Header("Skill Settings")]
    [SerializeField] private float dmgBuffDuration = 15f;
    [SerializeField] private float dmgBuffRatio = 0.5f;
    [SerializeField] private int worldHealAmount = 3;

    [Header("Gacha Data")]
    [Tooltip("가챠 결과물 테이블 (빨강/노랑/파랑 잉크 + 드림코튼 포함)")]
    [SerializeField] private ItemDropTable gachaDropTable; // [★수정★] 가챠 테이블
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private ItemDropTable inkSkillTable; // 스킬용(잉크생성) 테이블

    // 내부 상태
    private int hpLevel = 0;
    private int speedLevel = 0;
    private int dmgLevel = 0;

    private void Start()
    {
        UpdateAllUI();
    }

    // --- [UI 통합 갱신] ---
    private void UpdateAllUI()
    {
        // 1. 보유 코튼 개수 (강화/스킬 패널)
        if (cottonCountTexts != null)
        {
            foreach (var txt in cottonCountTexts)
                if (txt != null) txt.text = currentCottonCount.ToString();
        }

        // 2. 보유 검은 잉크 개수 (가챠 패널) [★신규★]
        if (blackInkCountText != null)
        {
            blackInkCountText.text = currentBlackInkCount.ToString();
        }

        // 3. 강화 패널
        UpdateEnhanceSlot(hpLevel, hpCostTxt, hpBarImage);
        UpdateEnhanceSlot(speedLevel, speedCostTxt, speedBarImage);
        UpdateEnhanceSlot(dmgLevel, dmgCostTxt, dmgBarImage);

        // 4. 스탯 표시
        if (statText != null && player != null && player.info != null)
            statText.text = $"HP: {player.info.maxHp}\nATK: {player.info.dmg}\nSPD: {player.info.speed:F1}";

        // 5. 비용 표시
        if (dmgSkillCostTxt) dmgSkillCostTxt.text = skillCost_Dmg.ToString();
        if (hpSkillCostTxt) hpSkillCostTxt.text = skillCost_Heal.ToString();
        if (inkSkillCostTxt) inkSkillCostTxt.text = skillCost_Ink.ToString();
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

    // --- [가챠 (Gacha)] ---

    // [★신규★] 가챠 버튼 클릭 시 호출
    public void PlayGacha()
    {
        // 검은 잉크 소모 체크
        if (currentBlackInkCount < GACHA_COST_BLACK_INK)
        {
            Debug.Log("검은 잉크가 부족합니다!");
            return;
        }

        // 소모
        currentBlackInkCount -= GACHA_COST_BLACK_INK;
        UpdateAllUI();

        // 랜덤 보상 소환
        if (gachaDropTable != null && MonsterManager.Instance != null)
        {
            Vector3 pos = (spawnPoint != null) ? spawnPoint.position : player.transform.position;
            MonsterManager.Instance.SpawnItemsFromTable(gachaDropTable, pos);
            Debug.Log("가챠 성공!");
        }
    }

    // --- [강화 (Enhance)] ---

    public void OnClickUpgradeHP()
    {
        if (hpLevel >= maxLevel) return;
        if (TryConsumeCotton(upgradeCosts[hpLevel]))
        {
            hpLevel++;
            player.info.maxHp += hpPerLevel;
            player.GetComponent<PlayerHPController>()?.UpdateHpGauge();
            UpdateAllUI();
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

    // --- [스킬 (Skill)] ---

    public void Skill_DamageBuff()
    {
        if (TryConsumeCotton(skillCost_Dmg))
        {
            StartCoroutine(DmgBuffRoutine());
            UpdateAllUI();
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

    public void Skill_WorldHeal()
    {
        if (TryConsumeCotton(skillCost_Heal))
        {
            WorldHpController.Instance?.RecoverHP(worldHealAmount);
            UpdateAllUI();
        }
    }

    public void Skill_InkRandom()
    {
        if (TryConsumeCotton(skillCost_Ink))
        {
            Vector3 pos = (spawnPoint != null) ? spawnPoint.position : player.transform.position;
            if (inkSkillTable != null)
                MonsterManager.Instance.SpawnItemsFromTable(inkSkillTable, pos);
            UpdateAllUI();
        }
    }

    // --- [자원 관리] ---

    public void AddCotton(int amount)
    {
        currentCottonCount += amount;
        UpdateAllUI();
    }

    // [★신규★] 검은 잉크 획득 시 호출 (MonsterManager 등에서)
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
