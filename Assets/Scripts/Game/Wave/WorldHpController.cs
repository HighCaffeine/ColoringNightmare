using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldHpController : GenericSingleton<WorldHpController>
{
    [Header("UI Components")]
    [SerializeField] private Image fillImage;

    [Header("Settings")]
    [SerializeField] private int worldHP;
    [SerializeField] private UnityEngine.Events.UnityEvent OnGameOver;

    private int maxHP;

    private bool isGameOver = false;

    new void Awake()
    {
        base.Awake();
        maxHP = worldHP;
        UpdateUI();
    }

    public void SubHP()
    {
        worldHP--;

        if (worldHP <= 0 && !isGameOver)
        {
            isGameOver = true;
            worldHP = 0;

            UpdateUI();

            OnGameOver?.Invoke();
            GameManager.Instance.GameOver(1.5f);
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