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
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private Transform effectPivot;

    [Header("Dependencies")]
    [SerializeField] private Character character;

    public void PlayEffect(EffectVisualData specificVisualData)
    {
        if (specificVisualData == null)
        {
            Debug.LogWarning("재생할 VisualData가 없음.");
            return;
        }

        bool isFacingRight = true;
        if (character != null && character.skeleton != null)
        {
            isFacingRight = character.skeleton.skeleton.ScaleX > 0;
        }
        effectPivot.parent.localScale = new Vector3(isFacingRight ? 1 : -1, 1, 1);

        if (specificVisualData.visualType == EffectVisualType.SpriteAnimation)
        {

            if (effectPrefab == null) return;
            GameObject effectInstance = Instantiate(effectPrefab, effectPivot.position, Quaternion.identity, effectPivot);
            var effectPlayer = effectInstance.GetComponent<EffectPlayer>();


            if (effectPlayer != null)
            {
                effectPlayer.Play(isFacingRight, specificVisualData);
            }
        }
        else if (specificVisualData.visualType == EffectVisualType.ParticleSystem)
        {
            if (specificVisualData.prefab == null) return;
            ParticleSystem particleSystem = Instantiate(specificVisualData.prefab, effectPivot.position, Quaternion.identity, effectPivot).GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                var shape = particleSystem.shape;
                shape.rotation = isFacingRight ? new Vector3(shape.rotation.x, 90, shape.rotation.z) : new Vector3(shape.rotation.x, -90, shape.rotation.z);

                particleSystem.Play();
                var mainModule = particleSystem.main;
                Destroy(particleSystem.gameObject, mainModule.duration + mainModule.startLifetime.constantMax);
            }
        }
    }

    public void PlayHitEffectAt(Vector3 spawnPosition, EffectVisualData specificVisualData, bool isFacingLeft)
    {
        if (specificVisualData == null)
        {
            return;
        }

        if (specificVisualData.visualType == EffectVisualType.SpriteAnimation)
        {
            if (effectPrefab == null) return;

            GameObject effectInstance = Instantiate(effectPrefab, spawnPosition, Quaternion.identity, null);
            var effectPlayer = effectInstance.GetComponent<EffectPlayer>();

            if (effectPlayer != null)
            {
                effectPlayer.Play(isFacingLeft, specificVisualData);
            }
        }
        else if (specificVisualData.visualType == EffectVisualType.ParticleSystem)
        {
            if (specificVisualData.prefab == null) return;

            ParticleSystem particleSystem = Instantiate(specificVisualData.prefab, spawnPosition, Quaternion.identity, null).GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                var shape = particleSystem.shape;
                shape.rotation = isFacingLeft ? new Vector3(0, 90, 0) : new Vector3(0, -90, 0);

                particleSystem.Play();
                var mainModule = particleSystem.main;
                Destroy(particleSystem.gameObject, mainModule.duration + mainModule.startLifetime.constantMax);
            }
        }
    }

    public void NoneWeapon(EffectVisualData effectVisualData)
    {
        if (effectVisualData == null) return;
        PlayEffect(effectVisualData);
    }
}