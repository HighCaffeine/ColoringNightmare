using UnityEngine;
using TMPro;

public class DreamCottonEnhance : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerController player;

    [Header("Upgrade Stats")]
    [SerializeField] private int hpUpgradeAmount = 10;      // 최대 체력 증가량
    [SerializeField] private float speedUpgradeAmount = 0.5f; // 이동 속도 증가량
    [SerializeField] private int damageUpgradeAmount = 1;     // 공격력 증가량

    [SerializeField] private TextMeshProUGUI statText;

    private void Start()
    {
        UpdateStatUI();
    }

    public void OnClickUpgradeMaxHP()
    {
        if (player == null || player.info == null) return;

        player.info.maxHp += hpUpgradeAmount;

        Debug.Log($"[DreamCotton] Max HP Upgraded: {player.info.maxHp}");
        UpdateStatUI();
    }

    public void OnClickUpgradeSpeed()
    {
        if (player == null || player.info == null) return;

        player.info.speed += speedUpgradeAmount;

        Debug.Log($"[DreamCotton] Speed Upgraded: {player.info.speed}");
        UpdateStatUI();
    }

    public void OnClickUpgradeDamage()
    {
        if (player == null || player.info == null) return;

        player.info.dmg += damageUpgradeAmount;

        Debug.Log($"[DreamCotton] Damage Upgraded: {player.info.dmg}");
        UpdateStatUI();
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