using UnityEngine;
using System.Collections;

public class EffectPlayer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Play(EffectData data)
    {
        StartCoroutine(PlayAnimation(data));
    }

    private IEnumerator PlayAnimation(EffectData data)
    {
        for (int i = 0; i < data.sprites.Length; i++)
        {
            spriteRenderer.sprite = data.sprites[i];
            yield return new WaitForSeconds(1f / data.speed * Time.deltaTime);
        }

        Destroy(gameObject);
    }
}
