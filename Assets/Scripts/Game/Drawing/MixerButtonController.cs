using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CursorData
{
    public Texture2D texture;  // 커서 이미지
    public Vector2 hotspot;    // 클릭 위치
    public CursorMode cursorMode; // Auto / ForceSoftware
}

public class MixerButtonController : GenericSingleton<MixerButtonController>
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

    [Header("black, red, yellow, blue, white")]
    [SerializeField] private List<Sprite> color1Result;
    [SerializeField] private UnityEngine.UI.Image color1Render;
    [SerializeField] private List<Sprite> color2Result;
    [SerializeField] private UnityEngine.UI.Image color2Render;


    [Header("Lane Event (black, red, yellow, blue, white)")]
    [SerializeField] private List<UnityEngine.Events.UnityEvent> laneEvents;

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
            //color1Render.color = ColorMixer.Instance.GetColor(colorType);

            c1 = colorType;

            color1Index = Devcat.ValueCastTo<int>.From(colorType);
            color1Render.sprite = color1Result[Devcat.ValueCastTo<int>.From(colorType)];
            selectedColors++;
        }
        else if (selectedColors == 1)
        {
            color2Render.gameObject.SetActive(true);
            //color2Render.color = ColorMixer.Instance.GetColor(colorType);

            convertButtonState?.Invoke();

            color2Index = Devcat.ValueCastTo<int>.From(colorType);
            color2Render.sprite = color2Result[Devcat.ValueCastTo<int>.From(colorType)];
            c2 = colorType;

            Invoke(nameof(OffPanel), 0.5f);
        }
    }

    private int color1Index = -1;
    private int color2Index = -1;

    private void OffPanel()
    {
        //SelectColor();

        WolfWorkStation.Instance.SetIsOnInteractive(false);
        WolfWorkStation.Instance.Interactive();

        //스파인 이벤트 넣을 예정
        // if (color1Index > 0) laneEvents[color1Index]?.Invoke();
        // if (color2Index > 0) laneEvents[color2Index]?.Invoke();

        Debug.Log($"off panel : {c1}, {c2}");

        RailController.Instance.StartDuckSequence(c1, c2);
        Init();
    }

    public void Init()
    {
        first = c1;
        second = c2;

        c1 = ColorMixer.ColorType.None;
        c2 = ColorMixer.ColorType.None;

        color1Index = -1;
        color2Index = -1;

        color1Render.sprite = color1Result[0];
        color2Render.sprite = color2Result[0];

        selectedColors = 0;
    }

    private ColorMixer.ColorType resultColor;
    private void Mix()
    {
        selectedColors = 0;
        resultColor = ColorMixer.Instance.MixColor(first, second);
    }

    // public void SelectColor()
    // {
    //     ColorMixer.Instance.SetFirstColor(c1);
    //     Mix();
    //     cursorIndex = Devcat.ValueCastTo<int>.From(resultColor);

    //     DrawWeapon.Instance.SetLineColor(ColorMixer.Instance.GetColor(resultColor), resultColor);
    //     ColorMixer.Instance.SetFirstColor(ColorMixer.ColorType.White);

    //     if (colorResultSprite != null) colorResultSprite.color = ColorMixer.Instance.GetColor(resultColor);
    // }

    private ColorMixer.ColorType first, second;

    public void SetFirstColor(ColorMixer.ColorType colorType)
    {
        if (first != colorType)
        {
            ColorMixer.ColorType temp = second;
            second = first;
            first = temp;
        }

        Debug.Log(colorType);
        Debug.Log("first color set");
        ColorMixer.Instance.SetFirstColor(colorType);
    }

    public void SetSecondColor()
    {
        Mix();
        ApplyMixedColor();
    }

    public void ApplyMixedColor()
    {
        cursorIndex = Devcat.ValueCastTo<int>.From(resultColor);

        DrawWeapon.Instance.SetLineColor(ColorMixer.Instance.GetColor(resultColor), resultColor);
        ColorMixer.Instance.MixColor(first, second);


        if (colorResultSprite != null) colorResultSprite.color = ColorMixer.Instance.GetColor(resultColor);

        //스케치북 이벤트로 변경
        //SetCursor();
        //Init();
    }

    private int cursorIndex = 0;

    public void SetCursor()
    {
        //Cursor.SetCursor(defaultCursor.texture, defaultCursor.hotspot, defaultCursor.cursorMode);
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
