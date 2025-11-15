using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class OutlineController : MonoBehaviour
{
    private SpriteRenderer mainRenderer;
    private Material instancedMaterial;
    private bool isOutlineActive;

    [Header("아웃라인 머티리얼")]
    [SerializeField] private Material outlineMaterialBase;

    [Header("아웃라인 설정")]
    //public Color outlineColor = Color.black;

    [Range(0f, 3f)]
    //public float outlineThickness = 0.2f;

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
        mainRenderer.material = instancedMaterial;

        SetOutLineObj(false);
    }

    void Update()
    {
        if (instancedMaterial != null)
        {
            ApplyMaterialProperties();
        }
    }

    private void ApplyMaterialProperties()
    {
        if (instancedMaterial == null) return;

        //instancedMaterial.SetColor(PROP_COLOR, outlineColor);
        //float currentThickness = isOutlineActive ? outlineThickness : 0.0f;
        //instancedMaterial.SetFloat(PROP_THICKNESS, currentThickness);
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