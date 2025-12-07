using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

public class DrawWeaponGPU : GenericSingleton<DrawWeaponGPU>
{
    [Header("Drawing")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.1f;

    [SerializeField] private float minPointDis = 0.02f; // 포인트간 최소 거리
    [SerializeField] private Color lineColor = Color.white;
    [Range(0.0f, 1.0f)][SerializeField] private float saturation = 1.0f; // 채도
    [Range(40.0f, 80.0f)][SerializeField] private float similarityLimit = 60.0f;

    [Space(1f)]
    [Header("Drawing Layer")]
    [SerializeField] private LayerMask targetLayer;

    [Space(1f)]
    [Header("Sprite Data")]
    [SerializeField] private int pixelPerUnit = 100;
    [SerializeField] private int paddingPixels = 12;
    [SerializeField] private float drawPlaneZValue = 0.0f; // 2d라 기본 0.0f로 설정

    [Space(1f)]
    [Header("Weapon Size")]
    [Range(0.0f, 1.0f)][SerializeField] private float ratioFromSketchBook;

    [Space(1f)]
    [Header("Weapon Data")]
    [Tooltip("목록에 없는 색상으로 제작 시 사용할 기본 무기 데이터")]
    [SerializeField] private WeaponInkData defaultWeaponData;
    [SerializeField] private LineDrawArea drawArea;
    [SerializeField] private SpriteRenderer refSprite;

    [Space(1f)]
    [Header("유사도")]
    [SerializeField] private TMPro.TextMeshProUGUI similarityText;
    [SerializeField] private DrawSimilarityEffect similarityEffect;
    [SerializeField] private VisualSimilarity visualSimilarity;

    [SerializeField] private List<WeaponInkData> weaponInkDataList;
    public float LineWidth { get { return lineWidth; } }

    #region Rendering Cemra Variable
    private Camera targetCamera; // 오브젝트 만들기 위해 가상 레이어 카메라 캐싱
    private Camera mainCamera;
    private LineRenderer line;

    private readonly List<Vector3> points = new List<Vector3>();
    private List<GameObject> lineRenderers = new List<GameObject>();
    private int beforeLayer = 1000;

    private const int TextureMinSize = 2;
    private const int TextureMaxSize = 4096;

    private bool isCreated;
    private ColorMixer.ColorType colorType;
    private WeaponManager.WeaponType weaponType;
    private int lastPointCount = 0;
    private bool isAllowDrawing = false;
    #endregion

    #region GPU Render Burst Variable
    private NativeArray<Color32> refPixelsNative; // 도안 데이터 캐싱
    private bool isRefCached = false;
    private bool isCalculating = false; // 중복 계산 방지
    private float checkTimer = 0f;
    private float checkInterval = 0.2f; // 0.2초마다 검사
    private float currentDiceScore = 0f; // 현재 계산된 점수 저장

    [BurstCompile]
    struct SimilarityJob : IJob
    {
        [ReadOnly] public NativeArray<Color32> drawPixels;
        [ReadOnly] public NativeArray<Color32> refPixels;
        public NativeArray<float> results;

        public void Execute()
        {
            float userC = 0;
            float refC = 0;
            float interC = 0;
            float unionC = 0;
            float dotSum = 0;

            for (int i = 0; i < drawPixels.Length; i++)
            {
                float u = drawPixels[i].a > 25 ? 1f : 0f; // Threshold 10%
                float r = refPixels[i].a > 25 ? 1f : 0f;

                if (u > 0.5f)
                    userC++;
                if (r > 0.5f)
                    refC++;
                if (u > 0.5f && r > 0.5f)
                    interC++;
                if (u > 0.5f || r > 0.5f)
                    unionC++;
                dotSum += u * r;
            }

            results[0] = userC;
            results[1] = refC;
            results[2] = interC;
            results[3] = unionC;
            results[4] = dotSum;
        }
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;

        var cameraObj = new GameObject("TargetCamera");
        targetCamera = cameraObj.AddComponent<Camera>();
        targetCamera.orthographic = true;
        targetCamera.enabled = false;                                     // 수동 렌더를 위해 false
        targetCamera.clearFlags = CameraClearFlags.SolidColor;            // 단색 타입
        targetCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f); // 배경 색 투명
        targetCamera.cullingMask = targetLayer;                           // target Layer
        cameraObj.hideFlags = HideFlags.HideInHierarchy;

        isCreated = true;
    }

