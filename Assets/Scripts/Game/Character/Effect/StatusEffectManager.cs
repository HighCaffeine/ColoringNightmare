using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StatusEffectManager : MonoBehaviour
{
    private Character character;
    private readonly List<IStatusEffect> activeEffects = new List<IStatusEffect>();

    public float currentSpeedMultiplier = 1.0f;

    void Awake()
    {
        character = GetComponent<Character>();
    }

    void Update()
    {
        List<IStatusEffect> finishedEffects = new List<IStatusEffect>();

        currentSpeedMultiplier = 1.0f;

        foreach (var effect in activeEffects)
        {
            effect.UpdateEffect(Time.deltaTime);

            if (effect is PoisonEffect poison)
            {
                poison.ProcessDotDamage(character);
            }

            if (effect is SlowEffect slowEffect)
            {
                currentSpeedMultiplier = Mathf.Min(currentSpeedMultiplier, 1f - slowEffect.SlowRate);
            }

            if (effect.IsFinished)
            {
                finishedEffects.Add(effect);
            }
        }

        foreach (var effect in finishedEffects)
        {
            effect.Remove(character);
            activeEffects.Remove(effect);
        }
    }

    public void ClearEffects()
    {
        foreach (var effect in activeEffects)
        {
            effect.Remove(character);
        }

        activeEffects.Clear();
        currentSpeedMultiplier = 1.0f;
    }

    public void ApplyEffect(IStatusEffect effect)
    {
        effect.Apply(character);
        activeEffects.Add(effect);
    }

    public float GetSpeedMultiplier()
    {
        return currentSpeedMultiplier;
    }
}