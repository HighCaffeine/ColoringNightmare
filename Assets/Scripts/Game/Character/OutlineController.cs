using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class OutlineController : MonoBehaviour
{
    private SpriteRenderer mainRenderer;
    private SpriteRenderer outlineRenderer;
    private Material instancedMaterial;
    private bool isOutlineActive;

    [Header("아웃라인 머티리얼")]
    [SerializeField] private Material outlineMaterialBase;

    [Header("아웃라인 설정")]
    [Tooltip("아웃라인 색상")]
    public Color outlineColor = Color.HSVToRGB(45, 100, 100);

    [Tooltip("아웃라인 두께")]
    [Range(0f, 3f)]
    public float outlineThickness = 3.0f;
    private const string PROP_MAIN_TEX = "_MainTex";
    private const string PROP_COLOR = "_OutLineColor";
    private const string PROP_THICKNESS = "_OutLineThickness";

    void Awake()
    {
        mainRenderer = GetComponent<SpriteRenderer>();

        if (outlineMaterialBase == null)
        {
            return;
        }

        instancedMaterial = new Material(outlineMaterialBase);

        // --- 아웃라인 오브젝트 생성 ---
        GameObject outlineObj = new GameObject("OutlineEffect");
        outlineObj.transform.SetParent(this.transform);
        outlineObj.transform.localPosition = Vector3.zero;
        outlineObj.transform.localRotation = Quaternion.identity;
        outlineObj.transform.localScale = Vector3.one;

        outlineRenderer = outlineObj.AddComponent<SpriteRenderer>();
        outlineRenderer.material = instancedMaterial;

        SyncRendererProperties();
        SetOutLineObj(false);
    }
    private void SyncRendererProperties()
    {
        if (mainRenderer == null || outlineRenderer == null) return;

        outlineRenderer.sprite = mainRenderer.sprite;

        outlineRenderer.sortingLayerID = mainRenderer.sortingLayerID;
        outlineRenderer.sortingOrder = mainRenderer.sortingOrder - 1;

        outlineRenderer.flipX = mainRenderer.flipX;
        outlineRenderer.flipY = mainRenderer.flipY;

        if (instancedMaterial != null && mainRenderer.sprite != null)
        {
            instancedMaterial.SetTexture(PROP_MAIN_TEX, mainRenderer.sprite.texture);
        }
    }

    private void ApplyMaterialProperties()
    {
        if (instancedMaterial == null) return;

        instancedMaterial.SetColor(PROP_COLOR, outlineColor);

        float currentThickness = isOutlineActive ? outlineThickness : 0.0f;
        instancedMaterial.SetFloat(PROP_THICKNESS, currentThickness);
    }

    public void SetOutLineObj(bool active)
    {
        isOutlineActive = active;
        ApplyMaterialProperties();
    }

    void OnDestroy()
    {
        if (instancedMaterial != null)
        {
            Destroy(instancedMaterial);
        }
    }
}