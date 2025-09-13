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
            c1 = colorType;
            ColorMixer.Instance.SetFirstColor(c1);
            selectedColors++;
        }
        else if (selectedColors == 1)
        {
            convertButtonState?.Invoke();
            c2 = colorType;
            Mix();
        }
    }

    private ColorMixer.ColorType resultColor;
    private void Mix()
    {
        selectedColors = 0;
        resultColor = ColorMixer.Instance.MixColor(c1, c2);
    }

    public void SelectColor()
    {
        int i = Devcat.ValueCastTo<int>.From(resultColor);
        //Cursor.SetCursor(cursors[i].texture, cursors[i].hotspot, cursors[i].cursorMode);
        convertButtonState?.Invoke();

        DrawWeapon.Instance.SetLineColor(ColorMixer.Instance.GetColor(resultColor));
        ColorMixer.Instance.SetFirstColor(ColorMixer.ColorType.White);
    }
}
