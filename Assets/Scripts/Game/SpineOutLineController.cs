using UnityEngine;
using Spine.Unity;

[RequireComponent(typeof(SkeletonAnimation))]
public class SpineOutlineController : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] private Color outlineColor = Color.yellow;
    [Range(0f, 0.1f)] // 스파인 아웃라인은 값이 작아도 크게 보입니다.
    [SerializeField] private float outlineWidth = 0.005f;
    [Range(0f, 1f)]
    [SerializeField] private float threshold = 0.5f;

    [Header("State")]
    [SerializeField] private bool isOutlineActive = false;

    private SkeletonAnimation skeletonAnimation;
    private MeshRenderer meshRenderer;
    private Material originalMaterial;
    private Material outlineMaterial;

    private void Awake()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        meshRenderer = GetComponent<MeshRenderer>();

        // 1. 원본 마테리얼 저장
        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
            PrepareOutlineMaterial();

            SetOutline(true);
        }
    }

    private void PrepareOutlineMaterial()
    {
        // 2. Spine 아웃라인 셰이더 찾기
        Shader outlineShader = Shader.Find("Spine/Outline/Skeleton");
        if (outlineShader == null)
        {
            Debug.LogError("Spine/Outline/Skeleton 셰이더를 찾을 수 없습니다. Spine 패키지가 설치되어 있는지 확인하세요.");
            return;
        }

        // 3. 아웃라인용 마테리얼 생성 (원본 텍스처 복사)
        outlineMaterial = new Material(outlineShader);
        outlineMaterial.mainTexture = originalMaterial.mainTexture;

        // 4. 초기 설정 적용
        UpdateOutlineProperties();
    }

    private void UpdateOutlineProperties()
    {
        if (outlineMaterial != null)
        {
            outlineMaterial.SetColor("_OutlineColor", outlineColor);
            outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
            outlineMaterial.SetFloat("_Threshold", threshold);

            // 스파인 설정에 따라 필요시 추가 (Premultiply Alpha 등)
            if (originalMaterial.HasProperty("_StraightAlphaInput"))
            {
                float straightAlpha = originalMaterial.GetFloat("_StraightAlphaInput");
                outlineMaterial.SetFloat("_StraightAlphaInput", straightAlpha);
            }
        }
    }

    public void SetOutline(bool active)
    {
        isOutlineActive = active;

        if (meshRenderer == null || outlineMaterial == null) return;

        if (active)
        {
            // 아웃라인 설정 갱신 후 적용
            UpdateOutlineProperties();
            meshRenderer.material = outlineMaterial;
        }
        else
        {
            // 원본으로 복구
            meshRenderer.material = originalMaterial;
        }
    }

    public void SetColor(Color color)
    {
        outlineColor = color;
        if (isOutlineActive) UpdateOutlineProperties();
    }

    // 에디터에서 값 조절 시 실시간 반영용
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying && isOutlineActive)
        {
            UpdateOutlineProperties();
        }
    }
#endif

    private void OnDisable()
    {
        // 비활성화 시 원본으로 복구 (잔여 방지)
        if (meshRenderer != null && originalMaterial != null)
        {
            meshRenderer.material = originalMaterial;
        }
    }
}