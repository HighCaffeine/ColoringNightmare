using UnityEngine;

[CreateAssetMenu(fileName = "PassiveEffectData", menuName = "Weapon/PassiveEffectData")]
public class PassiveEffectData : ScriptableObject
{
    public enum EffectType
    {
        None, Slow, Poison, Heal, HighDamage
    }

    [Header("Effect Type")]
    public EffectType effectType;

    [Header("Effect Stats")]
    public float effectValue1; // 둔화율 / 독 데미지 / 회복량
    public float effectValue2; // 지속 시간

    [Header("Visuals")]
    public EffectVisualData statusVisual; // 이펙트 

    public ColorMixer.ColorType statusColorType = ColorMixer.ColorType.None;
}