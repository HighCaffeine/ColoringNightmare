using System.Collections.Generic;
using UnityEngine;


public class DrawWeapon : GenericSingleton<DrawWeapon>
{
    [Header("Drawing")]
    [SerializeField] private Material lineMaterial;         // Sprites / Default 사용
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private float minPointDis = 0.02f;     // 포인트간 최소 거리
    [SerializeField] private Color lineColor = Color.white;

    [Header("이어 그리기 허용 범위")][SerializeField] private float lineJoinDistance = 0.5f;

    [Header("그릴 레이어")][SerializeField] private LayerMask targetLayer;

    [Header("스프라이트 데이터")]
    [SerializeField] private int pixelPerUnit = 100;
    [SerializeField] private int paddingPixels = 12;
    [SerializeField] private float drawPlaneZValue = 0.0f;  //2d라 기본 0.0f로 설정


    private Camera targetCamera;        //오브젝트 만들기 위해 가상 레이어 카메라 캐싱
    private Camera mainCamera;
    private LineRenderer line;

    private readonly List<Vector3> points = new List<Vector3>();

    private const int TextureMinSize = 2;
    private const int TextureMaxSize = 4096;

    private bool isCreated;
    private Color lastColor;
    private Vector3 lastPoint;


    protected override void Awake()
    {
        base.Awake();

        mainCamera = Camera.main;

        var cameraObj = new GameObject("TargetCamera");
        targetCamera = cameraObj.AddComponent<Camera>();
        targetCamera.orthographic = true;
        targetCamera.enabled = false;                                       //수동 렌더를 위해 false
        targetCamera.clearFlags = CameraClearFlags.SolidColor;              //단색 타입
        targetCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);   //배경 색 투명
        targetCamera.cullingMask = targetLayer;                //target Layer

        cameraObj.hideFlags = HideFlags.HideInHierarchy;

