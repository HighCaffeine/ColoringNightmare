using UnityEngine;
using System.Collections;
using System.Collections.Generic; // List 사용
using TMPro; // TextMeshProUGUI 사용

[System.Serializable]
public struct SimilarityEffectData
{
    [Tooltip("유사도 (0.0 = 0%, 1.0 = 100%)")]
    [Range(0f, 1f)]
    public float probabilityThreshold;
    public Sprite sprite;
}

public class DrawSimilarityEffect : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private UnityEngine.UI.Image targetImage;

    [Header("Settings")]
    [SerializeField]
    private float displayDuration = 1.5f; // 멘트가 표시될 시간

    [Header("Effect Tiers")]
    [SerializeField]
    private List<SimilarityEffectData> effectTiers;

    void Awake()
    {
        if (targetImage != null)
        {
            targetImage.gameObject.SetActive(false);
        }

        effectTiers.Sort((a, b) => a.probabilityThreshold.CompareTo(b.probabilityThreshold));
    }

    public void ShowEffect(float diceValue)
    {
        if (targetImage == null || effectTiers.Count == 0)
        {
            return;
        }

        SimilarityEffectData currentTier = effectTiers[0];

        foreach (var tier in effectTiers)
        {
            if (diceValue <= tier.probabilityThreshold)
            {
                currentTier = tier;
                break;
            }
        }

        targetImage.sprite = currentTier.sprite;
        targetImage.gameObject.SetActive(true);

        CancelInvoke(nameof(HideEffect));

        Invoke(nameof(HideEffect), displayDuration);
    }

    private void HideEffect()
    {
        if (targetImage != null)
        {
            targetImage.gameObject.SetActive(false);
        }
    }
}