using UnityEngine;

[CreateAssetMenu(fileName = "BasicAttackSkill", menuName = "Skill/BasicAttackSkill")]
public class BasicAttackSkill : BaseSkillLogic
{
    public override void ActivateSkill(SkillController controller, Character character, Weapon weapon) { }
    public override void OnAnimationHit(SkillController controller, Weapon weapon)
    {
        base.OnAnimationHit(controller, weapon);
    }
}