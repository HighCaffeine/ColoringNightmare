using UnityEngine;

/// <summary>
/// [파파] 스킬에서 사용되는 몬스터 둔화 효과.
/// </summary>
public class SlowEffect : IStatusEffect
{
    public float Duration { get; private set; }
    public float SlowRate { get; private set; } // 둔화율 (0.1 = 10% 둔화)
    public bool IsFinished => remainingTime <= 0;

    private float remainingTime;

    public SlowEffect(float slowRate, float duration)
    {
        SlowRate = slowRate;
        Duration = duration;
        remainingTime = duration;
    }

    public void Apply(Character target)
    {
        // 둔화 효과가 시작될 때 필요한 시각 효과 등을 여기서 실행할 수 있습니다.
    }

    public void UpdateEffect(float deltaTime)
    {
        remainingTime -= deltaTime;
    }

    public void Remove(Character target)
    {
        // 둔화 효과가 끝났을 때 필요한 정리 작업(예: 시각 효과 제거)을 여기서 실행합니다.
        // StatusEffectManager가 자동으로 SpeedMultiplier를 1.0f로 되돌립니다.
    }
}
