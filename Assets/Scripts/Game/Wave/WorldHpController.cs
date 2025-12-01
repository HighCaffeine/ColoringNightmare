using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트 사용을 위해 필요
using TMPro;          // 텍스트 사용 (TextMeshPro 기준)

public class WorldHpController : GenericSingleton<WorldHpController>
{
    [Header("UI Components")]
    [SerializeField] private Image fillImage;

    [Header("Settings")]
    [SerializeField] private int worldHP;
    [SerializeField] private UnityEngine.Events.UnityEvent OnGameOver;

    private int maxHP;

    new void Awake()
    {
        base.Awake();
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

    public void RecoverHP(int amount)
    {
        worldHP = Mathf.Min(worldHP + amount, maxHP);
        UpdateUI();
        Debug.Log($"World HP Recovered: {worldHP}/{maxHP}");
    }

    private void UpdateUI()
    {
        if (fillImage != null)
        {
            float hpRatio = (float)worldHP / maxHP;
            fillImage.fillAmount = Mathf.Lerp(0.0f, 1.0f, hpRatio);
        }
    }
}