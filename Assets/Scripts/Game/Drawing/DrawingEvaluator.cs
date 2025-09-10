using UnityEngine;

public class DrawingEvaluator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer refSprite;
    [SerializeField] private TMPro.TextMeshProUGUI similarityText;

    public (float cosine, float jaccard, float dice) CalculateSimilarity()
    {
        var drawingManager = DrawingManager.Instance;

        Sprite referenceSprite = refSprite.sprite;
        Bounds refBounds = refSprite.bounds;
        int textureW = referenceSprite.texture.width;
        int textureH = referenceSprite.texture.height;

        Texture2D referenceTexture = referenceSprite.texture;
        Texture2D userDrawingTexture = RenderDrawingToTexture(drawingManager.GetLineRenderers(), refBounds, textureW, textureH);

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

        similarityText.text = $"Similarity - Cosine: {cosine * 100f:F3}, Jaccard: {jaccard * 100f:F3}, Dice: {dice * 100f:F3}";
        return (cosine, jaccard, dice);
    }

    private Texture2D RenderDrawingToTexture(System.Collections.Generic.List<GameObject> lines, Bounds bounds, int width, int height)
    {
        var targetCamera = DrawingManager.Instance.GetTargetCamera();
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

    private static float[] GetAlphaBinaryVector(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        float[] binaryVector = new float[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
            binaryVector[i] = (pixels[i].a > 0) ? 1.0f : 0.0f;
        return binaryVector;
    }
}
