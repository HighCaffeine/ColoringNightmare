using UnityEngine;
using System.Collections.Generic;

public class DrawManager : MonoBehaviour
{
    private LineRenderer currentLine;
    private List<Vector3> points = new List<Vector3>();

    public float lineWidth = 0.1f;
    public float simplifyTolerance = 0.05f; // 외곽선 단순화 정도
    public float pixelSize = 0.05f;         // 픽셀 → 월드 변환 비율

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CreateNewLine();
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            if (points.Count == 0 || Vector3.Distance(points[^1], mousePos) > 0.01f) // 너무 가까운 점은 무시
            {
                points.Add(mousePos);
                currentLine.positionCount = points.Count;
                currentLine.SetPositions(points.ToArray());
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            FinishLine();
        }
    }

    void CreateNewLine()
    {
        GameObject go = new GameObject("DrawedObject");
        currentLine = go.AddComponent<LineRenderer>();
        currentLine.positionCount = 0;
        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;

        points.Clear();
    }

    void FinishLine()
    {
        if (points.Count < 3) return;

        GameObject go = currentLine.gameObject;

        // Sprite/Texture로 변환해서 외곽선 추출
        Texture2D maskTex = RenderLineToTexture(go, points);

        // 폴리곤 콜라이더 생성
        AddPolygonCollider(go, maskTex);

        // Rigidbody2D 붙이기 (물리 적용)
        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    // 라인 → Texture2D 변환
    Texture2D RenderLineToTexture(GameObject go, List<Vector3> linePoints)
    {
        int texSize = 256;
        Texture2D tex = new Texture2D(texSize, texSize, TextureFormat.ARGB32, false);
        Color32[] clear = new Color32[texSize * texSize];
        for (int i = 0; i < clear.Length; i++) clear[i] = new Color32(0, 0, 0, 0);
        tex.SetPixels32(clear);

        foreach (Vector3 p in linePoints)
        {
            int x = Mathf.Clamp((int)((p.x - go.transform.position.x) / pixelSize + texSize / 2), 0, texSize - 1);
            int y = Mathf.Clamp((int)((p.y - go.transform.position.y) / pixelSize + texSize / 2), 0, texSize - 1);
            tex.SetPixel(x, y, Color.white);
        }

        tex.Apply();
        return tex;
    }

    // 외곽선 기반 폴리곤 콜라이더 추가
    void AddPolygonCollider(GameObject obj, Texture2D maskTex)
    {
        PolygonCollider2D poly = obj.GetComponent<PolygonCollider2D>();
        if (poly == null) poly = obj.AddComponent<PolygonCollider2D>();

        List<Vector2> outline = ExtractOutline(maskTex);
        List<Vector2> simplified = RamerDouglasPeucker(outline, simplifyTolerance);

        if (simplified.Count >= 3)
        {
            poly.pathCount = 1;
            poly.SetPath(0, simplified.ToArray());
        }
    }

    // Marching Squares 기반 외곽선 추출
    List<Vector2> ExtractOutline(Texture2D tex)
    {
        List<Vector2> points = new List<Vector2>();
        int w = tex.width;
        int h = tex.height;
        Color32[] pixels = tex.GetPixels32();

        for (int y = 0; y < h - 1; y++)
        {
            for (int x = 0; x < w - 1; x++)
            {
                int squareIndex = 0;
                if (IsFilled(pixels, x, y, w)) squareIndex |= 1;
                if (IsFilled(pixels, x + 1, y, w)) squareIndex |= 2;
                if (IsFilled(pixels, x + 1, y + 1, w)) squareIndex |= 4;
                if (IsFilled(pixels, x, y + 1, w)) squareIndex |= 8;

                if (squareIndex != 0 && squareIndex != 15)
                {
                    points.Add(new Vector2((x - w / 2) * pixelSize, (y - h / 2) * pixelSize));
                }
            }
        }

        return points;
    }

    bool IsFilled(Color32[] pixels, int x, int y, int width)
    {
        if (x < 0 || y < 0 || y * width + x >= pixels.Length) return false;
        Color32 c = pixels[y * width + x];
        return c.a > 128 && (c.r + c.g + c.b) > 50;
    }

    // --- Ramer–Douglas–Peucker 알고리즘 ---
    List<Vector2> RamerDouglasPeucker(List<Vector2> points, float tolerance)
    {
        if (points == null || points.Count < 3)
            return new List<Vector2>(points);

        int index = -1;
        float distMax = 0f;

        for (int i = 1; i < points.Count - 1; i++)
        {
            float dist = PerpendicularDistance(points[i], points[0], points[^1]);
            if (dist > distMax)
            {
                distMax = dist;
                index = i;
            }
        }

        if (distMax > tolerance)
        {
            List<Vector2> left = RamerDouglasPeucker(points.GetRange(0, index + 1), tolerance);
            List<Vector2> right = RamerDouglasPeucker(points.GetRange(index, points.Count - index), tolerance);

            left.RemoveAt(left.Count - 1);
            left.AddRange(right);
            return left;
        }
        else
        {
            return new List<Vector2>() { points[0], points[^1] };
        }
    }

    float PerpendicularDistance(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        if (lineStart == lineEnd)
            return Vector2.Distance(point, lineStart);

        float num = Mathf.Abs((lineEnd.y - lineStart.y) * point.x -
                              (lineEnd.x - lineStart.x) * point.y +
                              lineEnd.x * lineStart.y -
                              lineEnd.y * lineStart.x);

        float den = Mathf.Sqrt(Mathf.Pow(lineEnd.y - lineStart.y, 2) + Mathf.Pow(lineEnd.x - lineStart.x, 2));
        return num / den;
    }
}
