using UnityEngine;
using Spine.Unity;


[RequireComponent(typeof(LineRenderer))]
public class DrawingMouseCursor : MonoBehaviour
{
    [Header("Spine")]
    [SerializeField] private SkeletonAnimation spineObject;

    [Space(5)]
    [Header("Brush Outline Settings")]
    [Tooltip("원의 정점 수")]
    [SerializeField] private int circleSegments = 32;
    [Tooltip("테두리 의 굵기 (브러쉬 크기 X)")]
    [SerializeField] private float outlineWidth = 0.05f;
    [Tooltip("테두리 선의 색상")]
    [SerializeField] private Color outlineColor = Color.black;

    private string sortingLayer = "Default";
    [SerializeField] private int sortingOrder = 10;

    private string currentAnimation;
    private float lastMouseX;
    private const float idleThreshold = 0.05f;
    private bool isActivateDrawMouse = false;

    private LineRenderer brushOutline;
    private float currentBrushRadius = -1f;
    private int lastSetSegments = -1;

    void Awake()
    {
        brushOutline = GetComponent<LineRenderer>();
        ConfigureLineRenderer();
    }

    void ConfigureLineRenderer()
    {
        brushOutline.useWorldSpace = false;

        brushOutline.loop = true;

        brushOutline.startWidth = outlineWidth;
        brushOutline.endWidth = outlineWidth;

        if (brushOutline.material == null)
        {
            // 머티리얼이 할당되지 않았으면,
            // 색상 변경이 가능한 기본 머티리얼을 새로 생성해서 할당합니다.
            var material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            brushOutline.material = material;
        }
        // 그리고 스크립트의 Outline Color 값으로 색을 덮어씁니다.
        brushOutline.material.color = outlineColor;

        brushOutline.sortingLayerName = sortingLayer;
        brushOutline.sortingOrder = sortingOrder;

        brushOutline.positionCount = circleSegments;

        brushOutline.enabled = false;
    }


    public void SetMouse()
    {
        spineObject.gameObject.SetActive(true);
        brushOutline.enabled = true;
        lastMouseX = Input.mousePosition.x;
        Cursor.visible = false;
        isActivateDrawMouse = true;

        SetAnimation("wolf_idle1");

        UpdateBrushOutlineFromWeapon();
    }

    public void Init()
    {
        spineObject.gameObject.SetActive(false);
        brushOutline.enabled = false;
        isActivateDrawMouse = false;
        Cursor.visible = true;
    }

    void Update()
    {
        if (!isActivateDrawMouse) return;

        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        worldMousePosition.z = spineObject.transform.position.z;
        transform.position = worldMousePosition;

        float mouseDeltaX = mousePosition.x - lastMouseX;
        if (Mathf.Abs(mouseDeltaX) > idleThreshold)
        {
            if (mouseDeltaX < 0) SetAnimation("wolf_Right");
            else SetAnimation("wolf_Left");
        }
        else
        {
            SetAnimation("wolf_idle1");
        }
        lastMouseX = mousePosition.x;

        UpdateBrushOutlineFromWeapon();
    }

    private void UpdateBrushOutlineFromWeapon()
    {
        if (DrawWeaponGPU.Instance == null) return;

        float targetDiameter = DrawWeaponGPU.Instance.LineWidth;
        float targetCenterRadius = (targetDiameter * 0.5f) - (outlineWidth * 0.5f);

        if (targetCenterRadius < 0.001f)
        {
            targetCenterRadius = 0.001f;
        }

        if (Mathf.Abs(targetCenterRadius - currentBrushRadius) > 0.001f || circleSegments != lastSetSegments)
        {
            DrawCircle(targetCenterRadius);
            currentBrushRadius = targetCenterRadius;
        }
    }

    private void DrawCircle(float radius)
    {
        if (radius <= 0)
        {
            brushOutline.enabled = false;
            return;
        }

        if (!brushOutline.enabled) brushOutline.enabled = true;

        if (brushOutline.positionCount != circleSegments)
        {
            brushOutline.positionCount = circleSegments;
        }
        if (brushOutline.startWidth != outlineWidth)
        {
            brushOutline.startWidth = outlineWidth;
            brushOutline.endWidth = outlineWidth;
        }
        brushOutline.material.color = outlineColor;
        brushOutline.sortingLayerName = sortingLayer;
        brushOutline.sortingOrder = sortingOrder;

        float angleStep = 360f / circleSegments;

        for (int i = 0; i < circleSegments; i++)
        {
            float currentAngle = Mathf.Deg2Rad * (i * angleStep);
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;

            brushOutline.SetPosition(i, new Vector3(x, y, 0));
        }

        lastSetSegments = circleSegments;
    }

    private void SetAnimation(string animationName, bool loop = true)
    {
        if (currentAnimation != animationName)
        {
            spineObject.AnimationState.SetAnimation(0, animationName, loop);
            currentAnimation = animationName;
        }
    }
}