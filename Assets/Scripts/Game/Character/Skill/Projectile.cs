using UnityEngine;

public class Projectile : MonoBehaviour
{
    private ProjectileSkill skillData;
    private WeaponInkData inkData;
    private Vector3 direction;
    private int piercingCount;

    private Bounds moveAreaBounds;

    public void InitFromSkill(ProjectileSkill data, WeaponInkData ink, Vector3 dir)
    {
        this.skillData = data;
        this.inkData = ink;
        this.direction = dir.normalized;
        this.piercingCount = data.projectileParams.piercingCount;

        transform.localScale = Vector3.one * data.projectileParams.size;

        float spriteDirection = dir.x < 0 ? -1f : 1f;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * spriteDirection, transform.localScale.y, transform.localScale.z);

        Destroy(gameObject, data.projectileParams.lifeTime);

        if (MonsterManager.Instance != null)
        {
            moveAreaBounds = MonsterManager.Instance.GetAreaBound();
        }
    }

    void Update()
    {
        if (skillData != null)
        {
            transform.position += direction * skillData.projectileParams.speed * Time.deltaTime;

            if (moveAreaBounds.size != Vector3.zero)
            {
                if (!moveAreaBounds.Contains(transform.position))
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            Character monster = other.GetComponent<Character>();
            if (monster != null)
            {
                monster.TakeDamage(skillData.baseDamage, skillData.hitEffectVisualData);

                if (inkData != null && inkData.passiveEffect != null && inkData.passiveEffect.effectType == PassiveEffectData.EffectType.Slow)
                {
                    monster.ApplyStatusEffect(new SlowEffect(inkData.passiveEffect.effectValue1, inkData.passiveEffect.effectValue2));
                }

                piercingCount--;

                if (piercingCount < 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}