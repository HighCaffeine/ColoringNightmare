using UnityEngine;
using TMPro;

public class DreamCottonEnhance : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerController player;
    [SerializeField] private int currentCottonCount = 0;

    [Header("Upgrade Stats")]
    [SerializeField] private int hpUpgradeAmount = 3;      // 최대 체력 증가량
    [SerializeField] private float speedUpgradeAmount = 0.2f; // 이동 속도 증가량
    [SerializeField] private int damageUpgradeAmount = 1;     // 공격력 증가량

    [SerializeField] private TextMeshProUGUI statText;


    [SerializeField] private TMPro.TextMeshProUGUI[] cottonCount;

    [Space(1f)]
    [Header("Enhance")]
    [SerializeField] private UnityEngine.UI.Image hpImage;
    [SerializeField] private TMPro.TextMeshProUGUI hpTxt;
    private int hpImageIndex = 0;
    [SerializeField] private UnityEngine.UI.Image speedImage;
    [SerializeField] private TMPro.TextMeshProUGUI speedTxt;
    private int speedImageIndex = 0;
    [SerializeField] private UnityEngine.UI.Image dmgImage;
    [SerializeField] private TMPro.TextMeshProUGUI dmgTxt;
    private int dmgImageIndex = 0;

    [Header("Req Cotton Currency")]
    [SerializeField] private int[] reqArr = { 1, 3, 5, 10, 20 };
    [SerializeField] private Sprite[] levelSprite;


    [Space(1f)]
    [Header("Skill Settings")]
    [SerializeField] private TMPro.TextMeshProUGUI dmgSkillTxt;
    [SerializeField] private int reqDmg = 5;
    [Tooltip("공격력 버프 지속 시간")]
    [SerializeField] private float dmgBuffDuration = 15f;
    [Tooltip("일시적 공격력 증가량")]
    [SerializeField] private int dmgBuffAmount = 3;

    [Space(5f)]
    [SerializeField] private TMPro.TextMeshProUGUI hpSkillTxt;
    [SerializeField] private int reqWorldHp = 5;
    [Tooltip("월드 HP 회복량")]
    [SerializeField] private int worldHealAmount = 1;

    [Space(5f)]
    [SerializeField] private TMPro.TextMeshProUGUI inkSkillTxt;
    [SerializeField] private int reqInkCreate = 5;
    [Tooltip("잉크 뽑기 테이블 (ItemDropTable)")]
    [SerializeField] private ItemDropTable inkGachaTable;
    [Tooltip("아이템이 스폰될 위치")]
    [SerializeField] private Transform gachaSpawnPoint;

    private void Start()
    {
        UpdateStatUI();

        hpTxt.text = reqArr[0].ToString();
        speedTxt.text = reqArr[0].ToString();
        dmgTxt.text = reqArr[0].ToString();

        dmgSkillTxt.text = reqDmg.ToString();
        hpSkillTxt.text = reqWorldHp.ToString();
        inkSkillTxt.text = reqInkCreate.ToString();
    }

    public void OnClickUpgradeMaxHP()
    {
        if (player == null || player.info == null) return;
        if (!UseCotton(reqArr[hpImageIndex], hpImageIndex)) return;

        hpImageIndex++;
        hpTxt.text = reqArr[hpImageIndex].ToString();
        hpImage.sprite = levelSprite[hpImageIndex];
        player.info.maxHp += hpUpgradeAmount;

        Debug.Log($"[DreamCotton] Max HP Upgraded: {player.info.maxHp}");
        UpdateStatUI();
    }

    public void OnClickUpgradeSpeed()
    {
        if (player == null || player.info == null) return;
        if (!UseCotton(reqArr[speedImageIndex], speedImageIndex)) return;

        speedImageIndex++;
        speedTxt.text = reqArr[speedImageIndex].ToString();
        speedImage.sprite = levelSprite[speedImageIndex];
        player.info.speed += speedUpgradeAmount;

        Debug.Log($"[DreamCotton] Speed Upgraded: {player.info.speed}");
        UpdateStatUI();
    }

    public void OnClickUpgradeDamage()
    {
        if (player == null || player.info == null) return;
        if (!UseCotton(reqArr[dmgImageIndex], dmgImageIndex)) return;

        dmgImageIndex++;
        dmgTxt.text = reqArr[dmgImageIndex].ToString();
        dmgImage.sprite = levelSprite[dmgImageIndex];
        player.info.dmg += damageUpgradeAmount;

        Debug.Log($"[DreamCotton] Damage Upgraded: {player.info.dmg}");
        UpdateStatUI();
    }

    private bool UseCotton(int req, int index)
    {
        if (index >= reqArr.Length) return false;
        if (req > currentCottonCount) return false;

        currentCottonCount -= req;

        foreach (var txt in cottonCount)
        {
            if (txt.gameObject.activeSelf)
            {
                txt.text = currentCottonCount.ToString();
            }
        }
        return true;
    }

    public void HPSkill()
    {
        if (WorldHpController.Instance == null) return;
        if (!TryConsumeCotton(reqWorldHp)) return;

        WorldHpController.Instance.RecoverHP(worldHealAmount);
    }

    // 2. 일시적 공격력 증가
    public void DmgSkill()
    {
        if (player == null || player.info == null) return;
        if (!TryConsumeCotton(reqDmg)) return;

        StartCoroutine(DmgBuffRoutine());
    }

    private System.Collections.IEnumerator DmgBuffRoutine()
    {
        player.info.dmg += dmgBuffAmount;
        Debug.Log($"<color=red>Attack Buff Start!</color> (+{dmgBuffAmount})");
        UpdateStatUI();

        yield return new WaitForSeconds(dmgBuffDuration);

        player.info.dmg -= dmgBuffAmount;
        Debug.Log("Attack Buff End.");
        UpdateStatUI();
    }

    // 3. 잉크 뽑기 (랜덤 생성)
    public void InkSkill()
    {
        if (inkGachaTable == null || MonsterManager.Instance == null) return;
        if (!TryConsumeCotton(reqInkCreate)) return;

        Vector3 spawnPos = (gachaSpawnPoint != null) ? gachaSpawnPoint.position : player.transform.position;

        // MonsterManager를 통해 아이템 생성 (획득 이벤트 연결됨)
        MonsterManager.Instance.SpawnItemsFromTable(inkGachaTable, spawnPos);
        Debug.Log("Ink Gacha Spawned!");
    }

    // --- [유틸리티] ---

    private bool TryConsumeCotton(int amount)
    {
        if (currentCottonCount < amount) return false;

        currentCottonCount -= amount;
        UpdateCottonUI();
        return true;
    }

    private void UpdateCottonUI()
    {
        foreach (var txt in cottonCount)
        {
            if (txt != null && txt.gameObject.activeSelf)
            {
                txt.text = currentCottonCount.ToString();
            }
        }
    }

    private void UpdateStatUI()
    {
        if (statText != null && player != null && player.info != null)
        {
            statText.text = $"HP: {player.info.maxHp}\nATK: {player.info.dmg}\nSPD: {player.info.speed:F1}";
        }

        var hpController = player.GetComponent<PlayerHPController>();
        if (hpController != null) hpController.UpdateHpGauge();
    }
}