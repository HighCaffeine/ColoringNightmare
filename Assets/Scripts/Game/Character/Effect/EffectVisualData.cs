using UnityEngine;

public enum EffectVisualType { SpriteAnimation, ParticleSystem }

[CreateAssetMenu(fileName = "SkillVisualData", menuName = "Skill/SkillVisualData")]
public class EffectVisualData : ScriptableObject
{
    [Header("시각 효과 설정")]
    public ColorMixer.ColorType colorType;
    public EffectVisualType visualType = EffectVisualType.SpriteAnimation;

    [Header("Animation")]
    public Sprite[] sprites;
    public float animationSpeed = 1f;

    public Transform prefab;
}