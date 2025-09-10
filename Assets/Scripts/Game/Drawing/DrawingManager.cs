using System.Collections.Generic;
using UnityEngine;

public class DrawingManager : GenericSingleton<DrawingManager>
{
    [Header("Drawing")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private float minPointDis = 0.02f;
    [SerializeField] private Color lineColor = Color.white;
    [Range(0.0f, 1.0f)][SerializeField] private float saturation = 1.0f;

    [Header("이어 그리기 허용 범위")][SerializeField] private float lineJoinDistance = 0.5f;
    [Header("그릴 레이어")][SerializeField] private LayerMask targetLayer;
    [Header("스프라이트 데이터")]
    [SerializeField] private int pixelPerUnit = 100;
    [SerializeField] private int paddingPixels = 12;
    [SerializeField] private float drawPlaneZValue = 0.0f;

    private Camera targetCamera;
    private Camera mainCamera;
    private LineRenderer line;

    private readonly List<Vector3> points = new List<Vector3>();
    private List<GameObject> lineRenderers = new List<GameObject>();
    private int beforeLayer = 1000;

    private bool isCreated;
    private Color lastColor;
    private int lastPointCount = 0;

    [SerializeField] private LineDrawArea drawArea;

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;

        var cameraObj = new GameObject("TargetCamera");
        targetCamera = cameraObj.AddComponent<Camera>();
        targetCamera.orthographic = true;
        targetCamera.enabled = false;
        targetCamera.clearFlags = CameraClearFlags.SolidColor;
        targetCamera.backgroundColor = new Color(0, 0, 0, 0);
        targetCamera.cullingMask = targetLayer;
        cameraObj.hideFlags = HideFlags.HideInHierarchy;

        isCreated = true;
    }

    private void OnDestroy()
    {
        if (targetCamera) Destroy(targetCamera.gameObject);
    }

    private void Update()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0)) BeginDraw();
        if (Input.GetMouseButton(0)) ContinueDraw();
        if (Input.GetMouseButtonUp(0)) EndDraw();
    }

    private void BeginDraw()
    {
        if (isCreated)
        {
            points.Clear();
            isCreated = false;
        }
        Vector3 mousePos = GetMousePos();
        CreateNewLine(mousePos);
    }

    private void ContinueDraw()
    {
        if (line == null) return;
        Vector3 pos = GetMousePos();
        pos.z = drawPlaneZValue;
        AppendPoint(pos);
    }

    private void EndDraw()
    {
        if (line == null) return;

        if (points.Count == 1)
        {
            lineRenderers.Remove(line.gameObject);
            Destroy(line.gameObject);
            line = null;
            return;
        }

        lastColor = lineColor;
        lastPointCount = 0;
        line = null;
    }

    public void SetLineColor(Color c) => lineColor = c;

    private void CreateNewLine(Vector3 startPos)
    {
        lastPointCount = points.Count;
        var obj = new GameObject("Paint");
        obj.layer = Mathf.RoundToInt(Mathf.Log(targetLayer.value, 2));

        line = obj.AddComponent<LineRenderer>();
        line.positionCount = 0;
        line.widthMultiplier = lineWidth;
        line.useWorldSpace = true;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.sortingOrder = ++beforeLayer;
        beforeLayer = line.sortingOrder;
        line.textureMode = LineTextureMode.Tile;
        line.numCornerVertices = 0;
        line.numCapVertices = 4;

        var mat = lineMaterial != null ? new Material(lineMaterial) : new Material(Shader.Find("Sprites/Default"));
        mat.SetColor("_Color", AdjustSaturationHSV(lineColor, saturation));
        line.material = mat;

        lineRenderers.Add(obj);
        AppendPoint(startPos);
    }

    private void AppendPoint(Vector3 pos)
    {
        pos.z = drawPlaneZValue;

        if (drawArea != null && !drawArea.GetBounds().Contains(pos))
        {
            EndDraw();
            return;
        }

        if (points.Count == 0 || Vector3.Distance(pos, points[^1]) >= minPointDis)
        {
            points.Add(pos);
            line.positionCount = points.Count - lastPointCount;
            line.SetPosition(points.Count - 1 - lastPointCount, pos);
        }
    }

    private Vector3 GetMousePos()
    {
        Vector3 screenPos = Input.mousePosition;
        return mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y,
            Mathf.Abs(mainCamera.transform.position.z - drawPlaneZValue)));
    }

    Color AdjustSaturationHSV(Color color, float saturation)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        s = Mathf.Clamp01(s * saturation);
        return Color.HSVToRGB(h, s, v);
    }

    public List<Vector3> GetPoints() => points;
    public List<GameObject> GetLineRenderers() => lineRenderers;
    public Camera GetTargetCamera() => targetCamera;
    public float GetLineWidth() => lineWidth;
    public int GetPixelPerUnit() => pixelPerUnit;
    public int GetPaddingPixels() => paddingPixels;
    public float GetDrawPlaneZValue() => drawPlaneZValue;

    public void ClearLines()
    {
        foreach (var l in lineRenderers)
            if (l) Destroy(l);
        lineRenderers.Clear();
        line = null;
    }
}
