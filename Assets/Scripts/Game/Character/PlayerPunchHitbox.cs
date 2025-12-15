using UnityEngine;

public class PlayerPunchHitbox : MonoBehaviour
{
    [SerializeField] private CharacterData playerInfo;
    [SerializeField] private EffectVisualData punchHitEffect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            Character monster = other.GetComponent<Character>();
            if (monster != null)
            {
                monster.TakeDamage(playerInfo.dmg, punchHitEffect);
            }
        }
    }
}