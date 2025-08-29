#if UNITY_EDITOR_64
using UnityEngine;
using UnityEditor;
using System.IO;

public class WeaponDesignerWindow : EditorWindow
{
    Texture2D canvasTex;
    Color drawColor = Color.black;
    Vector2 pivot = new Vector2(0.5f, 0.5f);
    int brushSize = 4;
    bool isDrawing = false;

    [MenuItem("Tools/Weapon Designer")]
    static void OpenWindow()
    {
        GetWindow<WeaponDesignerWindow>("Weapon Designer");
    }

    void OnEnable()
    {
        InitCanvas();
    }

    void InitCanvas()
    {
        canvasTex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        ClearCanvas();
    }

    void OnGUI()
    {
        GUILayout.Label("Weapon Designer", EditorStyles.boldLabel);

        // 색상 선택
        drawColor = EditorGUILayout.ColorField("Brush Color", drawColor);

        // 브러시 크기
        brushSize = EditorGUILayout.IntSlider("Brush Size", brushSize, 1, 32);

        // Pivot 설정
        pivot = EditorGUILayout.Vector2Field("Pivot (0~1)", pivot);

        GUILayout.Space(10);

        // 캔버스 표시
        Rect canvasRect = GUILayoutUtility.GetRect(256, 256, GUILayout.ExpandWidth(false));
        EditorGUI.DrawPreviewTexture(canvasRect, canvasTex);

        HandleCanvasInput(canvasRect);

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear"))
        {
            ClearCanvas();
        }
        if (GUILayout.Button("Save Sprite"))
        {
            SaveSprite();
        }
        GUILayout.EndHorizontal();
    }

    void HandleCanvasInput(Rect canvasRect)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && canvasRect.Contains(e.mousePosition))
        {
            isDrawing = true;
        }

        if (e.type == EventType.MouseUp)
        {
            isDrawing = false;
        }

        if (isDrawing && e.type == EventType.MouseDrag)
        {
            Vector2 localPos = e.mousePosition - canvasRect.position;
            int x = Mathf.FloorToInt(localPos.x);
            int y = Mathf.FloorToInt(canvasTex.height - localPos.y);

            DrawCircle(x, y, brushSize, drawColor);
            canvasTex.Apply();
            Repaint();
        }
    }

    void DrawCircle(int cx, int cy, int r, Color col)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && px < canvasTex.width && py >= 0 && py < canvasTex.height)
                    {
                        canvasTex.SetPixel(px, py, col);
                    }
                }
            }
        }
    }

    void ClearCanvas()
    {
        Color[] colors = new Color[canvasTex.width * canvasTex.height];
        for (int i = 0; i < colors.Length; i++) colors[i] = Color.clear;
        canvasTex.SetPixels(colors);
        canvasTex.Apply();
    }

    void SaveSprite()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Weapon Sprite", "Weapon", "png", "Enter file name");
        if (string.IsNullOrEmpty(path)) return;

        // PNG로 저장
        byte[] bytes = canvasTex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path);

        // Sprite 설정
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePivot = pivot;
        ti.spritePixelsPerUnit = 100;
        ti.SaveAndReimport();

        Debug.Log($"Weapon Sprite saved at {path} with pivot {pivot}");
    }
}

#endif