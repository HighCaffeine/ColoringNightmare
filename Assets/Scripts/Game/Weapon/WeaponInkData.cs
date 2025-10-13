using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Weapon/WeaponInkData")]
public class WeaponInkData : ScriptableObject
{
    [Header("InkData")]
    public InkData inkData;
    public int durability;
    public int damage;
    public WeaponManager.WeaponType weaponType;
    public SkillData skillData;

    [Header("Passive Effect")]
    public PassiveEffectData passiveEffect;
}
