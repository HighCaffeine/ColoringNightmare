using UnityEngine;

public class PlayerNoneWeapon : MonoBehaviour
{
    [SerializeField] private BoxCollider2D collider;
    public void EnableCollider()
    {
        collider.enabled = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            var m = collision.GetComponent<Character>();

            m.TakeDamage(1);
            collider.enabled = false;

            Debug.Log($"[NoneWeapon]{m.CurrentHP} / {m.info.maxHp} take damage 1");
        }
    }
}
