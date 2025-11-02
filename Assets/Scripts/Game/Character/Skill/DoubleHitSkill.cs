using UnityEngine;

[CreateAssetMenu(fileName = "DoubleHitSkill", menuName = "Skill/DoubleHitSkill")]
public class DoubleHitSkill : BaseSkillLogic
{
    [Header("2연타 전용 설정")]
    [Tooltip("2연타 공격의 두 번째 타격에 사용할 시각 효과")]
    public EffectVisualData secondHitVisualData;

    [Tooltip("히트박스 활성화 시간")]
    public float hitboxDuration = 0.15f;

    private int hitCount = 0;

    public override void ActivateSkill(SkillController controller, Character character)
    {
        hitCount = 0;
    }

    public override void OnAnimationHit(SkillController controller)
    {
        if (hitCount == 0)
        {
            controller.PlayVisualEffect(visualData, this);
            controller.GetWeaponController()?.SubDurability();
        }
        else
        {
            var effectToPlay = secondHitVisualData != null ? secondHitVisualData : visualData;
            controller.PlayVisualEffect(effectToPlay, this);
            controller.GetWeaponController()?.SubDurability();
        }

        hitCount++;
    }
}