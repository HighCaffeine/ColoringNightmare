using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private WeaponInkData inkData;
    [SerializeField] private int currentDurability;
    private int damage;

    public UnityEngine.Events.UnityEvent OnWeaponBroken;
    private BaseSkillLogic skillLogic;

    private EdgeCollider2D testCollider;

    [HideInInspector]
    public float relativeScaleRatio = 1f;

    public void SetupInkData(WeaponInkData weaponInkData)
    {
        inkData = weaponInkData;
        currentDurability = weaponInkData.durability;
        damage = weaponInkData.damage;
        this.skillLogic = weaponInkData.skillLogic;
        testCollider = GetComponent<EdgeCollider2D>();

        if (this.skillLogic != null)
        {
            this.damage = this.skillLogic.baseDamage;
        }
    }

    public void SetActiveCollider(bool active)
    {
        if (testCollider != null)
            testCollider.enabled = active;
    }

    public WeaponInkData GetInkData() { return inkData; }
    public BaseSkillLogic GetSkillLogic() { return skillLogic; }

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

        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
    }

    void OnDestroy()
    {
        if (inkData != null)
        {
            if (inkData.skillLogic != null)
            {
                Destroy(inkData.skillLogic);
            }
            Destroy(inkData);
        }
    }

    public int Damage => damage;
    public int CurrentDurability => currentDurability;
}