    void Start()
    {
        CacheReferenceSprite(); //도안 캐싱
    }

    void OnDestroy()
    {
        if (targetCamera)
        {
            Destroy(targetCamera.gameObject);
        }
        if (isRefCached && refPixelsNative.IsCreated) refPixelsNative.Dispose();
    }

    //유사도 체크 Update 추가
    void Update()
    {
        //그리는 중에만 체크
        if (line != null && isAllowDrawing)
        {
            checkTimer += Time.deltaTime;
            if (checkTimer >= checkInterval && !isCalculating)
            {
                checkTimer = 0f;
                RequestRealtimeSimilarity();
            }
        }
    }

    public void CacheReferenceSprite()
    {
        if (refSprite == null || refSprite.sprite == null) return;

        Texture2D tex = refSprite.sprite.texture;

        // 텍스처가 Read/Write Enabled가 아니면 읽을 수 없으므로 처리
        if (!tex.isReadable)
        {
            return;
        }

        if (isRefCached && refPixelsNative.IsCreated) refPixelsNative.Dispose();

        // 도안 데이터를 NativeArray로 캐싱
        var pixels = tex.GetPixels32();

        refPixelsNative = new NativeArray<Color32>(pixels, Allocator.Persistent);
        isRefCached = true;
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
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }
        if (colorType == ColorMixer.ColorType.None || colorType == ColorMixer.ColorType.Count)
        {
            return false;
        }
        return true;
    }

    // 그리기 시작
    public void BeginDraw()
    {
        if (!IsAllowEvents()) return;
        if (isCreated)
        {
            points.Clear(); // 저장된 포인트 비움
            isCreated = false;
        }
        Vector3 mousePos = GetMousePos();
        CreateNewLine(mousePos);
    }

    // 마우스 Pos 저장
    public void ContinueDraw()
    {
        if (!IsAllowEvents() || line == null) return;
        Vector3 pos = GetMousePos();
        pos.z = drawPlaneZValue;
        AppendPoint(pos);
    }

    // 그리기 끝
    public void EndDraw()
    {
        if (!IsAllowEvents() || line == null) return;
        if (points.Count == 1)
        {
            lineRenderers.Remove(line.gameObject);
            Destroy(line.gameObject); // 풀링으로 추후 변경
            line = null;
            return;
        }

        lastPointCount = 0;
        line = null;

        //그리기 종료 후 마지막 체크
        RequestRealtimeSimilarity();
    }

    public void SetLineColor(Color c, ColorMixer.ColorType colorType)
    {
        lineColor = c;
        this.colorType = colorType;
    }

    //임시 외부 호출용 함수
    public void TEST_UpdateLineColor()
    {
        lineColor = ColorMixer.Instance.GetColor(colorType);
    }

    private void CreateNewLine(Vector3 startPos)
    {
        lastPointCount = points.Count;
        // 페인팅 될 오브젝트 생성
        var obj = new GameObject("Paint");
        obj.layer = Mathf.RoundToInt(Mathf.Log(targetLayer.value, 2));

        // 오브젝트 세팅 (라인 렌더러)
        line = obj.AddComponent<LineRenderer>();
        line.positionCount = 0;
        line.widthMultiplier = lineWidth;
        line.useWorldSpace = true;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // 쉐도우 캐스팅 끔
        line.receiveShadows = false;
        line.sortingOrder = ++beforeLayer; // 가장 위에 나오게 최대값
        beforeLayer = line.sortingOrder;
        line.textureMode = LineTextureMode.Tile;
        line.numCornerVertices = 4; // 코너 깔끔하게
        line.numCapVertices = 4;

        // 마테리얼 컬러 변경 시 사용(인스턴스화 필요)
        var mat = lineMaterial != null ? new Material(lineMaterial) : new Material(Shader.Find("Sprites/Default"));
        Color.RGBToHSV(lineColor, out float h, out float s, out float v);
        s = Mathf.Clamp01(s * saturation);
        mat.SetColor("_Color", Color.HSVToRGB(h, s, v));
        line.material = mat;

        lineRenderers.Add(obj);
        AppendPoint(startPos);
    }

    private void AppendPoint(Vector3 pos)
    {
        pos.z = drawPlaneZValue;
        if (points.Count == 0 || Vector3.Distance(pos, points[points.Count - 1]) >= minPointDis)
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

    // 비동기 유사도 체크 함수
    private void RequestRealtimeSimilarity()
    {
        if (!isRefCached || refSprite == null)
        {
            return;
        }

        isCalculating = true;

        int w = refSprite.sprite.texture.width;
        int h = refSprite.sprite.texture.height;
        Bounds b = refSprite.bounds;

        RenderTexture render = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);

        targetCamera.targetTexture = render;
        targetCamera.orthographicSize = b.size.y * 0.5f;
        targetCamera.aspect = (float)w / h;
        targetCamera.transform.position = new Vector3(b.center.x, b.center.y, -10f);
        targetCamera.Render();

        AsyncGPUReadback.Request(render, 0, (request) => OnCompareComplete(request, w, h));

        targetCamera.targetTexture = null;
        RenderTexture.ReleaseTemporary(render);
    }

    //콜백용 함수
    private void OnCompareComplete(AsyncGPUReadbackRequest request, int w, int h)
    {
        if (request.hasError)
        {
            isCalculating = false;
            return;
        }

        if (!refPixelsNative.IsCreated)
        {
            isCalculating = false;
            return;
        }

        NativeArray<Color32> drawPixels = request.GetData<Color32>();

        if (drawPixels.Length != refPixelsNative.Length)
        {
            isCalculating = false;
            return;
        }

        // [0]:User, [1]:Ref, [2]:Inter, [3]:Union, [4]:Dot
        NativeArray<float> results = new NativeArray<float>(5, Allocator.TempJob);

        SimilarityJob job = new SimilarityJob
        {
            drawPixels = drawPixels,
            refPixels = refPixelsNative,
            results = results
        };

        JobHandle handle = job.Schedule();
        handle.Complete();

        float userCount = results[0];
        float refCount = results[1];
        float intersection = results[2];
        float union = results[3];
        float dot = results[4];

        float cosine = (userCount * refCount == 0) ? 0f : dot / (Mathf.Sqrt(userCount) * Mathf.Sqrt(refCount));
        float jaccard = (union == 0) ? 0f : intersection / union;
        float dice = (userCount + refCount == 0) ? 0f : (2f * intersection) / (userCount + refCount);

        currentDiceScore = dice;

        //dice 계수로 값 업데이트
        visualSimilarity.UpdateFillAmount(currentDiceScore);

        if (similarityText != null)
        {
            similarityText.text = string.Format($"Sim - Cos: {cosine * 100f:F3}, Jac: {jaccard * 100f:F3}, Dice: {dice * 100f:F3}");
        }

        results.Dispose();
        isCalculating = false;
    }


    public void CreateDrawObj()
    {
        if (MonsterManager.Instance.IsPlayerDead())
        {
            InitLine();
            return;
        }

        try
        {
            float dice = currentDiceScore;

            if (dice * 100.0f > similarityLimit)
            {
                SpawnDrawObj();
                SoundManager.Instance.PlaySound(SoundManager.Effect.SFX_WorkStation_Weapon_Suc_Good.ToString(), false);
            }
            else
            {
                SoundManager.Instance.PlaySound(SoundManager.Effect.SFX_WorkStation_Weapon_Fail.ToString(), false);
                Debug.Log("유사도가 낮아 무기를 생성하지 않음.");
            }

            if (similarityEffect != null)
            {
                similarityEffect.ShowEffect(dice);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[무기 생성 중 에러 발생] : {e}");
        }
        finally
        {
            InitLine();
        }
    }

    public void Undo()
    {
        if (lineRenderers.Count == 0) return;

        // 마지막 라인 제거
        var lastLine = lineRenderers[lineRenderers.Count - 1];
        lineRenderers.RemoveAt(lineRenderers.Count - 1);
        if (lastLine != null) Destroy(lastLine.gameObject);
    }

    private void InitLine()
    {
        foreach (var line in lineRenderers)
        {
            if (line) Destroy(line);
        }

        lineRenderers.Clear();
        points.Clear();
        line = null;
    }

    public void ForceCancelDrawing()
    {
        isAllowDrawing = false; // 그리기 허용 끄기
        InitLine();             // 그리던 선 모두 삭제

        Cursor.visible = true;
    }

    public void SetRefSprite(SpriteRenderer refSprite)
    {
        this.refSprite = refSprite;
        CacheReferenceSprite();
    }

    public void SetWeaponType(WeaponManager.WeaponType weaponType)
    {
        this.weaponType = weaponType;
    }

    private void SpawnDrawObj()
    {
        // 월드 경계값 계산
        Bounds b = CalculateBound(points, lineWidth);
        float padUnits = paddingPixels / (float)pixelPerUnit;
        b.Expand(new Vector3(padUnits * 2.0f, padUnits * 2.0f, 0.0f));

        // 텍스쳐 크기 계산
        int textureW = Mathf.Clamp(Mathf.CeilToInt(b.size.x * pixelPerUnit), TextureMinSize, TextureMaxSize);
        int textureH = Mathf.Clamp(Mathf.CeilToInt(b.size.y * pixelPerUnit), TextureMinSize, TextureMaxSize);

        // Texture2D 생성 (RenderTexture 활용)
        var render = RenderTexture.GetTemporary(textureW, textureH, 0, RenderTextureFormat.ARGB32);
        render.wrapMode = TextureWrapMode.Clamp;
        render.filterMode = FilterMode.Bilinear;

        targetCamera.targetTexture = render;
        targetCamera.orthographicSize = b.size.y * 0.5f;
        targetCamera.aspect = (float)textureW / textureH;
        targetCamera.transform.position = new Vector3(b.center.x, b.center.y, -10f);
        targetCamera.transform.rotation = Quaternion.identity;
        targetCamera.Render();

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = render;

        var texture = new Texture2D(textureW, textureH, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0.0f, 0.0f, textureW, textureH), 0, 0, false);
        texture.Apply();

        RenderTexture.active = prev;
        targetCamera.targetTexture = null;
        RenderTexture.ReleaseTemporary(render);

        // Sprite 생성
        Rect r = new Rect(0, 0, textureW, textureH);

        // world -> pixel scale
        float worldToPixelX = (b.size.x > 0.0001f) ? (textureW / b.size.x) : pixelPerUnit;
        float worldToPixelY = (b.size.y > 0.0001f) ? (textureH / b.size.y) : pixelPerUnit;

        // Pivot 계산
        Vector3 refPivotWorld = refSprite.transform.position;
        Vector2 textureWorldMin = new Vector2(b.center.x - b.size.x * 0.5f, b.center.y - b.size.y * 0.5f);
        float pivotPixelX = (refPivotWorld.x - textureWorldMin.x) * worldToPixelX;
        float pivotPixelY = (refPivotWorld.y - textureWorldMin.y) * worldToPixelY;
        Vector2 normalizedPivot = new Vector2(Mathf.Clamp01(pivotPixelX / (float)textureW), Mathf.Clamp01(pivotPixelY / (float)textureH));

        var s = Sprite.Create(texture, r, normalizedPivot, pixelPerUnit, 0, SpriteMeshType.Tight);

        // 오브젝트화
        var obj = new GameObject("DrawedObject");
        var spriteRender = obj.AddComponent<SpriteRenderer>();
        spriteRender.sprite = s;
        spriteRender.sortingOrder = 100;
        obj.transform.position = refPivotWorld;

        // Physics 적용
        var rigid = obj.AddComponent<Rigidbody2D>();
        rigid.gravityScale = 0.0f;

        AddEdgeCollider(obj, points);
        obj.transform.localScale = Vector3.one; // 초기 스케일 1

        // Weapon 컴포넌트 추가 및 데이터 설정
        var weapon = obj.AddComponent<Weapon>();
        obj.tag = "Weapon";
        weapon.relativeScaleRatio = ratioFromSketchBook; // 배율 저장

        // --- [잉크 및 무기 데이터 셋업] ---
        WeaponInkData weaponDataAsset = weaponInkDataList.Find(data => data.inkData.color == colorType);

        if (weaponDataAsset != null)
        {
            // ColorMixer에서 조합된 색상 가져옴
            ColorMixer.ColorType c1 = ColorMixer.Instance.GetLastMixedColor1();
            ColorMixer.ColorType c2 = ColorMixer.Instance.GetLastMixedColor2();

            WeaponInkData runtimeInkData = Instantiate(weaponDataAsset);
            BaseSkillLogic runtimeSkillLogic;

            if (runtimeInkData.skillLogic == null)
            {
                if (defaultWeaponData != null && defaultWeaponData.skillLogic != null)
                {
                    runtimeSkillLogic = Instantiate(defaultWeaponData.skillLogic);
                }
                else
                {
                    runtimeSkillLogic = null;
                    Debug.LogError("기본 무기 데이터의 스킬 로직도 없습니다.");
                }
            }
            else
            {
                runtimeSkillLogic = Instantiate(weaponDataAsset.skillLogic);
            }

            if (runtimeSkillLogic != null)
            {
                // 스킬 로직에 색상 조합 적용
                runtimeSkillLogic.ApplyColorModifier(c1, c2);
                // 잉크 데이터가 복제된 스킬 로직을 참조
                runtimeInkData.skillLogic = runtimeSkillLogic;
            }

            weapon.Initialize(runtimeInkData, this.weaponType);

            if (WeaponStorageController.Instance != null)
            {
                WeaponStorageController.Instance.StoreNewWeapon(obj);
            }
        }
        else
        {
            if (weaponDataAsset == null)
            {
                Debug.LogError("해당 색상의 무기 데이터가 지정되지 않음. 무기를 생성할 수 없음");
            }
            Destroy(obj);
            return;
        }

        // 색상 초기화
        colorType = ColorMixer.ColorType.None;
    }

    private Bounds CalculateBound(List<Vector3> points, float width)
    {
        if (points.Count == 0) return new Bounds(Vector3.zero, Vector3.zero);

        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;

        foreach (var point in points)
        {
            minX = Mathf.Min(minX, point.x);
            minY = Mathf.Min(minY, point.y);
            maxX = Mathf.Max(maxX, point.x);
            maxY = Mathf.Max(maxY, point.y);
        }

        float expansion = width * 0.5f;
        return new Bounds
        {
            min = new Vector3(minX - expansion, minY - expansion, 0),
            max = new Vector3(maxX + expansion, maxY + expansion, 0)
        };
    }

    private void AddEdgeCollider(GameObject obj, List<Vector3> points)
    {
        List<Vector2> simplified = SimplifyPath(points, 0.1f);
        Vector3 origin = obj.transform.position;
        List<Vector2> localPoints = new List<Vector2>();

        foreach (Vector2 p in simplified)
        {
            localPoints.Add(p - (Vector2)origin);
        }

        var edge = obj.AddComponent<EdgeCollider2D>();
        edge.points = localPoints.ToArray();
        edge.isTrigger = true;
        edge.edgeRadius = lineWidth * 0.5f;
        edge.enabled = false;
    }

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
            return new List<Vector2> { start, end };
        }
    }

    private float PerpendicularDistance(Vector3 point, Vector2 lineStart, Vector2 lineEnd)
    {
        float dx = lineEnd.x - lineStart.x;
        float dy = lineEnd.y - lineStart.y;
        float lenSq = dx * dx + dy * dy;
        if (lenSq == 0) return Vector2.Distance(point, lineStart);

        float t = Mathf.Clamp01(((point.x - lineStart.x) * dx + (point.y - lineStart.y) * dy) / lenSq);
        Vector2 projection = new Vector2(lineStart.x + t * dx, lineStart.y + t * dy);
        return Vector2.Distance(point, projection);
    }
}