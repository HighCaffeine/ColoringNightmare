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
    public EffectVisualData visualData;

    [Header("피격 효과")]
    [Tooltip("이 스킬이 적에게 적중했을 때 적 위치에 표시할 이펙트")]
    public EffectVisualData hitEffectVisualData;

    public abstract void ActivateSkill(SkillController controller, Character character);

    public virtual void OnAnimationHit(SkillController controller)
    {
        controller.PlayVisualEffect(visualData, this);
        controller.GetWeaponController()?.SubDurability();
    }

    public virtual void ApplyColorModifier(ColorMixer.ColorType c1, ColorMixer.ColorType c2)
    {
        if (c1 == ColorMixer.ColorType.Black || c2 == ColorMixer.ColorType.Black)
        {
            baseDamage = Mathf.Max(1, baseDamage - 1);
        }
        else if (c1 == ColorMixer.ColorType.White || c2 == ColorMixer.ColorType.White)
        {
            baseDamage = baseDamage + 1;
        }
    }
}