using UnityEngine;
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
    [SerializeField] private GameObject effectPrefab; // EffectPlayer 컴포넌트가 붙은 프리팹
    [SerializeField] private Transform effectPivot; // 이펙트가 출력될 위치/방향 기준
    private EffectVisualData visualData;

    public void SetVisualData(EffectVisualData visualData) { this.visualData = visualData; }

    public void PlayEffect()
    {
        if (effectPrefab == null)
        {
            return;
        }


        bool isFacingRight = effectPivot.parent.transform.localScale.x < 0;
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
            ParticleSystem particleSystem = Instantiate(visualData.prefab, effectPivot.position, Quaternion.identity, effectPivot).GetComponent<ParticleSystem>();

            //particleSystem.transform.localScale = new Vector3(isFacingRight ? -1 : 1, 1, 1);
            particleSystem.transform.rotation = new Quaternion(-90, 0, 0, 0);

            if (particleSystem != null)
            {
                particleSystem.Play();

                var mainModule = particleSystem.main;
                Destroy(particleSystem.gameObject, mainModule.duration * 2);
            }
        }
    }

    public void NoneWeapon(EffectVisualData effectVisualData)
    {
        if (effectPrefab == null)
        {
            return;
        }


        bool isFacingRight = effectPivot.parent.transform.localScale.x < 0;
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