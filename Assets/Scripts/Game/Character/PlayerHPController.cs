using UnityEngine;

public class PlayerHPController : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image hpImage;

    [SerializeField] private Transform damagedPanel;
    [SerializeField] private float panelDisplayTime = 0.2f;

    private PlayerController playerController;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void OnDamagedPanelEvent()
    {
        SetActivePanel(true);
        Invoke(nameof(SetActivePanel), panelDisplayTime);

        UpdateHpGauge();
    }

    public void UpdateHpGauge()
    {
        float currentHP = playerController.GetHP();
        float maxHP = playerController.GetMaxHP();

        hpImage.fillAmount = currentHP / maxHP;
    }


    private void SetActivePanel(bool active) { damagedPanel.gameObject.SetActive(active); }
}
