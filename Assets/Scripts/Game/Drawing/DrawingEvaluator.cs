using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DrawingEvaluator : GenericSingleton<DrawingEvaluator>
{
    [Header("도안")][SerializeField] private SpriteRenderer refSprite;              //도안이미지
    [Header("유사도 출력 텍스트")][SerializeField] private TMPro.TextMeshProUGUI similarityText;  //유사도 출력 텍스트

    private Camera targetCamera;    //유사도 계산용 카메라

    protected override void Awake()
    {
        base.Awake();

        var cameraObj = new GameObject("EvalCamera");
        targetCamera = cameraObj.AddComponent<Camera>();
        targetCamera.orthographic = true;
        targetCamera.enabled = false;
        targetCamera.clearFlags = CameraClearFlags.SolidColor;
        targetCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        targetCamera.cullingMask = LayerMask.NameToLayer("Drawing");
        cameraObj.hideFlags = HideFlags.HideInHierarchy;
    }

    //튜플 반환 유사도 계산 함수
    public (float cosine, float jaccard, float dice) CalculateSimilarity(List<GameObject> lineRenderers)
    {
        if (refSprite == null || refSprite.sprite == null) return (0f, 0f, 0f);

        //도안의 크기로 설정
        Sprite referenceSprite = refSprite.sprite;
        Bounds refBounds = refSprite.bounds;
        int textureW = referenceSprite.texture.width;
        int textureH = referenceSprite.texture.height;

        // 사용자 그림을 고정된 텍스처에 렌더링
        Texture2D userDrawingTexture = RenderDrawingToTexture(lineRenderers, refBounds, textureW, textureH);

        // 레퍼런스 텍스처 가져오기
        Texture2D referenceTexture = referenceSprite.texture;

        // 이진 벡터로 변환하여 유사도 계산
        float[] userVector = GetAlphaBinaryVector(userDrawingTexture);
        float[] refVector = GetAlphaBinaryVector(referenceTexture);

        DestroyImmediate(userDrawingTexture);

        //길이가 다르면 유사하지 않다고 판단
        if (userVector.Length != refVector.Length) return (0f, 0f, 0f);

        //각 유사도에 쓰일 값
        int intersection = 0;
        int union = 0;
        int userCount = 0;
        int refCount = 0;

        //각 유사도 체크에 쓰일 값들 계산
        for (int i = 0; i < userVector.Length; i++)
        {
            bool u = userVector[i] > 0.5f;
            bool r = refVector[i] > 0.5f;

            if (u) userCount++;
            if (r) refCount++;
            if (u || r) union++;
            if (u && r) intersection++;
        }

        float dot = 0, magU = 0, magR = 0; //내적, 벡터길이
        for (int i = 0; i < userVector.Length; i++)
        {
            dot += userVector[i] * refVector[i];
            magU += userVector[i] * userVector[i];
            magR += refVector[i] * refVector[i];
        }

        //유사도 계산
        float cosine = (magU * magR == 0) ? 0f : dot / (Mathf.Sqrt(magU) * Mathf.Sqrt(magR));
        float jaccard = (union == 0) ? 0f : (float)intersection / union;
        float dice = (userCount + refCount == 0) ? 0f : (2f * intersection) / (userCount + refCount);

        similarityText.text = $"Similarity - Cosine: {cosine * 100f:F3}, Jaccard: {jaccard * 100f:F3}, Dice: {dice * 100f:F3}";

        return (cosine, jaccard, dice);
    }

    public RenderTexture RenderDrawnTexture(List<GameObject> lines, Bounds bounds, int width, int height)
    {
        // 렌더링에 사용되는 targetCamera는 DrawingEvaluator 클래스 내부에서 관리
        RenderTexture render = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

        targetCamera.targetTexture = render;
        targetCamera.orthographicSize = bounds.size.y * 0.5f;
        targetCamera.aspect = (float)width / height;
        targetCamera.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);
        targetCamera.Render();

        return render;
    }

    //고정된 텍스쳐에 렌더(bounds - 도안크기)
    private Texture2D RenderDrawingToTexture(List<GameObject> lines, Bounds bounds, int width, int height)
    {
        //임시 렌더 텍스쳐를 생성
        RenderTexture render = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

        //카메라에 임시 렌더 텍스쳐를 지정
        targetCamera.targetTexture = render;
        targetCamera.orthographicSize = bounds.size.y * 0.5f;
        targetCamera.aspect = (float)width / height;
        targetCamera.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);
        targetCamera.Render();

        //현재 활성 렌더를 저장
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = render;

        //gpu 픽셀 데이터를 읽어 texture로 변환
        //검사용 텍스쳐
        Texture2D evalTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        evalTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
        evalTexture.Apply();

        //원래 랜더로 변경
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(render);

        return evalTexture;
    }

    private float[] GetAlphaBinaryVector(Texture2D texture)
    {
        if (texture == null) return new float[0];

        //텍스쳐 설정에 readable이 false일 시 임시 텍스쳐 만들어서 복사함
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

    //픽셀을 전부 검사하여 Alpha값이 0인지 아닌지 검사
    // 픽셀수만큼의 float 배열을 반환
    private float[] ConvertToBinaryVector(Color[] pixels)
    {
        float[] binaryVector = new float[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
        {
            binaryVector[i] = (pixels[i].a > 0) ? 1.0f : 0.0f;
        }
        return binaryVector;
    }

    public Camera GetCamera() => targetCamera;
}