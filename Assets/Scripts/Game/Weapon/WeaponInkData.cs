using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct WeaponVisualData
{
    public WeaponManager.WeaponType weaponType;
    [Tooltip("공격 모션 이펙트")] public EffectVisualData attackEffect;
    [Tooltip("타격 성공 시 이펙트")] public EffectVisualData hitEffect;
}

[System.Serializable]
[CreateAssetMenu(menuName = "Weapon/WeaponInkData")]
public class WeaponInkData : ScriptableObject
{
    [Header("InkData")]
    public InkData inkData;
    public int durability;
    public int damage;

    [Header("Skill Logic")]
    public BaseSkillLogic skillLogic;

    [Header("Visual Effects per Weapon Type")]
    public List<WeaponVisualData> visualEffects;

    [Header("Passive Effect")]
    public PassiveEffectData passiveEffect;

    public WeaponVisualData GetVisualData(WeaponManager.WeaponType type)
    {
        if (visualEffects == null) return new WeaponVisualData();
        return visualEffects.Find(x => x.weaponType == type);
    }
}