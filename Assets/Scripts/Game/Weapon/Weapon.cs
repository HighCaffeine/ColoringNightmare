using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private WeaponInkData inkData;
    [SerializeField] private int currentDurability;
    private int damage;
    private WeaponManager.WeaponType currentWeaponType;

    [SerializeField] private BaseSkillLogic skillLogic;
    private EdgeCollider2D testCollider;
    private List<Character> hitTargets = new List<Character>();
    private Character player;

    public UnityEngine.Events.UnityEvent OnWeaponBroken;
    [HideInInspector] public float relativeScaleRatio = 1f;


    public void Initialize(WeaponInkData data, WeaponManager.WeaponType type)
    {
        this.inkData = data;
        this.currentWeaponType = type;
        this.player = null;

        if (inkData != null)
        {
            currentDurability = inkData.durability;
            damage = inkData.damage;
            skillLogic = inkData.skillLogic;

            if (skillLogic != null)
            {
                damage = skillLogic.baseDamage;
            }
        }

        // 콜라이더 초기화
        testCollider = GetComponent<EdgeCollider2D>();
        if (testCollider != null) testCollider.enabled = false;
    }

    public void Equip(Character player)
    {
        this.player = player;
    }

    private void InitData()
    {
        if (inkData == null) return;

        currentDurability = inkData.durability;
        damage = inkData.damage;
        this.skillLogic = inkData.skillLogic;

        if (this.skillLogic != null)
        {
            this.damage = this.skillLogic.baseDamage;
        }

        var weaponCollider = GetComponent<BoxCollider2D>();
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    public void EnableHitbox()
    {
        hitTargets.Clear();
        if (testCollider != null) testCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        if (testCollider != null) testCollider.enabled = false;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            Character monster = other.GetComponent<Character>();

            if (monster != null)
            {
                if (hitTargets.Contains(monster)) return;
                hitTargets.Add(monster);

                monster.TakeDamage(damage, GetHitEffect());
                ApplyPassiveEffect(monster);
            }
        }
    }

    public void SetScaleBasedOnScore(float ratio)
    {
        relativeScaleRatio = ratio;
        transform.localScale *= ratio;

        Debug.Log($"Weapon Size Adjusted: {ratio * 100}%");
    }

    private void ApplyPassiveEffect(Character monster)
    {
        if (inkData.passiveEffect == null) return;
        PassiveEffectData passive = inkData.passiveEffect;

        switch (passive.effectType)
        {
            case PassiveEffectData.EffectType.Slow:
                monster.ApplyStatusEffect(new SlowEffect(passive.effectValue1, passive.effectValue2, passive.statusColorType));
                PlayStatusEffect(monster.transform.position, passive.statusVisual);
                break;

            case PassiveEffectData.EffectType.Poison:
                monster.ApplyStatusEffect(new PoisonEffect((int)passive.effectValue1, passive.effectValue2, passive.statusColorType));
                PlayStatusEffect(monster.transform.position, passive.statusVisual);
                break;

            case PassiveEffectData.EffectType.Heal:
                player.Heal(1);
                PlayStatusEffect(player.transform.position, passive.statusVisual);
                break;
        }
    }

    private void PlayStatusEffect(Vector3 pos, EffectVisualData visual)
    {
        if (visual != null)
        {
            player?.GetComponent<EffectController>()?.PlayHitEffectAt(pos, visual, false);
        }
    }

    public EffectVisualData GetAttackEffect()
    {
        return inkData.GetVisualData(currentWeaponType).attackEffect;
    }

    public EffectVisualData GetHitEffect()
    {
        return inkData.GetVisualData(currentWeaponType).hitEffect;
    }

    public WeaponInkData GetInkData() { return inkData; }
    public BaseSkillLogic GetSkillLogic() { return skillLogic; }
    public WeaponManager.WeaponType GetWeaponType() { return currentWeaponType; }

    public int DecreaseDurability()
    {
        currentDurability--;
        if (currentDurability <= 0)
        {
            OnWeaponBroken?.Invoke();
            DestroyWeapon();
            return 0;
        }
        return currentDurability;
    }

    private Coroutine destroyCoroutine;
    public void DestroyWeapon()
    {
        if (destroyCoroutine != null) return;
        destroyCoroutine = StartCoroutine(DestroyWeaponCoroutine());
    }
    private IEnumerator DestroyWeaponCoroutine()
    {
        float time = 0.0f;
        float duration = 1.0f;
        Vector3 initialScale = transform.localScale;
        while (time < duration)
        {
            time += Time.deltaTime;
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, time / duration);
            yield return null;
        }
        transform.localScale = Vector3.zero;
        if (transform.parent != null) Destroy(transform.parent.gameObject);
        else Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (inkData != null)
        {
            if (inkData.skillLogic != null) Destroy(inkData.skillLogic);
            Destroy(inkData);
        }
    }
    public void SetActiveCollider(bool active) { if (testCollider != null) testCollider.enabled = active; }
}