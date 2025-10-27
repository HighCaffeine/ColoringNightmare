using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillController : MonoBehaviour
{
    [Header("외부 참조")]
    public GameObject projectilePrefab;
    [SerializeField] private Transform effectPivot;
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private EffectController effectController;
    private Character character;
    private SkillData currentSkill;
    private int hitCountForDoubleAttack = 0; // 2연타 횟수를 세는 카운터

    void Awake()
    {
        character = GetComponent<Character>();
    }

    // WeaponController가 무기를 장착/해제할 때 호출하여 현재 스킬 정보를 업데이트
    public void SetCurrentSkill(SkillData skillData)
    {
        this.currentSkill = skillData;
    }

    // PlayerController가 공격을 시작할 때 호출
    public void UseSkill()
    {
        // '더블 어택' 스킬이 시작될 때마다 타격 횟수 카운터를 0으로 초기화
        if (currentSkill != null && currentSkill.skillType == SkillType.DoubleHit)
        {
            hitCountForDoubleAttack = 0;
        }

        // 투사체 스킬일 경우, 애니메이션 시작과 동시에 발사 로직 실행
        if (currentSkill != null && currentSkill.skillType == SkillType.Projectile)
        {
            ExecuteProjectileSkill(currentSkill);
        }
    }

    public void OnAnimationHit()
    {
        if (currentSkill == null) return;

        if (currentSkill.skillType == SkillType.DoubleHit)
        {
            if (hitCountForDoubleAttack == 0)
            {
                effectController.PlayEffect(currentSkill.visualData);
            }
            else
            {
                effectController.PlayEffect(currentSkill.secondHitVisualData ?? currentSkill.visualData);
            }
            hitCountForDoubleAttack++;
        }
        else
        {
            effectController.PlayEffect(currentSkill.visualData);
        }

        weaponController.SubDurability();

        if (currentSkill.skillType == SkillType.BasicAttack || currentSkill.skillType == SkillType.DoubleHit)
        {
            weaponController.ActivateHitboxForDuration(0.15f);
        }
    }

    private void ExecuteProjectileSkill(SkillData data)
    {
        if (projectilePrefab == null) return;

        float currentScaleX = character.skeleton.skeleton.ScaleX;
        Vector3 forward = transform.right * Mathf.Sign(currentScaleX) * -1;
        float[] angles = { 0f /*15f, -15f*/ };

        foreach (float angle in angles)
        {
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Vector3 dir = rot * forward;
            GameObject proj = Instantiate(projectilePrefab, effectPivot.position, Quaternion.identity);

            Projectile projComponent = proj.GetComponent<Projectile>();
            if (projComponent != null)
            {
                projComponent.Init(data, dir);
            }
        }
    }
}