        isCreated = true;
    }

    void OnDestroy()
    {
        if (targetCamera)
        {
            Destroy(targetCamera.gameObject);
        }
    }

    private void Update()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        //input System Event
        // begin - begindraw
        // continue - continuedraw
        // end - endDraw (오브젝트화)

        if (Input.GetMouseButtonDown(0)) BeginDraw();
        if (Input.GetMouseButton(0)) ContinueDraw();
        if (Input.GetMouseButtonUp(0)) EndDraw();
    }

    //그리기 시작
    private void BeginDraw()
    {
        if (isCreated)
        {
            points.Clear();     //저장된 포인트 비움
            isCreated = false;
        }
        Vector3 mousePos = GetMousePos();

        if (!isCreated
            && Vector3.Distance(mousePos, lastPoint) <= lineJoinDistance)
        {
            if (line == null) CreateNewLine(mousePos);

            AppendPoint(mousePos);
        }
        else
        {
            Debug.Log("New Line");
            CreateNewLine(mousePos);
        }
    }

    //마우스 Pos 저장
    private void ContinueDraw()
    {
        if (line == null) return;

        Vector3 pos = GetMousePos();

        pos.z = drawPlaneZValue;

        AppendPoint(pos);
    }

    //그리기 끝
    private void EndDraw()
    {
        //라인이 없으면 스킵
        if (line == null) return;

        if (points.Count == 1)
        {
            lineRenderers.Remove(line.gameObject);
            Destroy(line.gameObject);   //풀링으로 추후 변경
            line = null;

            return;
        }

        lastColor = lineColor;
        lastPoint = points[^1];
        line = null;
    }

    private List<GameObject> lineRenderers = new List<GameObject>();

    private void CreateNewLine(Vector3 startPos)
    {
        //페인팅 될 오브젝트 생성
        var obj = new GameObject("Paint");
        obj.layer = Mathf.RoundToInt(Mathf.Log(targetLayer.value, 2));

        //마테리얼 컬러 변경 시 사용 (인스턴스화 필요)
        // var mat = lineMaterial != null ? new Material(lineMaterial) : new Material(Shader.Find("Sprites/Default"));
        // mat.color = lineColor;

        //오브젝트 세팅 (라인 렌더러)
        line = obj.AddComponent<LineRenderer>();
        line.positionCount = 0;
        line.material = lineMaterial ?? new Material(Shader.Find("Sprites/Default"));   //마테리얼 있을 경우 넣고 아니면 기본 
        line.widthMultiplier = lineWidth;
        line.useWorldSpace = true;
        line.numCapVertices = 8;        //끝부분 둥글게
        line.numCornerVertices = 4;     //코너 깔끔하게
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;   //쉐도우 캐스팅 끔
        line.receiveShadows = false;
        line.sortingOrder = 10000;   //가장 위에 나오게 최대값
        line.textureMode = LineTextureMode.Tile;

        line.startColor = lineColor;
        line.endColor = lineColor;

        lineRenderers.Add(obj);

        Debug.Log($"Start Draw Line : {startPos}");

        AppendPoint(startPos);
    }

    private Vector3 GetMousePos()
    {
        Vector3 screenPos = Input.mousePosition;
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x,
                                                                screenPos.y,
                                                                Mathf.Abs(mainCamera.transform.position.z - drawPlaneZValue)));

        return mousePos;
    }

    public void CreateDrawObj()
    {
        //라인 포인트 2개 이상일 경우 그리기
        SpawnDrawObj();
        InitLine();
    }

    private void InitLine()
    {
        foreach (var line in lineRenderers)
        {
            if (line) Destroy(line);
        }
        lineRenderers.Clear();
        line = null;
    }

    private void AppendPoint(Vector3 pos)
    {
        pos.z = drawPlaneZValue;

        if (points.Count == 0 || Vector3.Distance(pos, points[^1]) >= minPointDis)
        {
            points.Add(pos);
            line.positionCount = points.Count;
            line.SetPosition(points.Count - 1, pos);
        }
    }

    private void SpawnDrawObj()
    {
        //월드 경계값 계산 - 선 굵기, padding 값
        Bounds b = CalculateBound(points, lineWidth);
        float padUnits = paddingPixels / (float)pixelPerUnit;
        b.Expand(new Vector3(padUnits * 2.0f, padUnits * 2.0f, 0.0f));

        //텍스쳐 크기 계산
        int textureW = Mathf.Clamp(Mathf.CeilToInt(b.size.x * pixelPerUnit), TextureMinSize, TextureMaxSize);
        int textureH = Mathf.Clamp(Mathf.CeilToInt(b.size.y * pixelPerUnit), TextureMinSize, TextureMaxSize);

        //가상 카메라에 렌더링
        var render = new RenderTexture(textureW, textureH, 0, RenderTextureFormat.ARGB32);
        render.wrapMode = TextureWrapMode.Clamp;
        render.filterMode = FilterMode.Bilinear;

        targetCamera.targetTexture = render;
        targetCamera.orthographicSize = b.size.y * 0.5f;
        targetCamera.aspect = (float)textureW / textureH;
        targetCamera.transform.position = new Vector3(b.center.x, b.center.y, -10f);        //카메라쪽으로
        targetCamera.transform.rotation = Quaternion.identity;

        targetCamera.Render(); //렌더

        //Texture2D로 변경
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = render;

        var texture = new Texture2D(textureW, textureH, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0.0f, 0.0f, textureW, textureH), 0, 0, false);
        texture.Apply();

        RenderTexture.active = prev;
        targetCamera.targetTexture = null;      //초기화
        render.Release();
        Destroy(render);        //추후 풀링으로 변경

        //랜더에 남은 선 제거
        if (line != null) Destroy(line.gameObject); //풀링으로 변경

        //Sprite 생성
        Rect r = new Rect(0, 0, textureW, textureH);
        var s = Sprite.Create(texture, r, new Vector2(0.5f, 0.5f), pixelPerUnit, 0, SpriteMeshType.Tight);


        //오브젝트화
        var obj = new GameObject("DrawedObject");
        var spriteRender = obj.AddComponent<SpriteRenderer>();
        spriteRender.sprite = s;
        obj.transform.position = b.center;


        //Physics 적용
        var rigid = obj.AddComponent<Rigidbody2D>();
        rigid.gravityScale = 0.0f;

        AddPolygonCollider(obj, points, lineWidth);

        isCreated = true;
        InitLine();
    }

    private Bounds CalculateBound(List<Vector3> points, float width)
    {
        //포인트가 없으면 bounds 0으로
        if (points.Count == 0) return new Bounds(Vector3.zero, Vector3.zero);

        Bounds b = new Bounds(points[0], Vector3.zero);

        for (int i = 0; i < points.Count; i++)
        {
            b.Encapsulate(points[i]);
        }

        //선 굵기 처리
        b.Expand(new Vector3(width * 0.5f, width * 0.5f, 0f));

        return b;
    }

    private void AddPolygonCollider(GameObject obj, List<Vector3> points, float lineWidth)
    {
        // 1. 압축 Distance
        List<Vector2> simplified = new List<Vector2>();

        simplified.Add(points[0]);

        for (int i = 1; i < points.Count; i++)
        {
            if (Vector2.Distance(simplified[^1], (Vector2)points[i]) > lineJoinDistance)
            {
                simplified.Add(points[i]);
            }
        }

        Vector3 origin = obj.transform.position;

        // 2. 두께 반영: Offset Path 만들기
        List<Vector2> path = new List<Vector2>();
        float halfWidth = lineWidth * 0.5f;

        // 위쪽 라인 (바깥쪽)
        for (int i = 0; i < simplified.Count - 1; i++)
        {
            Vector2 dir = (simplified[i + 1] - simplified[i]).normalized;
            Vector2 normal = new Vector2(-dir.y, dir.x) * halfWidth;
            path.Add(simplified[i] + normal - (Vector2)origin);
        }

        // 아래쪽 라인 (안쪽)
        for (int i = simplified.Count - 1; i > 0; i--)
        {
            Vector2 dir = (simplified[i] - simplified[i - 1]).normalized;
            Vector2 normal = new Vector2(-dir.y, dir.x) * halfWidth;
            path.Add(simplified[i] - normal - (Vector2)origin);
        }

        // 3. 콜라이더 생성
        var poly = obj.AddComponent<PolygonCollider2D>();
        poly.pathCount = 1;
        poly.SetPath(0, path.ToArray());
    }
}
