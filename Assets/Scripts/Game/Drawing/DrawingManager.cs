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

    [Header("Sprite Data")]
    [SerializeField] private int pixelPerUnit = 100;
    [SerializeField] private int paddingPixels = 12;
    [SerializeField] private float drawPlaneZValue = 0.0f;

    [Header("Weapon Size")]
    [Range(0.0f, 1.0f)][SerializeField] private float ratioFromSketchBook;

    [Header("그릴 레이어")]
    [SerializeField] private LayerMask targetLayer;
    [Header("그리기 영역")]
    [SerializeField] private LineDrawArea drawArea;

    private Camera targetCamera;
    private Camera mainCamera;
    private LineRenderer line;

    private readonly List<Vector3> points = new List<Vector3>();
    private List<GameObject> activeLines = new List<GameObject>();
    private bool isAllowDrawing = false;
    private Color lastColor;
    private ColorMixer.ColorType colorType;
    private int lastPointCount = 0;


    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;

        var cameraObj = new GameObject("TargetCamera");
        targetCamera = cameraObj.AddComponent<Camera>();
        targetCamera.orthographic = true;
        targetCamera.enabled = false;
        targetCamera.clearFlags = CameraClearFlags.SolidColor;
        targetCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        targetCamera.cullingMask = targetLayer;
        cameraObj.hideFlags = HideFlags.HideInHierarchy;
    }

    void OnDestroy()
    {
        if (targetCamera)
        {
            Destroy(targetCamera.gameObject);
        }
    }

    public void SetAllowDrawingState(bool value)
    {
        isAllowDrawing = value;
    }
    private bool IsAllowEvents()
    {
        if (!isAllowDrawing) return false;

        if (drawArea != null && !drawArea.GetBounds().Contains(GetMousePos()))
        {
            return false;
        }

        if (UnityEngine.EventSystems.EventSystem.current != null
            && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }

        return true;
    }

    public void BeginDraw()
    {
        if (!IsAllowEvents()) return;

        points.Clear();
        Vector3 mousePos = GetMousePos();
        CreateNewLine(mousePos);
    }

    public void ContinueDraw()
    {
        if (!IsAllowEvents()) return;
        if (line == null) return;

        Vector3 pos = GetMousePos();
        pos.z = drawPlaneZValue;
        AppendPoint(pos);
    }

    public void EndDraw()
    {
        if (!IsAllowEvents()) return;
        if (line == null) return;

        if (points.Count == 1)
        {
            activeLines.Remove(line.gameObject);
            Destroy(line.gameObject);
            line = null;
            return;
        }

        lastColor = lineColor;
        lastPointCount = 0;
        line = null;
    }

    public void SetLineColor(Color c, ColorMixer.ColorType colorType)
    {
        lineColor = c;
        this.colorType = colorType;
    }

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
        line.sortingOrder = 1000;
        line.textureMode = LineTextureMode.Tile;
        line.numCornerVertices = 0;
        line.numCapVertices = 4;

        var mat = lineMaterial != null ? new Material(lineMaterial) : new Material(Shader.Find("Sprites/Default"));
        Color.RGBToHSV(lineColor, out float h, out float s, out float v);
        s = Mathf.Clamp01(s * saturation);
        mat.SetColor("_Color", Color.HSVToRGB(h, s, v));
        line.material = mat;

        activeLines.Add(obj);
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

    public Vector3 GetMousePos()
    {
        Vector3 screenPos = Input.mousePosition;
        return mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCamera.transform.position.z - drawPlaneZValue)));
    }
    public void ClearLines()
    {
        foreach (var l in activeLines)
        {
            if (l) Destroy(l);
        }

        activeLines.Clear();
        points.Clear();
        line = null;
    }

    public List<Vector3> GetPoints() => points;
    public List<GameObject> GetLineRenderers() => activeLines;
    public Camera GetTargetCamera() => targetCamera;
    public float GetLineWidth() => lineWidth;
    public int GetPixelPerUnit() => pixelPerUnit;
    public int GetPaddingPixels() => paddingPixels;
    public float GetRatioFromSketchBook() => ratioFromSketchBook;
    public Color GetLastColor() => lastColor;
    public ColorMixer.ColorType GetColorType() => colorType;
}