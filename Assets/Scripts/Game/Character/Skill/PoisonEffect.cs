using UnityEngine;

public class PoisonEffect : IStatusEffect
{
    public float Duration { get; private set; }
    public bool IsFinished => remainingTime <= 0;

    private float remainingTime;
    private float damageInterval = 1.0f;
    private float damageTimer = 0f;
    private int damagePerTick;
    private ColorMixer.ColorType statusColor;

    public PoisonEffect(int damage, float duration, ColorMixer.ColorType colorType)
    {
        this.damagePerTick = damage;
        this.Duration = duration;
        this.remainingTime = duration;
        this.statusColor = colorType;
    }

    public void Apply(Character target)
    {
        target.GetComponent<MonsterColorChanger>()?.SetStatusColor(statusColor);
    }

    public void UpdateEffect(float deltaTime)
    {
        remainingTime -= deltaTime;
        damageTimer += deltaTime;
    }

    public void ProcessDotDamage(Character target)
    {
        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            target.TakeDamage(damagePerTick);
        }
    }

    public void Remove(Character target)
    {
        target.GetComponent<MonsterColorChanger>()?.RemoveStatusColor();
    }
}