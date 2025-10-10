using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class OutlineController : MonoBehaviour
{
    private SpriteRenderer outlineRenderer;
    private SpriteRenderer mainRenderer;

    [Header("아웃라인 설정")]
    public Color outlineColor = Color.black;
    [Range(1.0f, 1.2f)]
    public float outlineThickness = 1.05f;

    void Awake()
    {
        mainRenderer = GetComponent<SpriteRenderer>();
        CreateOutline();
    }

    void CreateOutline()
    {
        // 아웃라인 스프라이트 생성
        GameObject outlineObject = new GameObject("Outline");
        outlineObject.transform.parent = transform;

        outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();

        // 원본 스프라이트의 속성 복사
        outlineRenderer.sprite = mainRenderer.sprite;
        outlineRenderer.color = outlineColor;
        outlineRenderer.transform.localScale = Vector3.one * outlineThickness;

        // 렌더링 순서 설정
        outlineRenderer.sortingLayerID = mainRenderer.sortingLayerID;
        outlineRenderer.sortingOrder = mainRenderer.sortingOrder - 1;
    }
}