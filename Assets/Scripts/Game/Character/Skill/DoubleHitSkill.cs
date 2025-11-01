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
            // --- 첫 번째 타격 ---
            // 1. 기본 이펙트 (visualData) 재생
            controller.PlayVisualEffect(visualData);

            // 2. 내구도 소모
            controller.GetWeaponController()?.SubDurability();

            // 3. 히트박스 활성화
            controller.GetWeaponController()?.ActivateHitboxForDuration(hitboxDuration);
        }
        else
        {
            // --- 두 번째 타격 ---
            // 1. 두 번째 이펙트 (secondHitVisualData) 재생
            //    (없으면 기본 이펙트로 대체)
            var effectToPlay = secondHitVisualData != null ? secondHitVisualData : visualData;
            controller.PlayVisualEffect(effectToPlay);

            // 2. 내구도 소모
            controller.GetWeaponController()?.SubDurability();

            // 3. 히트박스 활성화
            controller.GetWeaponController()?.ActivateHitboxForDuration(hitboxDuration);
        }

        // 타격 횟수 증가
        hitCount++;
    }
}