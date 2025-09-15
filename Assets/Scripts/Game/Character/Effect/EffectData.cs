using UnityEngine;


[CreateAssetMenu(fileName = "EffectData", menuName = "Effect/EffectData")]
public class EffectData : ScriptableObject
{
    public Sprite[] sprites;
    public ColorMixer.ColorType colorType;
    public EffectType effectType;
    public float speed = 1f;
}

