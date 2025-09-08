using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponFactory : GenericSingleton<WeaponFactory>
{
    [SerializeField] private SpriteRenderer refSprite;
    [SerializeField] private int pixelPerUnit = 100;
    [SerializeField] private int paddingPixels = 12;

    [Header("Color Value")]
    [SerializeField] private TMPro.TextMeshProUGUI totalTxt;
    [SerializeField] private TMPro.TextMeshProUGUI redTxt;
    [SerializeField] private TMPro.TextMeshProUGUI greenTxt;
    [SerializeField] private TMPro.TextMeshProUGUI blueTxt;

    [Space(5f)]
    [Header("유사도 제한")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float similarityValue = 0.7f;

    public void CreateWeapon()
    {
        List<Vector3> points = DrawingManager.Instance.GetPoints();
        List<GameObject> lineRenderers = DrawingManager.Instance.GetLineRenderers();
        float lineWidth = DrawingManager.Instance.GetLineWidth();

        var similarity = DrawingEvaluator.Instance.CalculateSimilarity(lineRenderers);

        if (similarity.dice < similarityValue)
        {
            Debug.Log("weapon create fail");
            return;
        }

        // 무기 생성
        Bounds b = CalculateBound(points, lineWidth);
        float padUnits = paddingPixels / (float)pixelPerUnit;
        b.Expand(new Vector3(padUnits * 2.0f, padUnits * 2.0f, 0.0f));

        int textureW = Mathf.Clamp(Mathf.CeilToInt(b.size.x * pixelPerUnit), 2, 4096);
        int textureH = Mathf.Clamp(Mathf.CeilToInt(b.size.y * pixelPerUnit), 2, 4096);

        // DrawingEvaluator에게 렌더링을 요청하고 결과 텍스처를 받음
        RenderTexture render = DrawingEvaluator.Instance.RenderDrawnTexture(lineRenderers, b, textureW, textureH);

        // 픽셀 개수 셈
        CountPixels(render);

        //렌더 임시 렌더로 변경
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = render;

        Texture2D texture = new Texture2D(textureW, textureH, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0, 0, textureW, textureH), 0, 0, false);
        texture.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(render);

        //Rect 크기 설정
        Rect r = new Rect(0, 0, textureW, textureH);

        float worldToPixelX = (b.size.x > 0.0001f) ? (textureW / b.size.x) : pixelPerUnit;
        float worldToPixelY = (b.size.y > 0.0001f) ? (textureH / b.size.y) : pixelPerUnit;

        Vector3 refPivotWorld = refSprite.transform.position;
        Vector2 textureWorldMin = new Vector2(b.center.x - b.size.x * 0.5f, b.center.y - b.size.y * 0.5f);

        float pivotPixelX = (refPivotWorld.x - textureWorldMin.x) * worldToPixelX;
        float pivotPixelY = (refPivotWorld.y - textureWorldMin.y) * worldToPixelY;

        Vector2 normalizedPivot = new Vector2(Mathf.Clamp01(pivotPixelX / (float)textureW), Mathf.Clamp01(pivotPixelY / (float)textureH));

        Sprite s = Sprite.Create(texture, r, normalizedPivot, pixelPerUnit, 0, SpriteMeshType.Tight);

        GameObject obj = new GameObject("DrawedObject");
        SpriteRenderer spriteRender = obj.AddComponent<SpriteRenderer>();
        spriteRender.sprite = s;
        obj.transform.position = refPivotWorld;

        Rigidbody2D rigid = obj.AddComponent<Rigidbody2D>();
        rigid.gravityScale = 0.0f;

        AddEdgeCollider(obj, points);
    }

    //최대 최소치 계산하여 바운드를 그림 크기만큼 설정
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

    private void AddEdgeCollider(GameObject obj, List<Vector3> points)
    {
        List<Vector2> simplified = SimplifyPath(points, 0.1f);
        Vector3 origin = obj.transform.position;
        List<Vector2> localPoints = new List<Vector2>();

        foreach (Vector2 p in simplified)
        {
            localPoints.Add(p - (Vector2)origin);
        }

        EdgeCollider2D edge = obj.AddComponent<EdgeCollider2D>();
        edge.points = localPoints.ToArray();
        edge.edgeRadius = DrawingManager.Instance.GetLineWidth() * 0.5f;
    }

    //다각형 근사화 알고리즘
    // 시작 점과 끝점부터 안쪽 점까지 거리를 계산 tolerance보다 크면 가능한 지점
    //가능한 거리 ? 중간 나눠 반반 재귀, 아닐 시 중간점 모두 삭제
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
        float len = Mathf.Sqrt(dx * dx + dy * dy);

        //예외처리
        if (len == 0) return Vector2.Distance(point, lineStart);

        //방향 벡터랑 point에서 lineStart로 향하는 벡터를 내적
        //길이의 제곱으로 나눠 정규화 계산
        float t = ((point.x - lineStart.x) * dx + (point.y - lineStart.y) * dy) / (len * len);
        t = Mathf.Clamp(t, 0, 1);   //값 범위를 초과할 경우 보정
        Vector2 projection = new Vector2(lineStart.x + t * dx, lineStart.y + t * dy);   //실제 투영 좌표를 구함

        return Vector2.Distance(point, projection); //거리를 도출
    }

    private int CountPixels(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        Color32[] pixels = tex.GetPixels32();
        int total = 0;
        Dictionary<Color, int> colorCount = new();

        //픽셀 수를 셈 (색 비율 검사)
        foreach (var p in pixels)
        {
            if (p.a == 0) continue;
            if (p.r > 200 && p.g < 50 && p.b < 50) AddColor(colorCount, Color.red, ref total);
            else if (p.g > 200 && p.r < 50 && p.b < 50) AddColor(colorCount, Color.green, ref total);
            else if (p.b > 200 && p.r < 50 && p.g < 50) AddColor(colorCount, Color.blue, ref total);
        }

        totalTxt.text = $"Total Pixel : {total}";
        foreach (var p in colorCount)
        {
            float percent = (p.Value / (float)total) * 100f;
            if (p.Key == Color.red) redTxt.text = $"Red Pixel : {p.Value}({percent:F2}%)";
            else if (p.Key == Color.green) greenTxt.text = $"Green Pixel : {p.Value}({percent:F2}%)";
            else blueTxt.text = $"Blue Pixel : {p.Value}({percent:F2}%)";
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