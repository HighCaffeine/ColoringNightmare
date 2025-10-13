using UnityEngine;
using System.Collections;

public class EffectPlayer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Play(bool isRight, EffectVisualData data)
    {
        //transform.localScale = new Vector3(isRight ? -1 : 1, 1, 1);
        StartCoroutine(PlayAnimation(data));
    }

    // EffectPlayer.cs
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
