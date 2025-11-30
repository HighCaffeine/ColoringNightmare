using UnityEngine;

[CreateAssetMenu(fileName = "DoubleHitSkill", menuName = "Skill/DoubleHitSkill")]
public class DoubleHitSkill : BaseSkillLogic
{
    private int hitCount = 0;

    public override void ActivateSkill(SkillController controller, Character character, Weapon weapon)
    {
        hitCount = 0;
    }
    public override void OnAnimationHit(SkillController controller, Weapon weapon)
    {
        if (hitCount == 0 && weapon != null)
        {
            controller.PlayVisualEffect(weapon.GetAttackEffect(), this);
            controller.GetWeaponController()?.SubDurability();
        }
        controller.EnableHitbox();
        hitCount++;
    }
}