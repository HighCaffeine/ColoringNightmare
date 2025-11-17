using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class DrawWeapon : GenericSingleton<DrawWeapon>
{
    [Header("Drawing")]
    [SerializeField] private Material lineMaterial; // Sprites / Default 사용
    [SerializeField] private float lineWidth = 0.1f;
    public float LineWidth { get { return lineWidth; } }
    [SerializeField] private float minPointDis = 0.02f; // 포인트간 최소 거리
    [SerializeField] private Color lineColor = Color.white;
    [Range(0.0f, 1.0f)][SerializeField] private float saturation = 1.0f; //채도
    [Range(40.0f, 80.0f)][SerializeField] private float similarityLimit = 60.0f;

    [Header("Drawing Layer")][SerializeField] private LayerMask targetLayer;

    [Header("Sprite Data")]
    [SerializeField] private int pixelPerUnit = 100;
    [SerializeField] private int paddingPixels = 12;
    [SerializeField] private float drawPlaneZValue = 0.0f; //2d라 기본 0.0f로 설정

    [Header("Weapon Size")]
    [Range(0.0f, 1.0f)][SerializeField] private float ratioFromSketchBook;

    [Space(5f)]
    [Header("Weapon Data")]
    [SerializeField] private List<WeaponInkData> weaponInkDataList;
    [Tooltip("목록에 없는 색상으로 제작 시 사용할 기본 무기 데이터")]
    [SerializeField] private WeaponInkData defaultWeaponData;

    private Camera targetCamera; //오브젝트 만들기 위해 가상 레이어 카메라 캐싱
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

    [SerializeField] private LineDrawArea drawArea;
    [SerializeField] private SpriteRenderer refSprite;
    [Space(5f)]
    [Header("유사도")]
    [SerializeField] private TMPro.TextMeshProUGUI similarityText;
    //[SerializeField] private WeaponStorageController weaponStorage;

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;

        var cameraObj = new GameObject("TargetCamera");
        targetCamera = cameraObj.AddComponent<Camera>();
        targetCamera.orthographic = true;
        targetCamera.enabled = false; //수동 렌더를 위해 false
        targetCamera.clearFlags = CameraClearFlags.SolidColor; //단색 타입
        targetCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f); //배경 색 투명
        targetCamera.cullingMask = targetLayer; //target Layer
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

    //그리기 시작
    public void BeginDraw()
    {
        if (!IsAllowEvents()) return;
        if (isCreated)
        {
            points.Clear(); //저장된 포인트 비움
            isCreated = false;
        }
        Vector3 mousePos = GetMousePos();
        CreateNewLine(mousePos);
    }

    //마우스 Pos 저장
    public void ContinueDraw()
    {
        if (!IsAllowEvents() || line == null) return;
        Vector3 pos = GetMousePos();
        pos.z = drawPlaneZValue;
        AppendPoint(pos);
    }

    //그리기 끝
    public void EndDraw()
    {
        if (!IsAllowEvents() || line == null) return;
        if (points.Count == 1)
        {
            lineRenderers.Remove(line.gameObject);
            Destroy(line.gameObject); //풀링으로 추후 변경
            line = null;
            return;
        }
        lastPointCount = 0;
        line = null;
    }

    public void SetLineColor(Color c, ColorMixer.ColorType colorType)
    {
        lineColor = c;
        this.colorType = colorType;
    }

    public void TEST_UpdateLineColor()
    {
        lineColor = ColorMixer.Instance.GetColor(colorType);
    }

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
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; //쉐도우 캐스팅 끔
        line.receiveShadows = false;
        line.sortingOrder = ++beforeLayer; //가장 위에 나오게 최대값
        beforeLayer = line.sortingOrder;
        line.textureMode = LineTextureMode.Tile;
        line.numCornerVertices = 4; //코너 깔끔하게
        line.numCapVertices = 4;

        //마테리얼 컬러 변경 시 사용(인스턴스화 필요)
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

    public void CreateDrawObj()
    {
        if (MonsterManager.Instance.IsPlayerDead())
        {
            Debug.Log("Player is groggy. Weapon creation is canceled.");
            InitLine();
            return;
        }

        //유사도
        var (cos, jaccad, dice) = CalculateSimilarity();

        if (dice * 100.0f > similarityLimit)
        {
            SpawnDrawObj();
            SoundManager.Instance.PlaySound(SoundManager.Effect.SFX_WorkStation_Weapon_Suc_Good.ToString(), false);
        }
        else
        {
            SoundManager.Instance.PlaySound(SoundManager.Effect.SFX_WorkStation_Weapon_Fail.ToString(), false);
            Debug.Log("유사도가 낮아 무기를 생성하지 않습니다.");
        }

        InitLine();
    }

    public void Undo()
    {
        if (lineRenderers.Count == 0) return;
        // 마지막 라인 제거
        var lastLine = lineRenderers[lineRenderers.Count - 1];
        lineRenderers.RemoveAt(lineRenderers.Count - 1);
        // 게임 오브젝트 삭제
        if (lastLine != null)
            Destroy(lastLine.gameObject);
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

    public void SetRefSprite(SpriteRenderer refSprite)
    {
        this.refSprite = refSprite;
    }

    [SerializeField] private DrawSimilarityEffect similarityEffect;

    public (float cosine, float jaccard, float dice) CalculateSimilarity()
    {
        if (refSprite == null || refSprite.sprite == null) return (0, 0, 0);

        Sprite referenceSprite = refSprite.sprite;
        Bounds refBounds = refSprite.bounds;
        int textureW = referenceSprite.texture.width;
        int textureH = referenceSprite.texture.height;

        Texture2D referenceTexture = referenceSprite.texture;
        Texture2D userDrawingTexture = RenderDrawingToTexture(lineRenderers, refBounds, textureW, textureH);

        float[] userVector = GetAlphaBinaryVector(userDrawingTexture);
        float[] refVector = GetAlphaBinaryVector(referenceTexture);

        Object.DestroyImmediate(userDrawingTexture);

        if (userVector.Length != refVector.Length) return (0f, 0f, 0f);

        int intersection = 0, union = 0, userCount = 0, refCount = 0;
        for (int i = 0; i < userVector.Length; i++)
        {
            bool u = userVector[i] > 0.5f;
            bool r = refVector[i] > 0.5f;
            if (u) userCount++;
            if (r) refCount++;
            if (u || r) union++;
            if (u && r) intersection++;
        }

        float dot = 0, magU = 0, magR = 0;
        for (int i = 0; i < userVector.Length; i++)
        {
            dot += userVector[i] * refVector[i];
            magU += userVector[i] * userVector[i];
            magR += refVector[i] * refVector[i];
        }

        float cosine = (magU * magR == 0) ? 0f : dot / (Mathf.Sqrt(magU) * Mathf.Sqrt(magR));
        float jaccard = (union == 0) ? 0f : (float)intersection / union;
        float dice = (userCount + refCount == 0) ? 0f : (2f * intersection) / (userCount + refCount);

        if (similarityText != null)
            similarityText.text = string.Format($"Similarity - Cosine: {cosine * 100f:F3}, Jaccard: {jaccard * 100f:F3}, Dice: {dice * 100f:F3}");

        if (similarityEffect != null)
        {
            similarityEffect.ShowEffect(dice); // dice는 0.0 ~ 1.0 사이의 값
        }

        return (cosine, jaccard, dice);
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

        //Texture2D로 변경
        Texture2D texture = RenderDrawingToTexture(lineRenderers, b, textureW, textureH);

        //Sprite 생성
        Rect r = new Rect(0, 0, textureW, textureH);

        // world -> pixel scale : (texture 픽 수) / (b.size in world units)
        // b.size.x 이 0 이 되는 극단 상황 방지
        float worldToPixelX = (b.size.x > 0.0001f) ? (textureW / b.size.x) : pixelPerUnit;
        float worldToPixelY = (b.size.y > 0.0001f) ? (textureH / b.size.y) : pixelPerUnit;

        //ref pivot 의 월드 위치 (SpriteRenderer.transform.position은 Sprite의 pivot 위치임)
        Vector3 refPivotWorld = refSprite.transform.position;

        //텍스처에서의 왼쪽하단 월드 좌표 = b.center - 0.5 * b.size
        Vector2 textureWorldMin = new Vector2(b.center.x - b.size.x * 0.5f, b.center.y - b.size.y * 0.5f);

        //ref pivot이 텍스처 내 어느 픽셀에 대응하는지 계산
        float pivotPixelX = (refPivotWorld.x - textureWorldMin.x) * worldToPixelX;
        float pivotPixelY = (refPivotWorld.y - textureWorldMin.y) * worldToPixelY;

        //정규화
        float normPX = Mathf.Clamp01(pivotPixelX / (float)textureW);
        float normPY = Mathf.Clamp01(pivotPixelY / (float)textureH);
        Vector2 normalizedPivot = new Vector2(normPX, normPY);

        var s = Sprite.Create(texture, r, normalizedPivot, pixelPerUnit, 0, SpriteMeshType.Tight);

        //오브젝트화
        var obj = new GameObject("DrawedObject");
        var spriteRender = obj.AddComponent<SpriteRenderer>();
        var weapon = obj.AddComponent<Weapon>();
        obj.tag = "Weapon";

        spriteRender.sortingOrder = 100;
        spriteRender.sprite = s;
        obj.transform.position = refPivotWorld;

        //Physics 적용
        var rigid = obj.AddComponent<Rigidbody2D>();
        rigid.gravityScale = 0.0f;

        AddEdgeCollider(obj, points);
        obj.transform.localScale = Vector3.one;

        if (weapon != null)
        {
            weapon.relativeScaleRatio = ratioFromSketchBook;
        }
        //잉크 데이터 셋업
        WeaponInkData weaponDataAsset = weaponInkDataList.Find(data => data.inkData.color == colorType);

        if (weaponDataAsset != null)
        {
            // ColorMixer에서 조합에 사용된 두 색상을 가져옴
            ColorMixer.ColorType c1 = ColorMixer.Instance.GetLastMixedColor1();
            ColorMixer.ColorType c2 = ColorMixer.Instance.GetLastMixedColor2();

            WeaponInkData runtimeInkData = Instantiate(weaponDataAsset);
            BaseSkillLogic runtimeSkillLogic;

            if (runtimeInkData.skillLogic == null)
            {
                Debug.LogWarning($"'{colorType}' 색상의 스킬 데이터가 없음. 기본 무기 데이터로 대체");
                runtimeSkillLogic = defaultWeaponData.skillLogic;
            }
            else
            {
                runtimeSkillLogic = Instantiate(weaponDataAsset.skillLogic);
            }

            // 스킬 로직에 색상 조합 적용
            runtimeSkillLogic.ApplyColorModifier(c1, c2);

            // 잉크 데이터가 복제된 스킬 로직을 참조
            runtimeInkData.skillLogic = runtimeSkillLogic;
            runtimeInkData.weaponType = weaponType;

            // Weapon에 원본 대신 복사본 설정
            weapon.SetupInkData(runtimeInkData);

            if (WeaponStorageController.Instance != null)
            {
                WeaponStorageController.Instance.StoreNewWeapon(obj);
            }
        }
        else
        {
            if (weaponDataAsset == null)
            {
                Debug.LogError("기본 무기 데이터가 지정되지 않음. 무기를 생성할 수 없음");
            }
            else if (weaponDataAsset.skillLogic == null)
            {
                Debug.LogError($"'{weaponDataAsset.name}'에 skillLogic이 지정되지 않음. 무기를 생성할 수 없음");
            }
            Destroy(obj);
            return;
        }

        // if (weaponStorage != null)
        // {
        //     weaponStorage.StoreWeapon(obj);
        // }

        colorType = ColorMixer.ColorType.None;
    }

    public void SetWeaponType(WeaponManager.WeaponType weaponType)
    {
        this.weaponType = weaponType;
    }

    private Texture2D RenderDrawingToTexture(List<GameObject> lines, Bounds bounds, int width, int height)
    {
        var render = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

        targetCamera.targetTexture = render;
        targetCamera.orthographicSize = bounds.size.y * 0.5f;
        targetCamera.aspect = (float)width / height;
        targetCamera.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);
        targetCamera.Render();

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = render;

        var evalTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        evalTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
        evalTexture.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(render);

        return evalTexture;
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
        Bounds b = new Bounds();
        b.min = new Vector3(minX - expansion, minY - expansion, 0);
        b.max = new Vector3(maxX + expansion, maxY + expansion, 0);
        return b;
    }

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
        edge.isTrigger = true;
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
        Vector2 end = points[points.Count - 1];
        //가장 먼 점을 찾음
        for (int i = 1; i < points.Count - 1; i++)
        {
            float distance = PerpendicularDistance(points[i], start, end);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                d = i;
            }
        }
        //임계값 범위내
        if (maxDistance > tolerance)
        {
            //시작 - 끝 지점을 구함
            //시작 중간 / 중간 끝 두 구역으로 나눠서 재귀 반복
            List<Vector2> results1 = SimplifyPath(points.GetRange(0, d + 1), tolerance);
            List<Vector2> results2 = SimplifyPath(points.GetRange(d, points.Count - d), tolerance);
            results1.RemoveAt(results1.Count - 1);
            results1.AddRange(results2);
            return results1;
        }
        else //임계값 벗어남
        {
            //여기 부분에 존재하는 중간 점들은 모두 제거
            return new List<Vector2> { start, end };
        }
    }

    // 점과 선분 사이의 수직 거리 계산
    private float PerpendicularDistance(Vector3 point, Vector2 lineStart, Vector2 lineEnd)
    {
        //방향 벡터
        float dx = lineEnd.x - lineStart.x;
        float dy = lineEnd.y - lineStart.y;
        float lenSq = dx * dx + dy * dy; //피타고라스 정리
        //예외처리
        if (lenSq == 0) return Vector2.Distance(point, lineStart);
        //방향 벡터랑 point에서 linestart로 향하는 벡터를 내적, 길이 제곱으로 정규화
        float t = Mathf.Clamp01(((point.x - lineStart.x) * dx + (point.y - lineStart.y) * dy) / lenSq);
        //실제 좌표 구함
        Vector2 projection = lineStart + t * new Vector2(dx, dy);
        //거리 도출
        return Vector2.Distance(point, projection);
    }

    private static float[] GetAlphaBinaryVector(Texture2D texture)
    {
        if (!texture.isReadable)
        {
            RenderTexture tempRenderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(texture, tempRenderTexture);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = tempRenderTexture;
            Texture2D tempTexture = new Texture2D(texture.width, texture.height);
            tempTexture.ReadPixels(new Rect(0, 0, tempRenderTexture.width, tempRenderTexture.height), 0, 0);
            tempTexture.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(tempRenderTexture);
            Color[] pixels = tempTexture.GetPixels();
            Object.DestroyImmediate(tempTexture);
            return ConvertToBinaryVector(pixels);
        }
        else
        {
            return ConvertToBinaryVector(texture.GetPixels());
        }
    }

    private static float[] ConvertToBinaryVector(Color[] pixels)
    {
        float[] binaryVector = new float[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
        {
            binaryVector[i] = (pixels[i].a > 0.1f) ? 1.0f : 0.0f;
        }
        return binaryVector;
    }
}