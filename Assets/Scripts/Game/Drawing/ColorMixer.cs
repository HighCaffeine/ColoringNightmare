using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColorMixer : GenericSingleton<ColorMixer>
{
    public const int MaxColor = 2;


    [Header("Blend Settings")]
    [SerializeField] private float blendDuration = 2f; // 혼합 완료까지 시간
    //[SerializeField] private UnityEngine.UI.Image result;
    [SerializeField] private SpriteRenderer result;


    public class HSV
    {
        public float h { private set; get; }
        public float s { private set; get; }
        public float v { private set; get; }

        public HSV(float h, float s, float v)
        {
            this.h = h / 360.0f;
            this.s = s / 100.0f;
            this.v = v / 100.0f;
        }
    }

    public enum ColorType
    {
        None = -1,

        Black, Red, Yellow, Blue, White,
        Orange, Cyan, Magenta, Gray,
        DarkRed, LightRed,
        DarkYellow, LightYellow,
        DarkBlue, LightBlue,

        Count
    }
    // 2색 조합 → 최종 색
    private Dictionary<(ColorType, ColorType), ColorType> colorMap = new Dictionary<(ColorType, ColorType), ColorType>()
    {
        {(ColorType.Red,    ColorType.Red),     ColorType.Red},
        {(ColorType.Red,    ColorType.Yellow),  ColorType.Orange},
        {(ColorType.Yellow, ColorType.Red),     ColorType.Orange},
        {(ColorType.Red,    ColorType.Blue),    ColorType.Magenta},
        {(ColorType.Blue,   ColorType.Red),     ColorType.Magenta},
        {(ColorType.Yellow, ColorType.Yellow),  ColorType.Yellow},
        {(ColorType.Yellow, ColorType.Blue),    ColorType.Cyan},
        {(ColorType.Blue,   ColorType.Yellow),  ColorType.Cyan},
        {(ColorType.Blue,   ColorType.Blue),    ColorType.Blue},
        {(ColorType.Black,  ColorType.White),   ColorType.Gray},
        {(ColorType.White,  ColorType.Black),   ColorType.Gray},
        {(ColorType.Red,    ColorType.White),   ColorType.LightRed},
        {(ColorType.Yellow, ColorType.White),   ColorType.LightYellow},
        {(ColorType.Blue,   ColorType.White),   ColorType.LightBlue},
        {(ColorType.Black,  ColorType.Red),     ColorType.DarkRed},
        {(ColorType.Black,  ColorType.Yellow),  ColorType.DarkYellow},
        {(ColorType.Black,  ColorType.Blue),    ColorType.DarkBlue},
        {(ColorType.Black,  ColorType.Black),   ColorType.Black},
        {(ColorType.White,  ColorType.White),   ColorType.White}
    };

    private Dictionary<ColorType, HSV> hsvMap = new Dictionary<ColorType, HSV>()
    {
        { ColorType.Black           , new HSV(0, 0, 0) },
        { ColorType.Red             , new HSV(0, 100, 100) },
        { ColorType.Yellow          , new HSV(60, 100, 100) },
        { ColorType.Blue            , new HSV(240, 100, 100) },
        { ColorType.White           , new HSV(0, 0, 100) },

        { ColorType.Orange          , new HSV(30, 100, 100) },
        { ColorType.Cyan            , new HSV(180, 100, 100) },
        { ColorType.Magenta         , new HSV(300, 100, 75) },
        { ColorType.Gray            , new HSV(0, 0, 50) },

        { ColorType.DarkRed         , new HSV(0, 100, 50) },
        { ColorType.LightRed        , new HSV(0, 50, 100) },
        { ColorType.DarkYellow      , new HSV(60, 100, 50) },
        { ColorType.LightYellow     , new HSV(60, 50, 100) },
        { ColorType.DarkBlue        , new HSV(240, 100, 50) },
        { ColorType.LightBlue       , new HSV(240, 50, 100) },
    };

    public Color GetColor(ColorType colorType)
    {
        HSV hsv = hsvMap[colorType];
        return Color.HSVToRGB(hsv.h, hsv.s, hsv.v);
    }

    public ColorType MixColor(ColorType c1, ColorType c2)
    {
        ColorType mixColor = MixColors(c1, c2);
        StartCoroutine(BlendTwoColors(c1, mixColor, blendDuration));

        return mixColor;
    }
    // dictionary 기반 색 판정
    ColorType MixColors(ColorType color1, ColorType color2)
    {
        ColorType result = ColorType.None;

        if (colorMap.TryGetValue((color1, color2), out result))
        {
            return result;
        }

        if (colorMap.TryGetValue((color2, color1), out result))
        {
            return result;
        }

        return result;
    }

    public void SetFirstColor(ColorType color)
    {
        HSV hsv = GetHSV(color);

        result.color = Color.HSVToRGB(hsv.h, hsv.s, hsv.v);
    }

    public HSV GetHSV(ColorType color)
    {
        if (color >= ColorType.Count || color == ColorType.None) return new HSV(0f, 0f, 0f);

        return hsvMap[color];
    }

    IEnumerator BlendTwoColors(ColorType startColor, ColorType endColor, float duration)
    {
        HSV start = GetHSV(startColor);
        HSV end = GetHSV(endColor);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            //360 보정
            float deltaH = end.h - start.h;
            if (deltaH > 0.5f) deltaH -= 1f;
            if (deltaH < -0.5f) deltaH += 1f;

            float h = (start.h + deltaH * t) % 1f;
            float s = Mathf.Lerp(start.s, end.s, t);
            float v = Mathf.Lerp(start.v, end.v, t);

            result.color = Color.HSVToRGB(h, s, v);

            yield return null;
        }

        // 마지막 색 적용
        result.color = Color.HSVToRGB(end.h, end.s, end.v);
    }
}
