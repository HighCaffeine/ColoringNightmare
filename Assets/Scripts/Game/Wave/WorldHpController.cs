using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트 사용을 위해 필요
using TMPro;          // 텍스트 사용 (TextMeshPro 기준)

public class WorldHpController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image fillImage;

    [Header("Settings")]
    [SerializeField] private int worldHP;
    [SerializeField] private UnityEngine.Events.UnityEvent OnGameOver;

    private int maxHP;

    void Awake()
    {
        maxHP = worldHP;
        UpdateUI();
    }

    public void SubHP()
    {
        worldHP--;

        if (worldHP <= 0)
        {
            worldHP = 0;
            OnGameOver?.Invoke();
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (fillImage != null)
        {
            float hpRatio = (float)worldHP / maxHP;
            fillImage.fillAmount = Mathf.Lerp(0.1f, 1.0f, hpRatio);
        }
    }
}