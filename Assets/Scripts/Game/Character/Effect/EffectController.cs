using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public interface IStatusEffect
{
    float Duration { get; }
    void Apply(Character target);
    void UpdateEffect(float deltaTime);
    void Remove(Character target);
    bool IsFinished { get; }
}

public class EffectController : MonoBehaviour
{
    [Header("Effect Prefab")]
    [SerializeField] private GameObject effectPrefab; // EffectPlayer 프리팹
    [SerializeField] private Transform effectPivot;

    [Header("Dependencies")]
    [SerializeField] private Character character;

    public void PlayEffect(EffectVisualData specificVisualData, BaseSkillLogic skillLogic, WeaponInkData inkData, Vector3 weaponScale)
    {
        if (specificVisualData == null)
        {
            Debug.LogWarning("재생할 VisualData가 없음.");
            return;
        }

        bool isFacingLeft = true;
        Vector3 newWeaponScale = weaponScale / 4.0f;
        if (character != null && character.skeleton != null)
        {
            isFacingLeft = character.skeleton.skeleton.ScaleX > 0;
        }
        else if (character != null && character.IsExistsSprite())
        {
            isFacingLeft = character.FlipX();
        }

        if (specificVisualData.visualType == EffectVisualType.SpriteAnimation)
        {
            if (effectPrefab == null) return;

            GameObject effectInstance = Instantiate(effectPrefab, effectPivot.position, effectPivot.rotation, null);
            var effectPlayer = effectInstance.GetComponent<EffectPlayer>();

            if (effectPlayer != null)
            {
                effectPlayer.Play(specificVisualData, skillLogic, inkData, newWeaponScale, isFacingLeft);
            }
        }
        else if (specificVisualData.visualType == EffectVisualType.ParticleSystem)
        {
            if (specificVisualData.prefab == null) return;

            ParticleSystem particleSystem = Instantiate(specificVisualData.prefab, effectPivot.position, specificVisualData.prefab.transform.rotation, null).gameObject.GetComponent<ParticleSystem>();

            if (particleSystem != null)
            {
                float scaleMultiplier = Mathf.Max(newWeaponScale.x, newWeaponScale.y);
                var mainModule = particleSystem.main;

                mainModule.startSizeMultiplier *= scaleMultiplier;
                var childrenParticles = particleSystem.GetComponentsInChildren<ParticleSystem>();

                foreach (var childPS in childrenParticles)
                {
                    if (childPS == particleSystem) continue;
                    var childMain = childPS.main;
                    childMain.startSizeMultiplier *= scaleMultiplier;
                }

                CircleCollider2D particleCollider = particleSystem.GetComponent<CircleCollider2D>();
                if (particleCollider != null)
                {
                    particleCollider.radius *= scaleMultiplier;
                }

                float zDirection = isFacingLeft ? 0 : -180f;
                particleSystem.transform.rotation *= Quaternion.Euler(0, 0, zDirection);

                particleSystem.Play();
                //var mainModule = particleSystem.main;
                Destroy(particleSystem.gameObject, mainModule.duration + mainModule.startLifetime.constantMax);
            }
        }
    }

    public void PlayHitEffectAt(Vector3 spawnPosition, EffectVisualData specificVisualData, bool isFacingLeft)
    {
        if (specificVisualData == null) return;

        if (specificVisualData.visualType == EffectVisualType.SpriteAnimation)
        {
            if (effectPrefab == null) return;

            GameObject effectInstance = Instantiate(effectPrefab, spawnPosition, effectPrefab.transform.rotation, null);

            var effectPlayer = effectInstance.GetComponent<EffectPlayer>();

            if (effectPlayer != null)
            {
                effectPlayer.Play(specificVisualData, null, null, Vector3.one, isFacingLeft);
            }
        }
        else if (specificVisualData.visualType == EffectVisualType.ParticleSystem)
        {
            if (specificVisualData.prefab == null) return;

            ParticleSystem particleSystem = Instantiate(specificVisualData.prefab, spawnPosition, specificVisualData.prefab.transform.rotation, null).GetComponent<ParticleSystem>();

            if (particleSystem != null)
            {
                float zDirection = isFacingLeft ? 0 : -180f;
                particleSystem.transform.rotation *= Quaternion.Euler(0, 0, zDirection);

                particleSystem.Play();
                var mainModule = particleSystem.main;
                Destroy(particleSystem.gameObject, mainModule.duration + mainModule.startLifetime.constantMax);
            }
        }
    }
    public void NoneWeapon(EffectVisualData effectVisualData)
    {
        if (effectVisualData == null) return;
        PlayEffect(effectVisualData, null, null, Vector3.one);
    }

    public void Flip(bool isRight)
    {
        effectPivot.parent.transform.localScale = new Vector3(isRight ? -1 : 1, 1, 1);
    }
}