using Spine.Unity;
using UnityEngine;

[ExecuteAlways]
public class MonsterColorChanger : MonoBehaviour
{
    [SerializeField] private ColorMixer.ColorType baseColorType = ColorMixer.ColorType.Black;
    [SerializeField, Range(0, 1)] private float saturation = 1f;
    [SerializeField, Range(0, 1)] private float brightness = 1f;

    [SerializeField] private SkeletonAnimation skeleton;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private ColorMixer.ColorType defaultColor = ColorMixer.ColorType.White;

    private ColorMixer.ColorType currentStatusColor = ColorMixer.ColorType.None;

    public void SetColorEnum(ColorMixer.ColorType colorType)
    {
        this.baseColorType = colorType;
        UpdateColor();
    }

    public void SetStatusColor(ColorMixer.ColorType colorType)
    {
        currentStatusColor = colorType;
        UpdateColor();
    }

    public void RemoveStatusColor()
    {
        currentStatusColor = ColorMixer.ColorType.None;
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (spriteRenderer != null) NoneSpine();
        else SetColor();
    }

    private Color GetTargetColor()
    {
        ColorMixer.ColorType targetType = (currentStatusColor != ColorMixer.ColorType.None)
                                        ? currentStatusColor
                                        : baseColorType;

        if (targetType == ColorMixer.ColorType.Black) targetType = defaultColor;

        ColorMixer.HSV hsv = ColorMixer.Instance.GetHSV(targetType);
        return Color.HSVToRGB(hsv.h, hsv.s * saturation, hsv.v * brightness);
    }

    private void NoneSpine()
    {
        spriteRenderer.color = GetTargetColor();
    }

    private void SetColor()
    {
        if (skeleton == null || skeleton.skeleton == null) return;

        Color newColor = GetTargetColor();

        skeleton.skeleton.R = newColor.r;
        skeleton.skeleton.G = newColor.g;
        skeleton.skeleton.B = newColor.b;
        skeleton.skeleton.A = (baseColorType == ColorMixer.ColorType.White) ? 1.5f : 1.0f;
    }
}