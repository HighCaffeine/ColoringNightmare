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

    // SkillData -> BaseSkillLogic
    public void SetCurrentSkill(BaseSkillLogic skillData)
    {
        this.currentSkill = skillData;
    }

    // PlayerController가 공격을 시작할 때 호출
    public void UseSkill()
    {
        if (currentSkill == null) return;

        currentSkill.ActivateSkill(this, character);
    }

    // Spine 애니메이션 이벤트에서 호출
    public void OnAnimationHit()
    {
        if (currentSkill == null) return;

        currentSkill.OnAnimationHit(this);
    }

    public WeaponController GetWeaponController()
    {
        return weaponController;
    }

    public void PlayVisualEffect(EffectVisualData visualData)
    {
        if (effectController != null)
        {
            effectController.PlayEffect(visualData);
        }
    }

    public Transform GetEffectPivot()
    {
        return effectPivot;
    }
}