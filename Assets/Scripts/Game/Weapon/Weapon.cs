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

    public void DecreaseDurability()
    {
        currentDurability--;

        if (currentDurability <= 0)
        {
            OnWeaponBroken?.Invoke();
            Destroy(gameObject);
        }
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
