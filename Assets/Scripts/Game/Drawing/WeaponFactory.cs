using System.Collections.Generic;
using UnityEngine;

public class WeaponFactory : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private DrawingManager drawingManager;
    [SerializeField] private DrawingEvaluator evaluator;
    [SerializeField] private SpriteRenderer refSprite;
    [SerializeField] private List<WeaponInkData> weaponInkDataList;

    public void CreateDrawObj()
    {
        if (evaluator.IsDrawingValid())
        {
            SpawnDrawObj();
        }
        else
        {
            Debug.Log("유사도가 낮아 무기를 생성하지 않습니다.");
        }
        drawingManager.ClearLines();
    }

    private void SpawnDrawObj()
    {
        List<Vector3> points = drawingManager.GetPoints();
        float lineWidth = drawingManager.GetLineWidth();

        Bounds b = CalculateBound(points, lineWidth);

        float padUnits = drawingManager.GetPaddingPixels() / (float)drawingManager.GetPixelPerUnit();
        b.Expand(new Vector3(padUnits * 2.0f, padUnits * 2.0f, 0.0f));

        int pixelPerUnit = drawingManager.GetPixelPerUnit();
        int textureW = Mathf.Clamp(Mathf.CeilToInt(b.size.x * pixelPerUnit), 2, 4096);
        int textureH = Mathf.Clamp(Mathf.CeilToInt(b.size.y * pixelPerUnit), 2, 4096);

        var render = new RenderTexture(textureW, textureH, 0, RenderTextureFormat.ARGB32);
        render.wrapMode = TextureWrapMode.Clamp;
        render.filterMode = FilterMode.Bilinear;

        var targetCamera = drawingManager.GetTargetCamera();
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
        render.Release();
        Destroy(render);

        Rect r = new Rect(0, 0, textureW, textureH);
        float worldToPixelX = (b.size.x > 0.0001f) ? (textureW / b.size.x) : pixelPerUnit;
        float worldToPixelY = (b.size.y > 0.0001f) ? (textureH / b.size.y) : pixelPerUnit;

        Vector3 refPivotWorld = refSprite.transform.position;
        Vector2 textureWorldMin = new Vector2(b.center.x - b.size.x * 0.5f, b.center.y - b.size.y * 0.5f);

        float pivotPixelX = (refPivotWorld.x - textureWorldMin.x) * worldToPixelX;
        float pivotPixelY = (refPivotWorld.y - textureWorldMin.y) * worldToPixelY;

        Vector2 normalizedPivot = new Vector2(Mathf.Clamp01(pivotPixelX / (float)textureW), Mathf.Clamp01(pivotPixelY / (float)textureH));

        var s = Sprite.Create(texture, r, normalizedPivot, pixelPerUnit, 0, SpriteMeshType.Tight);

        var obj = new GameObject("DrawedObject");
        var spriteRender = obj.AddComponent<SpriteRenderer>();
        spriteRender.sprite = s;
        obj.transform.position = refPivotWorld;

        var rigid = obj.AddComponent<Rigidbody2D>();
        rigid.gravityScale = 0.0f;

        AddEdgeCollider(obj, points, lineWidth);

        obj.transform.localScale = Vector3.one * drawingManager.GetRatioFromSketchBook();

        // Weapon 컴포넌트 추가 및 데이터 설정
        var weapon = obj.AddComponent<Weapon>();
        obj.tag = "Weapon";
        WeaponInkData weaponData = null;

        foreach (var data in weaponInkDataList)
        {
            if (data.inkData.color == DrawingManager.Instance.GetColorType())
            {
                weaponData = data;

                break;
            }
        }

        weapon.SetupInkData(weaponData);
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

    private void AddEdgeCollider(GameObject obj, List<Vector3> points, float lineWidth)
    {
        List<Vector2> simplified = SimplifyPath(points, 0.1f);
        Vector3 origin = obj.transform.position;
        List<Vector2> localPoints = new List<Vector2>();
        foreach (Vector2 p in simplified) localPoints.Add(p - (Vector2)origin);

        var edge = obj.AddComponent<EdgeCollider2D>();
        edge.points = localPoints.ToArray();
        edge.edgeRadius = lineWidth * 0.5f;
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
        float len = Mathf.Sqrt(dx * dx + dy * dy);
        if (len == 0) return Vector2.Distance(point, lineStart);

        float t = ((point.x - lineStart.x) * dx + (point.y - lineStart.y) * dy) / (len * len);
        t = Mathf.Clamp(t, 0, 1);
        Vector2 projection = new Vector2(lineStart.x + t * dx, lineStart.y + t * dy);
        return Vector2.Distance(point, projection);
    }
}