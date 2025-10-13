using UnityEngine;


[CreateAssetMenu(fileName = "EffectData", menuName = "Effect/EffectData")]
public class EffectData : ScriptableObject
{
    public ColorMixer.ColorType colorType;
    public PassiveEffectData.EffectType effectType;
    public float speed = 1f;
}

