using UnityEngine;

[CreateAssetMenu(fileName = "PassiveEffectData", menuName = "Weapon/PassiveEffectData")]
public class PassiveEffectData : ScriptableObject
{
    public enum EffectType
    {
        None,
        Slow,
        Projectile,
        DoubleStrike
    }

    [Header("Effect Type")]
    public EffectType effectType;

    [Header("Effect Stats")]
    // 모든 효과에 필요한 변수들을 여기에 정의하고, 필요 없는 것은 사용하지 않음
    [Tooltip("둔화율 또는 투사체 속도 등")] public float effectValue1; // 둔화율 또는 투사체 속도 등
    [Tooltip("둔화 지속시간 또는 투사체 데미지 등")] public float effectValue2; // 둔화 지속시간 또는 투사체 데미지 등
    [Tooltip("투사체 프리팹 등")] public GameObject effectPrefab; // 투사체 프리팹 등
}