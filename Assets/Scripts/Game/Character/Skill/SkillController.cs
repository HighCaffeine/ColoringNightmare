using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillController : MonoBehaviour
{
    [Header("외부 참조")]
    public GameObject projectilePrefab; // ProjectileSkill이 사용할 프리팹
    [SerializeField] private Transform effectPivot;
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private EffectController effectController;
    private Character character;

    private BaseSkillLogic currentSkill;

    void Awake()
    {
        character = GetComponent<Character>();
    }

    public void SetCurrentSkill(BaseSkillLogic skillData)
    {
        this.currentSkill = skillData;
    }

    public void UseSkill()
    {
        if (currentSkill == null) return;
        currentSkill.ActivateSkill(this, character);
    }

    public void OnAnimationHit()
    {
        if (currentSkill == null) return;
        currentSkill.OnAnimationHit(this);
    }

    public WeaponController GetWeaponController()
    {
        return weaponController;
    }

    public void PlayVisualEffect(EffectVisualData visualData, BaseSkillLogic skillLogic)
    {
        if (effectController != null)
        {
            WeaponInkData inkData = GetWeaponController()?.GetEquippedWeapon()?.GetInkData();
            Weapon currentWeapon = GetWeaponController()?.GetEquippedWeapon();
            Vector3 weaponScale = (currentWeapon != null) ? currentWeapon.transform.localScale : Vector3.one;

            effectController.PlayEffect(visualData, skillLogic, inkData, weaponScale);
        }
    }

    public Transform GetEffectPivot()
    {
        return effectPivot;
    }
}