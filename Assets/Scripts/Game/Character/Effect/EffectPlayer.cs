using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EffectPlayer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BaseSkillLogic skillData;
    private WeaponInkData inkData;

    private List<Collider2D> alreadyHit;

    private CircleCollider2D circleCollider;
    private float originalRadius;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        alreadyHit = new List<Collider2D>();

        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            originalRadius = circleCollider.radius;
        }
    }

    public void Play(EffectVisualData data, BaseSkillLogic skill, WeaponInkData ink, Vector3 localScale, bool isFacingLeft)
    {
        this.skillData = skill;
        this.inkData = ink;

        float scaleMultiplier = Mathf.Max(Mathf.Abs(localScale.x), Mathf.Abs(localScale.y));

        if (circleCollider != null)
        {
            circleCollider.radius = originalRadius * scaleMultiplier;
        }
        transform.localScale = new Vector3(
            scaleMultiplier * (isFacingLeft ? 1 : -1),
            scaleMultiplier,
            scaleMultiplier
        );

        StartCoroutine(PlayAnimation(data));
    }

    private IEnumerator PlayAnimation(EffectVisualData data)
    {
        spriteRenderer.enabled = false;

        yield return null;

        spriteRenderer.enabled = true;

        for (int i = 0; i < data.sprites.Length; i++)
        {
            spriteRenderer.sprite = data.sprites[i];

            yield return new WaitForSeconds(1f / data.animationSpeed);
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (skillData == null || alreadyHit.Contains(other))
        {
            return;
        }

        if (other.CompareTag("Monster"))
        {
            Character monster = other.GetComponent<Character>();
            if (monster != null)
            {
                alreadyHit.Add(other);
                monster.TakeDamage(skillData.baseDamage, skillData.hitEffectVisualData);

                if (inkData != null && inkData.passiveEffect != null && inkData.passiveEffect.effectType == PassiveEffectData.EffectType.Slow)
                {
                    monster.ApplyStatusEffect(new SlowEffect(inkData.passiveEffect.effectValue1, inkData.passiveEffect.effectValue2));
                }
            }
        }
    }
}