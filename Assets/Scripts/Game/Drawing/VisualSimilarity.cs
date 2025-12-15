using UnityEngine;

public class VisualSimilarity : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image targetFillImage;

    public void Init()
    {
        UpdateFillAmount(0.0f);
    }

    public void UpdateFillAmount(float amount)
    {
        Debug.Log($"{amount}");
        targetFillImage.fillAmount = amount;
    }
}