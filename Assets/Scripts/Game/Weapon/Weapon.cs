using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private WeaponInkData inkData;
    [SerializeField] private int currentDurability;
    private int damage;

    public UnityEngine.Events.UnityEvent OnWeaponBroken;
    private SkillData skillData;

    private System.Func<bool> isAllowAttack;

    public void SetupInkData(WeaponInkData weaponInkData)
    {
        inkData = weaponInkData;
        currentDurability = weaponInkData.durability;
        damage = weaponInkData.damage;
        this.skillData = weaponInkData.skillData;
    }

    public void InitEvent(System.Func<bool> func)
    {
        isAllowAttack = func;
    }

    public WeaponInkData GetInkData() { return inkData; }

    public SkillData GetSKillData() { return skillData; }

    public int DecreaseDurability()
    {
        if (!isAllowAttack()) return currentDurability;

        currentDurability--;

        if (currentDurability <= 0)
        {
            OnWeaponBroken?.Invoke();

            EffectController.Instance.SetVisualData(null);
            DestroyWeapon();
            return 0;
        }

        return currentDurability;
    }

    private Coroutine destroyCoroutine;

    public void DestroyWeapon()
    {
        if (destroyCoroutine != null)
        {
            return;
        }

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

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            Debug.Log("[Weapon] OnTriggerEnter Monster");

            Character monster = other.GetComponent<Character>();
            if (monster != null)
            {
                monster.TakeDamage(Damage);

                if (inkData.passiveEffect != null && inkData.passiveEffect.effectType == PassiveEffectData.EffectType.Slow)
                {
                    float slowRate = inkData.passiveEffect.effectValue1; // 둔화율
                    float slowDuration = inkData.passiveEffect.effectValue2; // 둔화 지속시간

                    // 몬스터에게 새로운 SlowEffect 인스턴스 적용
                    monster.ApplyStatusEffect(new SlowEffect(0.5f, 2.0f));
                }
            }
        }
    }

    public int Damage => damage;

    public int CurrentDurability => currentDurability;
}
