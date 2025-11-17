using UnityEngine;
using System.Collections;
using System.Collections.Generic; // List 사용
using TMPro; // TextMeshProUGUI 사용

/// <summary>
/// [★신규★] 확률 구간별 데이터를 담을 구조체
/// </summary>
[System.Serializable]
public struct SimilarityEffectData
{
    [Tooltip("유사도 (0.0 = 0%, 1.0 = 100%)")]
    [Range(0f, 1f)]
    public float probabilityThreshold;

    [Tooltip("표시할 멘트 (예: Good!, Perfect!)")]
    public string message;

    [Tooltip("멘트의 색상")]
    public Color color;
}

public class DrawSimilarityEffect : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private TextMeshProUGUI effectText; // 멘트 표시

    [Header("Settings")]
    [SerializeField]
    private float displayDuration = 1.5f; // 멘트가 표시될 시간

    [Header("Effect Tiers")]
    [SerializeField]
    private List<SimilarityEffectData> effectTiers;

    void Awake()
    {
        if (effectText != null)
        {
            effectText.gameObject.SetActive(false);
        }

        effectTiers.Sort((a, b) => a.probabilityThreshold.CompareTo(b.probabilityThreshold));
    }

    public void ShowEffect(float diceValue)
    {
        if (effectText == null || effectTiers.Count == 0)
        {
            return;
        }

        SimilarityEffectData currentTier = effectTiers[0];

        effectText.text = $"제작 실패";

        foreach (var tier in effectTiers)
        {
            if (diceValue >= tier.probabilityThreshold)
            {
                currentTier = tier;
            }
            else
            {
                break;
            }
        }

        effectText.text = currentTier.message;
        effectText.color = currentTier.color;
        effectText.gameObject.SetActive(true);

        CancelInvoke(nameof(HideEffect));

        Invoke(nameof(HideEffect), displayDuration);
    }

    private void HideEffect()
    {
        if (effectText != null)
        {
            effectText.gameObject.SetActive(false);
        }
    }
}