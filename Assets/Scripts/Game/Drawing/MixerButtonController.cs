using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class CursorData
{
    public Texture2D texture;  // 커서 이미지
    public Vector2 hotspot;    // 클릭 위치
    public CursorMode cursorMode; // Auto / ForceSoftware
}

[System.Serializable]
public class RailEvents
{
    public UnityEngine.Events.UnityEvent black;
    public UnityEngine.Events.UnityEvent red;
    public UnityEngine.Events.UnityEvent yellow;
    public UnityEngine.Events.UnityEvent blue;

    public UnityEngine.Events.UnityEvent this[ColorMixer.ColorType colorType]
    {
        get
        {
            switch (colorType)
            {
                case ColorMixer.ColorType.Black:
                    return black;
                case ColorMixer.ColorType.Red:
                    return red;
                case ColorMixer.ColorType.Yellow:
                    return yellow;
                case ColorMixer.ColorType.Blue:
                    return blue;
                default:
                    Debug.LogError("정의되지 않은 색: " + colorType);
                    return null;
            }
        }
    }
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

    [Header("black, red, yellow, blue")]
    [SerializeField] private List<Sprite> color1Result;
    [SerializeField] private UnityEngine.UI.Image color1Render;
    [SerializeField] private List<Sprite> color2Result;
    [SerializeField] private UnityEngine.UI.Image color2Render;


    [Header("Rail Spine Event (black, red, yellow, blue)")]
    [SerializeField] private RailEvents railEvents;

    [Header("Palette Dolls")]
    [SerializeField] private PaletteDollController blackDoll;
    [SerializeField] private PaletteDollController redDoll;
    [SerializeField] private PaletteDollController blueDoll;
    [SerializeField] private PaletteDollController yellowDoll;


    public float RailSpineDuration => 0.5f;

    private int selectedColors = 0;

    private ColorMixer.ColorType c1, c2;



    public void ColorButton_Black() { SelectColor(ColorMixer.ColorType.Black); if (blackDoll != null) blackDoll.PlaySelect(); }
    public void ColorButton_Yellow() { SelectColor(ColorMixer.ColorType.Yellow); if (yellowDoll != null) yellowDoll.PlaySelect(); }
    public void ColorButton_Red() { SelectColor(ColorMixer.ColorType.Red); if (redDoll != null) redDoll.PlaySelect(); }
    public void ColorButton_Blue() { SelectColor(ColorMixer.ColorType.Blue); if (blueDoll != null) blueDoll.PlaySelect(); }

    private void SelectColor(ColorMixer.ColorType colorType)
    {
        if (!syringers[Devcat.ValueCastTo<int>.From(colorType)].IsAllowUse())
        {
            Debug.Log($"[Palette Select Fail] {colorType} :  zero amount");
            return;
        }

        if (selectedColors == 0)
        {

            if (colorType == ColorMixer.ColorType.Black)
            {
                lockColor = true;

                for (int i = 0; i < 3; i++)
                {
                    int index = Devcat.ValueCastTo<int>.From(ColorMixer.ColorType.Red + i);
                    OnLockColorEvent[ColorMixer.ColorType.Red + i]?.Invoke();
                }
            }
            else
            {
                lockBlack = true;
                OnLockColorEvent[ColorMixer.ColorType.Black]?.Invoke();
            }

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
    [Header("Black, Red, Yellow, Blue")]
    [SerializeField] private List<SyringeController> syringers;
    [SerializeField] private UnityEngine.Events.UnityEvent OnInitLockEvent;

    [SerializeField] private RailEvents OnLockColorEvent;
    private bool lockColor = false;
    private bool lockBlack = false;

    private int color1Index = -1;
    private int color2Index = -1;

    public void AddInk(ColorMixer.ColorType colorType)
    {
        syringers[Devcat.ValueCastTo<int>.From(colorType)].AddInk();
    }

    public void SubInk(ColorMixer.ColorType colorType)
    {
        syringers[Devcat.ValueCastTo<int>.From(colorType)].UseInk();
    }

    private void OffPanel()
    {
        //SelectColor();

        WolfWorkStation.Instance.SetIsOnInteractive(false);
        WolfWorkStation.Instance.Interactive();

        //스파인 이벤트 넣을 예정
        // if (color1Index > 0) laneEvents[color1Index]?.Invoke();
        // if (color2Index > 0) laneEvents[color2Index]?.Invoke();

        Debug.Log($"off panel : {c1}, {c2}");

        SubInk(c1);
        SubInk(c2);
        RailController.Instance.StartDuckSequence(c1, c2, railEvents[c1], railEvents[c2]);

        Init();
    }

    public void Init()
    {
        first = c1;
        second = c2;

        c1 = ColorMixer.ColorType.None;
        c2 = ColorMixer.ColorType.None;

        OnInitLockEvent?.Invoke();

        for (int i = 0; i < 4; i++)
        {
            int index = Devcat.ValueCastTo<int>.From(ColorMixer.ColorType.Black + i);
            if (!syringers[index].IsAllowUse()) OnLockColorEvent[ColorMixer.ColorType.Black + i]?.Invoke();
        }

        lockColor = false;
        lockBlack = false;

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

    //     DrawWeaponGPU.Instance.SetLineColor(ColorMixer.Instance.GetColor(resultColor), resultColor);
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

        DrawWeaponGPU.Instance.SetLineColor(ColorMixer.Instance.GetColor(resultColor), resultColor);
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
