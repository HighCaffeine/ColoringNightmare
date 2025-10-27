using UnityEngine;
using System.Collections;

public interface IStatusEffect
{
    float Duration { get; }
    void Apply(Character target);
    void UpdateEffect(float deltaTime);
    void Remove(Character target);
    bool IsFinished { get; }
}

public class EffectController : GenericSingleton<EffectController>
{
    [Header("Effect Prefab")]
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private Transform effectPivot;

    [Header("Dependencies")]
    [SerializeField] private WeaponController playerWeaponController;

    public void PlayEffect()
    {
        if (effectPrefab == null || playerWeaponController == null)
        {
            return;
        }

        SkillData currentSkillData = playerWeaponController.GetEquippedWeaponSkillData();
        if (currentSkillData == null || currentSkillData.visualData == null)
        {
            return;
        }

        EffectVisualData visualData = currentSkillData.visualData;

        bool isFacingRight = effectPivot.parent.transform.localScale.x > 0;

        if (visualData.visualType == EffectVisualType.SpriteAnimation)
        {
            GameObject effectInstance = Instantiate(effectPrefab, effectPivot.position, Quaternion.identity, effectPivot);
            var effectPlayer = effectInstance.GetComponent<EffectPlayer>();
            if (effectPlayer != null)
            {
                effectPlayer.Play(isFacingRight, visualData);
            }
        }
        else if (visualData.visualType == EffectVisualType.ParticleSystem)
        {
            if (visualData.prefab == null) return;

            ParticleSystem particleSystem = Instantiate(visualData.prefab, effectPivot.position, Quaternion.identity, effectPivot).GetComponent<ParticleSystem>();

            if (particleSystem != null)
            {
                particleSystem.Play();
                var mainModule = particleSystem.main;
                Destroy(particleSystem.gameObject, mainModule.duration + mainModule.startLifetime.constantMax);
            }
        }
    }

    public void NoneWeapon(EffectVisualData effectVisualData)
    {
        if (effectPrefab == null || effectVisualData == null)
        {
            return;
        }

        bool isFacingRight = effectPivot.parent.transform.localScale.x > 0;
        if (effectVisualData.visualType == EffectVisualType.SpriteAnimation)
        {
            GameObject effectInstance = Instantiate(effectPrefab, effectPivot.position, Quaternion.identity, effectPivot);
            var effectPlayer = effectInstance.GetComponent<EffectPlayer>();
            if (effectPlayer != null)
            {
                effectPlayer.Play(isFacingRight, effectVisualData);
            }
        }
    }
}