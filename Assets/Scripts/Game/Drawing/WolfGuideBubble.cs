using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WolfGuideBubble : GenericSingleton<WolfGuideBubble>
{
    [Header("UI Components")]
    [SerializeField] private GameObject bubbleRoot;
    [SerializeField] private SpriteRenderer iconImage;

    [Header("Resources")]
    [Tooltip("잉크 선택 단계에서 보여줄 4가지 캐릭터 이미지")]
    [SerializeField] private Sprite[] inkCharSprites;

    [Tooltip("도안(풀) 선택 단계에서 보여줄 이미지")]
    [SerializeField] private Sprite poolSelectSprite;

    private Coroutine randomAnimRoutine;

    private void Start()
    {
        ShowInkPhase();
    }

    public void ShowInkPhase()
    {
        if (bubbleRoot != null) bubbleRoot.SetActive(true);
        if (randomAnimRoutine != null) StopCoroutine(randomAnimRoutine);
        randomAnimRoutine = StartCoroutine(RandomInkAnim());
    }

    public void ShowPoolPhase()
    {
        if (bubbleRoot != null) bubbleRoot.SetActive(true);
        if (randomAnimRoutine != null) StopCoroutine(randomAnimRoutine);

        if (iconImage != null)
        {
            iconImage.sprite = poolSelectSprite;
        }
    }

    public void Hide()
    {
        if (randomAnimRoutine != null) StopCoroutine(randomAnimRoutine);
        if (bubbleRoot != null) bubbleRoot.SetActive(false);
    }

    private IEnumerator RandomInkAnim()
    {
        if (inkCharSprites == null || inkCharSprites.Length == 0) yield break;

        while (true)
        {
            int randomIndex = Random.Range(0, inkCharSprites.Length);
            if (iconImage != null) iconImage.sprite = inkCharSprites[randomIndex];

            yield return new WaitForSeconds(1.0f);
        }
    }
}