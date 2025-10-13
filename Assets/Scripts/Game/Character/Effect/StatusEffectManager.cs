using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 캐릭터에 부착되어 현재 적용 중인 모든 상태 이상을 관리합니다.
/// </summary>
public class StatusEffectManager : MonoBehaviour
{
    private Character character;
    private readonly List<IStatusEffect> activeEffects = new List<IStatusEffect>();

    // 캐릭터의 최종 이동 속도에 영향을 미치는 계수를 저장합니다.
    // 1.0f: 정상 속도, 0.5f: 50% 둔화
    public float currentSpeedMultiplier = 1.0f;

    void Awake()
    {
        character = GetComponent<Character>();
        if (character == null)
        {
            Debug.LogError("StatusEffectManager requires a Character component on the same GameObject.");
        }
    }

    void Update()
    {
        // 종료된 효과를 저장할 리스트
        List<IStatusEffect> finishedEffects = new List<IStatusEffect>();

        currentSpeedMultiplier = 1.0f;

        // 모든 활성 효과 업데이트
        foreach (var effect in activeEffects)
        {
            effect.UpdateEffect(Time.deltaTime);
            if (effect.IsFinished)
            {
                finishedEffects.Add(effect);
            }

            // 둔화 효과가 있다면 SpeedMultiplier 업데이트 (가장 큰 둔화 효과를 적용한다고 가정)
            if (effect is SlowEffect slowEffect)
            {
                currentSpeedMultiplier = Mathf.Min(currentSpeedMultiplier, 1f - slowEffect.SlowRate);
            }
        }

        // 종료된 효과 제거
        foreach (var effect in finishedEffects)
        {
            effect.Remove(character);
            activeEffects.Remove(effect);
        }
    }

    public void ApplyEffect(IStatusEffect effect)
    {
        effect.Apply(character);
        activeEffects.Add(effect);
        Debug.Log($"{character.name}에게 {effect.GetType().Name} 효과 적용됨. Duration: {effect.Duration}");
    }

    public float GetSpeedMultiplier()
    {
        return currentSpeedMultiplier;
    }
}
