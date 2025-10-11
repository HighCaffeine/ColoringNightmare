using Spine.Unity;
using UnityEngine;

[ExecuteAlways]
public class MonsterColorChanger : MonoBehaviour
{
    [SerializeField] private ColorMixer.ColorType baseColorType = ColorMixer.ColorType.Black;
    [SerializeField, Range(0, 1)] private float saturation = 1f;
    [SerializeField, Range(0, 1)] private float brightness = 1f;

    [SerializeField] private SkeletonAnimation skeleton;

    private ColorMixer.ColorType defaultColor = ColorMixer.ColorType.White;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (skeleton == null) return;
        SetColor();
    }
#endif

    private void SetColor()
    {
        if (skeleton == null) return;
        ColorMixer.HSV baseHsv;

        if (baseColorType == ColorMixer.ColorType.Black)
        {
            baseHsv = ColorMixer.Instance.GetHSV(defaultColor);
        }
        else
        {
            baseHsv = ColorMixer.Instance.GetHSV(baseColorType);
        }

        float finalH = baseHsv.h;
        float finalS = baseHsv.s * saturation;
        float finalV = baseHsv.v * brightness;
        skeleton.skeleton.A = 1.0f;

        if (baseColorType == ColorMixer.ColorType.White)
        {
            skeleton.skeleton.A = 1.5f;
        }

        Color newColor = Color.HSVToRGB(finalH, finalS, finalV);

        skeleton.skeleton.R = newColor.r;
        skeleton.skeleton.G = newColor.g;
        skeleton.skeleton.B = newColor.b;
    }
}