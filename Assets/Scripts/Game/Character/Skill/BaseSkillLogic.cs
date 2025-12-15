using UnityEngine;

public abstract class BaseSkillLogic : ScriptableObject
{
    public ColorMixer.ColorType colorType;
    public int baseDamage = 1;
    public float cooldown = 0.5f;

    public abstract void ActivateSkill(SkillController controller, Character character, Weapon weapon);

    public virtual void OnAnimationHit(SkillController controller, Weapon weapon)
    {
        if (weapon != null)
        {
            controller.PlayVisualEffect(weapon.GetAttackEffect(), this);
        }
        controller.GetWeaponController()?.SubDurability();
        controller.EnableHitbox();
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