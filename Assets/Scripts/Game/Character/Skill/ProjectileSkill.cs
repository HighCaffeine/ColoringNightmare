using UnityEngine;
using System;

[CreateAssetMenu(fileName = "ProjectileSkill", menuName = "Skill/ProjectileSkill")]
public class ProjectileSkill : BaseSkillLogic
{
    [Serializable]
    public class ProjectileParams
    {
        [Tooltip("투사체의 이동 속도")]
        public float speed = 10f;
        [Tooltip("투사체의 생명 시간 (거리 대신)")]
        public float lifeTime = 2f;
        [Tooltip("투사체의 크기 (Scale)")]
        public float size = 1f;
        [Tooltip("관통 횟수 (0이면 관통 없음)")]
        public int piercingCount = 1;
    }
    public ProjectileParams projectileParams = new ProjectileParams();

    public override void ActivateSkill(SkillController controller, Character character)
    {
        GameObject projectilePrefab = controller.projectilePrefab;
        Transform effectPivot = controller.GetEffectPivot();

        if (projectilePrefab == null || character == null || effectPivot == null) return;

        WeaponInkData inkData = controller.GetWeaponController()?.GetEquippedWeapon()?.GetInkData();
        if (inkData == null)
        {
            Debug.LogWarning("ProjectileSkill: WeaponInkData null");
        }

        float currentScaleX = character.skeleton.skeleton.ScaleX;
        Vector3 forward = character.transform.right * Mathf.Sign(currentScaleX) * -1;

        Vector3 dir = forward;
        GameObject proj = Instantiate(projectilePrefab, effectPivot.position, Quaternion.identity);

        Projectile projComponent = proj.GetComponent<Projectile>();
        if (projComponent != null)
        {
            projComponent.InitFromSkill(this, inkData, dir);
        }

        controller.PlayVisualEffect(visualData, this);
        controller.GetWeaponController()?.SubDurability();
    }

    public override void OnAnimationHit(SkillController controller)
    {
    }

    public override void ApplyColorModifier(ColorMixer.ColorType c1, ColorMixer.ColorType c2)
    {
        base.ApplyColorModifier(c1, c2);

        if (c1 == ColorMixer.ColorType.Black || c2 == ColorMixer.ColorType.Black)
        {
            projectileParams.size *= 0.8f;
        }
        else if (c1 == ColorMixer.ColorType.White || c2 == ColorMixer.ColorType.White)
        {
            projectileParams.size *= 2.0f;
        }
    }
}