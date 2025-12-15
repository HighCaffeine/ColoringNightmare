using UnityEngine;
using System;

[CreateAssetMenu(fileName = "ProjectileSkill", menuName = "Skill/ProjectileSkill")]
public class ProjectileSkill : BaseSkillLogic
{
    [Serializable]
    public class ProjectileParams
    {
        public float speed = 10f;
        public float lifeTime = 2f;
        public float size = 1f;
        public int piercingCount = 1;
    }
    public ProjectileParams projectileParams = new ProjectileParams();

    public override void ActivateSkill(SkillController controller, Character character, Weapon weapon)
    {
        WeaponInkData inkData = weapon.GetInkData();
        GameObject projectilePrefab = inkData.projectilePrefab;
        Transform effectPivot = controller.GetEffectPivot();

        if (projectilePrefab == null || character == null || effectPivot == null) return;

        float currentScaleX = character.skeleton.skeleton.ScaleX;
        Vector3 forward = character.transform.right * Mathf.Sign(currentScaleX) * -1;

        GameObject proj = Instantiate(projectilePrefab, effectPivot.position, Quaternion.identity);
        Projectile projComponent = proj.GetComponent<Projectile>();

        if (projComponent != null)
        {
            projComponent.InitFromSkill(this, weapon.GetInkData(), forward, character);
        }

        if (weapon != null) controller.PlayVisualEffect(weapon.GetAttackEffect(), this);
        controller.GetWeaponController()?.SubDurability();
    }

    public override void OnAnimationHit(SkillController controller, Weapon weapon) { }

    public override void ApplyColorModifier(ColorMixer.ColorType c1, ColorMixer.ColorType c2)
    {
        base.ApplyColorModifier(c1, c2);
        if (c1 == ColorMixer.ColorType.Black || c2 == ColorMixer.ColorType.Black) projectileParams.size *= 0.8f;
        else if (c1 == ColorMixer.ColorType.White || c2 == ColorMixer.ColorType.White) projectileParams.size *= 2.0f;
    }
}