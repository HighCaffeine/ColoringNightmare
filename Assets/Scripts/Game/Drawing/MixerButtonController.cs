using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CursorData
{
    public Texture2D texture;  // 커서 이미지
    public Vector2 hotspot;    // 클릭 위치
    public CursorMode cursorMode; // Auto / ForceSoftware
}

public class MixerButtonController : MonoBehaviour
{
    public UnityEngine.Events.UnityEvent convertButtonState;

    [TextArea(5, 20)]
    public string colorOrderExplanation =
    @"아래 순서로 커서 넣어주세요
    Black
    Red
    Yellow
    Blue
    White
    Orange
    Cyan
    Magenta
    Gray
    DarkRed
    LightRed
    DarkYellow
    LightYellow
    DarkBlue
    LightBlue";

    [SerializeField] private List<CursorData> cursors;

    [SerializeField] private CursorData defaultCursor;

    [SerializeField] private SpriteRenderer colorResultSprite;

    [SerializeField] private UnityEngine.UI.Image color1Render;
    [SerializeField] private UnityEngine.UI.Image color2Render;

    private int selectedColors = 0;

    private ColorMixer.ColorType c1, c2;

    public void ColorButton_Black() { SelectColor(ColorMixer.ColorType.Black); }
    public void ColorButton_Yellow() { SelectColor(ColorMixer.ColorType.Yellow); }
    public void ColorButton_Red() { SelectColor(ColorMixer.ColorType.Red); }
    public void ColorButton_Blue() { SelectColor(ColorMixer.ColorType.Blue); }
    public void ColorButton_White() { SelectColor(ColorMixer.ColorType.White); }

    private void SelectColor(ColorMixer.ColorType colorType)
    {
        if (selectedColors == 0)
        {
            color1Render.gameObject.SetActive(true);
            color1Render.color = ColorMixer.Instance.GetColor(colorType);

            c1 = colorType;
            selectedColors++;
        }
        else if (selectedColors == 1)
        {
            color2Render.gameObject.SetActive(true);
            color2Render.color = ColorMixer.Instance.GetColor(colorType);

            convertButtonState?.Invoke();

            c2 = colorType;
        }
    }

    public void Init()
    {
        c1 = ColorMixer.ColorType.None;
        c2 = ColorMixer.ColorType.None;
        selectedColors = 0;
    }

    private ColorMixer.ColorType resultColor;
    private void Mix()
    {
        selectedColors = 0;
        resultColor = ColorMixer.Instance.MixColor(c1, c2);
    }

    public void SelectColor()
    {
        ColorMixer.Instance.SetFirstColor(c1);
        Mix();
        cursorIndex = Devcat.ValueCastTo<int>.From(resultColor);

        DrawWeapon.Instance.SetLineColor(ColorMixer.Instance.GetColor(resultColor), resultColor);
        ColorMixer.Instance.SetFirstColor(ColorMixer.ColorType.White);

        if (colorResultSprite != null) colorResultSprite.color = ColorMixer.Instance.GetColor(resultColor);
    }

    private int cursorIndex = 0;

    public void SetCursor()
    {
        Cursor.SetCursor(cursors[cursorIndex].texture, cursors[cursorIndex].hotspot, cursors[cursorIndex].cursorMode);
    }

    public void SetDefaultCursor()
    {
        if (defaultCursor.texture != null)
        {
            Cursor.SetCursor(defaultCursor.texture, defaultCursor.hotspot, defaultCursor.cursorMode);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}
