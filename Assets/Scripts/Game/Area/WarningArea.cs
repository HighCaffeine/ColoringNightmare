using UnityEngine;
using System.Collections;

public enum WarningFillType
{
    CenterExpand,   // 중앙에서 전체로 커짐
    Horizontal,     // 중앙에서 좌우로
    Vertical,       // 중앙에서 위아래로
    TopToBottom,    // 위 -> 아래
    BottomToTop,    // 아래 -> 위
    LeftToRight,    // 좌 -> 우
    RightToLeft     // 우 -> 좌
}

[ExecuteInEditMode]
[ExecuteAlways]
public class WarningArea : MonoBehaviour
{
    [SerializeField] private SpriteRenderer fillSprite;
    [SerializeField] private SpriteRenderer backgroundSprite;

    [Header("Settings")]
    [Tooltip("기본 채우기 방식")]
    [SerializeField] private WarningFillType defaultFillType = WarningFillType.CenterExpand;

    [Tooltip("애니메이션 속도 그래프 (기본: Linear / 추천: Ease In Out)")]
    [SerializeField] private AnimationCurve fillCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField] private Sprite areaShape;

    private void Awake()
    {
        if (fillSprite != null)
        {
            fillSprite.transform.localScale = Vector3.zero;
        }
    }

    public void Setup(float duration, WarningFillType fillType)
    {
        if (fillSprite == null || areaShape == null) return;
        fillSprite.sprite = areaShape;
        backgroundSprite.sprite = areaShape;

        StartCoroutine(FillRoutine(duration, fillType));
    }

    public void Setup(float duration)
    {
        Setup(duration, defaultFillType);
    }


    public float TestDuration = 3.0f;

#if UNITY_EDITOR_64
    public void OnValidate()
    {
        Setup(TestDuration, defaultFillType);
    }
#endif

    private IEnumerator FillRoutine(float duration, WarningFillType fillType)
    {
        float elapsed = 0f;

        ApplyTransform(0f, fillType);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = fillCurve.Evaluate(t);

            ApplyTransform(smoothT, fillType);

            yield return null;
        }

        ApplyTransform(1f, fillType);
    }

    private void ApplyTransform(float t, WarningFillType type)
    {
        Vector3 newScale = Vector3.one;
        Vector3 newPos = Vector3.zero;

        switch (type)
        {
            case WarningFillType.CenterExpand:
                newScale = new Vector3(t, t, 1f);
                break;

            case WarningFillType.Horizontal:
                newScale = new Vector3(t, 1f, 1f);
                break;

            case WarningFillType.Vertical:
                newScale = new Vector3(1f, t, 1f);
                break;

            case WarningFillType.TopToBottom:
                newScale = new Vector3(1f, t, 1f);
                newPos = new Vector3(0f, 0.5f * (1f - t), 0f);
                break;

            case WarningFillType.BottomToTop:
                newScale = new Vector3(1f, t, 1f);
                newPos = new Vector3(0f, -0.5f * (1f - t), 0f);
                break;

            case WarningFillType.LeftToRight:
                newScale = new Vector3(t, 1f, 1f);
                newPos = new Vector3(-0.5f * (1f - t), 0f, 0f);
                break;

            case WarningFillType.RightToLeft:
                newScale = new Vector3(t, 1f, 1f);
                newPos = new Vector3(0.5f * (1f - t), 0f, 0f);
                break;
        }

        fillSprite.transform.localScale = newScale;
        fillSprite.transform.localPosition = newPos;
    }
}