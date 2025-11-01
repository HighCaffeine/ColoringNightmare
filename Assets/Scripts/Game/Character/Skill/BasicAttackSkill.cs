using UnityEngine;

[CreateAssetMenu(fileName = "BasicAttackSkill", menuName = "Skill/BasicAttackSkill")]
public class BasicAttackSkill : BaseSkillLogic
{
    [Header("기본 공격 설정")]
    [Tooltip("히트박스 활성화 시간")]
    public float hitboxDuration = 0.15f;

    public override void ActivateSkill(SkillController controller, Character character)
    {
    }

    public override void OnAnimationHit(SkillController controller)
    {
        base.OnAnimationHit(controller);
        controller.GetWeaponController()?.ActivateHitboxForDuration(hitboxDuration);
    }
}