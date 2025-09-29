using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private WeaponInkData inkData;
    [SerializeField] private int currentDurability;
    private int damage;

    public UnityEngine.Events.UnityEvent OnWeaponBroken;

    public void SetupInkData(WeaponInkData weaponInkData)
    {
        inkData = weaponInkData;
        currentDurability = weaponInkData.durability;
        damage = weaponInkData.damage;
    }

    public int DecreaseDurability()
    {
        currentDurability--;

        if (currentDurability <= 0)
        {
            OnWeaponBroken?.Invoke();
            StartCoroutine(DestroyWeapon());

            return 0;
        }

        return currentDurability;
    }

    private IEnumerator DestroyWeapon()
    {
        float time = 0.0f;
        float duration = 2.0f;
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
            Character monster = other.GetComponent<Character>();
            if (monster != null)
            {
                monster.TakeDamage(Damage);
            }
        }
    }

    public int Damage => damage;

    public int CurrentDurability => currentDurability;
}
