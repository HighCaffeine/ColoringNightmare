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

    [Header("Passive Effect")]
    public PassiveEffectData passiveEffect;
}
