using System.Collections.Generic;
using UnityEngine;

public class DrawingManager : GenericSingleton<DrawingManager>
{
    [Header("Drawing")]
    [SerializeField] private Material lineMaterial;             //마테리얼
    [SerializeField] private float lineWidth = 0.1f;            //라인굵기
    [SerializeField] private float minPointDis = 0.02f;         //라인 포인트 최소거리
    [SerializeField] private Color lineColor = Color.white;     //라인색
    [Range(0.0f, 1.0f)]
    [SerializeField] private float saturation = 1.0f;           //채도
    [SerializeField] private float drawPlaneZValue = 0.0f;


    private readonly List<Vector3> points = new List<Vector3>();                //포인트 리스트
    private readonly List<GameObject> lineRenderers = new List<GameObject>();   //라인 리스트
    private LineRenderer currentLine;   //현재라인
    private Camera mainCamera;  //미리 카메라 캐싱
    private int currentSortingOrder = 1000;

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        //UI 이벤트 시스템을 누를 경우에는 그리지 않도록 설정
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0)) BeginDraw();
        if (Input.GetMouseButton(0)) ContinueDraw();
        if (Input.GetMouseButtonUp(0)) EndDraw();
    }

    //mouse down
    private void BeginDraw()
    {
        //초기화
        points.Clear();
        InitDrawing();

        //라인 시작점 세팅 및 생성
        Vector3 mousePos = GetMousePos();
        CreateNewLine(mousePos);
    }

    //mouse continue
    private void ContinueDraw()
    {
        if (currentLine == null) return;

        Vector3 pos = GetMousePos();
        AppendPoint(pos);
    }

    //mouse up
    private void EndDraw()
    {
        if (currentLine == null) return;

        //포인트 1개만 있을 경우 선이 아니기 때문에 예외처리
        if (points.Count < 2)
        {
            lineRenderers.Remove(currentLine.gameObject);
            Destroy(currentLine.gameObject);
            return;
        }

        //이외에는 무기 생성
        WeaponFactory.Instance.CreateWeapon();
        InitDrawing();
    }

    //데이터 초기화
    public void InitDrawing()
    {
        foreach (var line in lineRenderers)
        {
            if (line) Destroy(line);
        }

        lineRenderers.Clear();
        currentLine = null;
        points.Clear();
    }

    //새로운 라인을 생성
    private void CreateNewLine(Vector3 startPos)
    {
        //오브젝트 생성
        var obj = new GameObject("Paint");
        obj.layer = LayerMask.NameToLayer("Draw");

        //라인렌더러 속성 설정
        currentLine = obj.AddComponent<LineRenderer>();
        currentLine.positionCount = 0;
        currentLine.widthMultiplier = lineWidth;
        currentLine.useWorldSpace = true;
        currentLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        currentLine.receiveShadows = false;
        currentLine.sortingOrder = ++currentSortingOrder; // 위에 덧그리기위해 sortingorder 설정

        //마테리얼을 지정하지 않을경우 기본 sprite 마테리얼
        var mat = lineMaterial != null ? new Material(lineMaterial) : new Material(Shader.Find("Sprites/Default"));
        mat.SetColor("_Color", AdjustSaturationHSV(lineColor, saturation)); //명도 적용된 색상
        currentLine.material = mat;

        lineRenderers.Add(obj);
        AppendPoint(startPos);
    }

    //min PointDis거리보다 큰 거리일 경우 points에 추가 및 line지정
    private void AppendPoint(Vector3 pos)
    {
        if (points.Count == 0 || Vector3.Distance(pos, points[^1]) >= minPointDis)
        {
            points.Add(pos);
            currentLine.positionCount = points.Count;
            currentLine.SetPosition(points.Count - 1, pos);
        }
    }

    //그리기 위한 마우스 위치를 반환
    private Vector3 GetMousePos()
    {
        Vector3 screenPos = Input.mousePosition;
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCamera.transform.position.z - drawPlaneZValue)));
        return mousePos;
    }

    //hsv로 변환해서 색 채도 명도 설정
    private Color AdjustSaturationHSV(Color color, float saturation)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        s = Mathf.Clamp01(s * saturation);
        return Color.HSVToRGB(h, s, v);
    }

    // 유사도 평가 및 무기 생성을 위해 필요한 데이터 접근 함수
    public List<Vector3> GetPoints() => points;
    public List<GameObject> GetLineRenderers() => lineRenderers;
    public float GetLineWidth() => lineWidth;
}