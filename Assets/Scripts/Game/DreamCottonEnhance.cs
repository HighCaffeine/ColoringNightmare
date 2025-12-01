using UnityEngine;
using TMPro;

public class DreamCottonEnhance : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerController player; // 강화할 대상

    [Header("Upgrade Settings")]
    [SerializeField] private int hpUpgradeAmount = 10;
    [SerializeField] private float speedUpgradeAmount = 0.5f;
    [SerializeField] private int damageUpgradeAmount = 1;

    public void UpgradeMaxHP()
    {
        if (player == null || player.info == null) return;

        // 데이터 직접 수정 (주의: 게임 끄면 저장됨. 런타임 복제본 쓰는 게 좋음)
        player.info.maxHp += hpUpgradeAmount;

        // 현재 체력도 같이 채워줄지 결정
        // player.Heal(hpUpgradeAmount); 

        Debug.Log($"Max HP Upgraded! Now: {player.info.maxHp}");
    }

    public void UpgradeMoveSpeed()
    {
        if (player == null || player.info == null) return;

        player.info.speed += speedUpgradeAmount;
        Debug.Log($"Speed Upgraded! Now: {player.info.speed}");
    }

    public void UpgradeDamage()
    {
        if (player == null || player.info == null) return;

        player.info.dmg += damageUpgradeAmount;
        Debug.Log($"Damage Upgraded! Now: {player.info.dmg}");
    }
}