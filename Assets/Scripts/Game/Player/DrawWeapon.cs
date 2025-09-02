using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class DrawWeapon : GenericSingleton<DrawWeapon>
{
    [Header("Drawing")]
    [SerializeField] private Material lineMaterial;         // Sprites / Default 사용
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private float minPointDis = 0.02f;     // 포인트간 최소 거리
    [SerializeField] private Color lineColor = Color.white;
    [Range(0.0f, 1.0f)][SerializeField] private float saturation = 1.0f;              //채도

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
    private int lastPointCount = 0;


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
        CreateNewLine(mousePos);
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
        lastPointCount = 0;
        line = null;
    }

    private List<GameObject> lineRenderers = new List<GameObject>();
    private void CreateNewLine(Vector3 startPos)
    {
        lastPointCount = points.Count;
        //페인팅 될 오브젝트 생성
        var obj = new GameObject("Paint");
        obj.layer = Mathf.RoundToInt(Mathf.Log(targetLayer.value, 2));


        //오브젝트 세팅 (라인 렌더러)
        line = obj.AddComponent<LineRenderer>();
        line.positionCount = 0;
        line.widthMultiplier = lineWidth;
        line.useWorldSpace = true;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;   //쉐도우 캐스팅 끔
        line.receiveShadows = false;
        line.sortingOrder = 10000;   //가장 위에 나오게 최대값

        line.textureMode = LineTextureMode.Tile;
        line.numCornerVertices = 0;     //코너 깔끔하게
        line.numCapVertices = 4;

        //마테리얼 컬러 변경 시 사용(인스턴스화 필요)
        var mat = lineMaterial != null ? new Material(lineMaterial) : new Material(Shader.Find("Sprites/Default"));
        //mat.SetColor("_Color", AdjustSaturation(lineColor, saturation));
        mat.SetColor("_Color", AdjustSaturationHSV(lineColor, saturation));
        line.material = mat;

        lineRenderers.Add(obj);

        Debug.Log($"Start Draw Line : {startPos}");

        AppendPoint(startPos);
    }

    Color AdjustSaturation(Color color, float saturation)
    {
        float gray = color.r * 0.3f + color.g * 0.59f + color.b * 0.11f;
        return Color.Lerp(new Color(gray, gray, gray, color.a), color, saturation);
    }
    Color AdjustSaturationHSV(Color color, float saturation)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        s = Mathf.Clamp01(s * saturation);
        return Color.HSVToRGB(h, s, v);
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
            line.positionCount = points.Count - lastPointCount;
            line.SetPosition(points.Count - 1 - lastPointCount, pos);
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


        AddEdgeCollider(obj, points);



        //AddPolygonCollider(obj, points, lineWidth);

        isCreated = true;
        InitLine();
    }

    private Bounds CalculateBound(List<Vector3> points, float width)
    {
        if (points.Count == 0) return new Bounds(Vector3.zero, Vector3.zero);

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (var point in points)
        {
            minX = Mathf.Min(minX, point.x);
            minY = Mathf.Min(minY, point.y);
            maxX = Mathf.Max(maxX, point.x);
            maxY = Mathf.Max(maxY, point.y);
        }

        float expansion = width * 0.5f;

        Bounds b = new Bounds();
        b.min = new Vector3(minX - expansion, minY - expansion, 0);
        b.max = new Vector3(maxX + expansion, maxY + expansion, 0);

        return b;
    }

    //안쓸듯 라인 굵기가 바뀐다면 사용
    // private void AddPolygonCollider(GameObject obj, List<Vector3> points, float lineWidth)
    // {
    //     // 1. 압축 Distance
    //     List<Vector2> simplified = new List<Vector2>();

    //     simplified.Add(points[0]);

    //     for (int i = 1; i < points.Count; i++)
    //     {
    //         if (Vector2.Distance(simplified[^1], (Vector2)points[i]) > lineJoinDistance)
    //         {
    //             simplified.Add(points[i]);
    //         }
    //     }

    //     Vector3 origin = obj.transform.position;

    //     // 2. 두께 반영: Offset Path 만들기
    //     List<Vector2> path = new List<Vector2>();
    //     float halfWidth = lineWidth * 0.5f;

    //     // 위쪽 라인 (바깥쪽)
    //     for (int i = 0; i < simplified.Count - 1; i++)
    //     {
    //         Vector2 dir = (simplified[i + 1] - simplified[i]).normalized;
    //         Vector2 normal = new Vector2(-dir.y, dir.x) * halfWidth;
    //         path.Add(simplified[i] + normal - (Vector2)origin);
    //     }

    //     // 아래쪽 라인 (안쪽)
    //     for (int i = simplified.Count - 1; i > 0; i--)
    //     {
    //         Vector2 dir = (simplified[i] - simplified[i - 1]).normalized;
    //         Vector2 normal = new Vector2(-dir.y, dir.x) * halfWidth;
    //         path.Add(simplified[i] - normal - (Vector2)origin);
    //     }

    //     // 3. 콜라이더 생성
    //     var poly = obj.AddComponent<PolygonCollider2D>();

    //     poly.pathCount = 1;
    //     poly.SetPath(0, path.ToArray());
    // }

    private void AddEdgeCollider(GameObject obj, List<Vector3> points)
    {
        // Douglas Peucker 다각형 근사화 알고리즘.
        List<Vector2> simplified = SimplifyPath(points, 0.1f);

        Vector3 origin = obj.transform.position;
        List<Vector2> localPoints = new List<Vector2>();

        foreach (Vector2 p in simplified)
        {
            localPoints.Add(p - (Vector2)origin);
        }

        var edge = obj.AddComponent<EdgeCollider2D>();
        edge.points = localPoints.ToArray();

        edge.edgeRadius = lineWidth * 0.5f;
    }

    // Douglas Peucker 다각형 근사화 알고리즘.
    // 시작 점과 끝점부터 안쪽 점까지 거리를 계산 tolerance보다 크면 가능한 지점
    private List<Vector2> SimplifyPath(List<Vector3> points, float tolerance)
    {
        if (points.Count <= 2) return points.ConvertAll(p => (Vector2)p);

        int d = 0;
        float maxDistance = 0f;
        Vector2 start = points[0];
        Vector2 end = points[^1];

        for (int i = 1; i < points.Count - 1; i++)
        {
            float distance = PerpendicularDistance(points[i], start, end);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                d = i;
            }
        }

        if (maxDistance > tolerance)
        {
            List<Vector2> results1 = SimplifyPath(points.GetRange(0, d + 1), tolerance);
            List<Vector2> results2 = SimplifyPath(points.GetRange(d, points.Count - d), tolerance);

            List<Vector2> combinedResults = new List<Vector2>();
            combinedResults.AddRange(results1);
            combinedResults.AddRange(results2);

            return combinedResults;
        }
        else
        {
            List<Vector2> simplified = new List<Vector2>();
            simplified.Add(start);
            simplified.Add(end);
            return simplified;
        }
    }

    // 점과 선분 사이의 수직 거리 계산
    private float PerpendicularDistance(Vector3 point, Vector2 lineStart, Vector2 lineEnd)
    {
        //방향 벡터
        float dx = lineEnd.x - lineStart.x;
        float dy = lineEnd.y - lineStart.y;
        float len = Mathf.Sqrt(dx * dx + dy * dy); //피타고라스 정리

        //예외처리
        if (len == 0) return Vector2.Distance(point, lineStart);

        //방향 벡터랑 point에서 linestart로 향하는 벡터를 내적, 길이 제곱으로 정규화
        float t = ((point.x - lineStart.x) * dx + (point.y - lineStart.y) * dy) / (len * len);
        t = Mathf.Clamp(t, 0, 1);   //넘어 가는 값이면 선분에 위치하지 않은 놈임 임의로 위치시킴
        Vector2 projection = new Vector2(lineStart.x + t * dx, lineStart.y + t * dy);   //실제 좌표 구함

        return Vector2.Distance(point, projection); //거리 도출
    }

    int CountPixels(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        Color32[] pixels = tex.GetPixels32();
        int total = 0;
        Dictionary<Color, int> colorCount = new();

        foreach (var p in pixels)
        {
            if (p.a == 0) continue; // 투명 제외

            // 단색만 들어있으므로 R/G/B 직접 체크
            if (p.r > 200 && p.g < 50 && p.b < 50)
            {
                AddColor(colorCount, Color.red, ref total);
            }
            else if (p.g > 200 && p.r < 50 && p.b < 50)
            {
                AddColor(colorCount, Color.green, ref total);
            }
            else if (p.b > 200 && p.r < 50 && p.g < 50)
            {
                AddColor(colorCount, Color.blue, ref total);
            }
        }

        // 퍼센트 출력
        foreach (var kv in colorCount)
        {
            float percent = (kv.Value / (float)total) * 100f;
            Debug.Log($"{kv.Key} : {percent:F2}%");
        }

        return total;
    }

    void AddColor(Dictionary<Color, int> dict, Color c, ref int total)
    {
        if (!dict.ContainsKey(c)) dict[c] = 0;
        dict[c]++;
        total++;
    }
}
