using UnityEngine;
using UnityEngine.UI; // Image를 쓰려면 필요

public class PlayerHPController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image hpImage;

    [Header("Damage Effect")]
    [SerializeField] private Transform damagedPanel;
    [SerializeField] private float panelDisplayTime = 0.2f;

    public void Init(int currentHP, int maxHP)
    {
        UpdateHpGauge(currentHP, maxHP);
        if (damagedPanel != null) damagedPanel.gameObject.SetActive(false);
    }

    public void UpdateHpGauge(int currentHP, int maxHP)
    {
        if (hpImage != null && maxHP > 0)
        {
            hpImage.fillAmount = (float)currentHP / maxHP;
        }
    }

    public void OnDamagedPanelEvent()
    {
        if (damagedPanel != null)
        {
            damagedPanel.gameObject.SetActive(true);

            CancelInvoke(nameof(OffPanel));
            Invoke(nameof(OffPanel), panelDisplayTime);
        }
    }

    private void OffPanel()
    {
        if (damagedPanel != null) damagedPanel.gameObject.SetActive(false);
    }
}