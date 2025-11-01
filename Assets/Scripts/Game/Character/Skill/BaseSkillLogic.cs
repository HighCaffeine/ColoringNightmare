using UnityEngine;

// 모든 스킬 ScriptableObject의 기본 클래스
public abstract class BaseSkillLogic : ScriptableObject
{
    [Header("공통 스킬 정보")]
    public ColorMixer.ColorType colorType;
    public int baseDamage = 1;
    public float cooldown = 0.5f;

    [Header("공통 시각 효과")]
    [Tooltip("스킬을 시전할 때 시전자 위치에 표시할 이펙트")]
    public EffectVisualData visualData; // 스킬에 사용할 기본 이펙트

    [Header("피격 효과")]
    [Tooltip("이 스킬이 적에게 적중했을 때 적 위치에 표시할 이펙트")]
    public EffectVisualData hitEffectVisualData;

    public abstract void ActivateSkill(SkillController controller, Character character);

    // Spine 애니메이션 이벤트에서 호출
    public virtual void OnAnimationHit(SkillController controller)
    {
        controller.PlayVisualEffect(visualData);
        controller.GetWeaponController()?.SubDurability();
    }

    public virtual void ApplyColorModifier(ColorMixer.ColorType c1, ColorMixer.ColorType c2)
    {
        // 1. 검은색과 조합 시 (약화)
        if (c1 == ColorMixer.ColorType.Black || c2 == ColorMixer.ColorType.Black)
        {
            // 데미지 감소 (최소 1 보장)
            baseDamage = Mathf.Max(1, baseDamage - 1);
        }
        // 2. 흰색과 조합 시 (강화)
        else if (c1 == ColorMixer.ColorType.White || c2 == ColorMixer.ColorType.White)
        {
            // 데미지 증가
            baseDamage = baseDamage + 1;
        }

        // 3. 그 외 (빨+빨, 빨+노랑 등)는 기본 스탯 유지
    }
}