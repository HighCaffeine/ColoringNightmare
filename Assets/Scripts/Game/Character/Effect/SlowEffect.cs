using UnityEngine;

public class SlowEffect : IStatusEffect
{
    public float Duration { get; private set; }
    public float SlowRate { get; private set; }
    public bool IsFinished => remainingTime <= 0;

    private float remainingTime;
    private ColorMixer.ColorType statusColor;

    public SlowEffect(float slowRate, float duration, ColorMixer.ColorType colorType)
    {
        SlowRate = slowRate;
        Duration = duration;
        remainingTime = duration;
        this.statusColor = colorType;
    }

    public void Apply(Character target)
    {
        target.GetComponent<MonsterColorChanger>()?.SetStatusColor(statusColor);
    }

    public void UpdateEffect(float deltaTime)
    {
        remainingTime -= deltaTime;
    }

    public void Remove(Character target)
    {
        target.GetComponent<MonsterColorChanger>()?.RemoveStatusColor();
    }
}