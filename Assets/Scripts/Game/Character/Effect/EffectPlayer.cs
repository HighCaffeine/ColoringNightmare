using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EffectPlayer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BaseSkillLogic skillData;
    private WeaponInkData inkData;

    private float originalRadius;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Play(EffectVisualData data, BaseSkillLogic skill, WeaponInkData ink, Vector3 localScale, bool isFacingLeft)
    {
        this.skillData = skill;
        this.inkData = ink;

        float scaleMultiplier = Mathf.Max(Mathf.Abs(localScale.x), Mathf.Abs(localScale.y));

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 100;
            spriteRenderer.enabled = true;
        }

        transform.localScale = new Vector3(
            scaleMultiplier * (isFacingLeft ? 1 : -1),
            scaleMultiplier,
            scaleMultiplier);

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
}