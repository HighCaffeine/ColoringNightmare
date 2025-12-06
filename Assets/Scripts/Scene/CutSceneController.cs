using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public enum CutsceneEffectType
{
    None,               // 효과 없음 (즉시 등장)
    FadeIn,             // 제자리에서 페이드 인
    SlideLeftToRight,   // 왼쪽 -> 오른쪽
    SlideRightToLeft,   // 오른쪽 -> 왼쪽
    SlideTopToBottom,   // 위 -> 아래
    SlideBottomToTop    // 아래 -> 위
}


[System.Serializable]
public class CutsceneStep
{
    [Header("Target")]
    [Tooltip("등장시킬 UI 오브젝트")]
    public GameObject targetObject;

    [Header("Effect Settings")]
    [Tooltip("등장 효과")]
    public CutsceneEffectType effectType;
    [Tooltip("슬라이드 시 페이드 인을 같이 할지 여부")]
    public bool useFadeWithSlide = true;
    [Tooltip("효과 재생 시간")]
    public float duration = 0.5f;
    [Tooltip("슬라이드 이동 거리 (UI 좌표 기준)")]
    public float slideDistance = 100f;

    [Header("Sound")]
    [Tooltip("재생할 효과음 이름 (SoundManager 사용 시)")]
    public SoundManager.Effect soundName;

    public UnityEngine.Events.UnityEvent OnSceneEndEvent;
}

public class CutSceneController : MonoBehaviour
{
    [Header("Cutscene Sequence")]
    [SerializeField] private List<CutsceneStep> steps;

    [Header("Events")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnCutsceneEnd;

    private int currentIndex = 0;
    private bool isPlayingEffect = false;
    private Coroutine currentCoroutine;

    private Dictionary<GameObject, Vector2> originalPositions = new Dictionary<GameObject, Vector2>();

    private void Awake()
    {
        foreach (var step in steps)
        {
            if (step.targetObject != null)
            {
                RectTransform rect = step.targetObject.GetComponent<RectTransform>();
                if (rect != null)
                {
                    originalPositions[step.targetObject] = rect.anchoredPosition;
                }

                if (step.targetObject.GetComponent<CanvasGroup>() == null)
                {
                    step.targetObject.AddComponent<CanvasGroup>();
                }

                step.targetObject.SetActive(false);
            }
        }
    }

    private void Start()
    {
        if (steps.Count > 0)
        {
            ShowStep(0);
        }
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            if (isPlayingEffect)
            {
                CompleteStep(currentIndex);
            }
            else
            {
                NextStep();
            }
        }
    }

    private void NextStep()
    {
        currentIndex++;
        if (currentIndex < steps.Count)
        {
            ShowStep(currentIndex);
        }
        else
        {
            EndCutscene();
        }
    }

    private void EndCutscene()
    {
        Debug.Log("Cutscene Ended");
        OnCutsceneEnd?.Invoke();
    }

    private void ShowStep(int index)
    {
        CutsceneStep step = steps[index];
        if (step.targetObject == null)
        {
            NextStep();
            return;
        }

        step.targetObject.SetActive(true);
        PlaySound(step);

        if (step.effectType == CutsceneEffectType.None)
        {
            // 효과 없음: 바로 보여주고 대기
            ResetObjectState(step.targetObject);
            isPlayingEffect = false;
        }
        else
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);

            currentCoroutine = StartCoroutine(EffectRoutine(step));
        }

        step.OnSceneEndEvent?.Invoke();
    }

    private void PlaySound(CutsceneStep step)
    {
        if (!string.IsNullOrEmpty(step.soundName.ToString()))
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySound(step.soundName.ToString(), false);
        }
    }

    private IEnumerator EffectRoutine(CutsceneStep step)
    {
        isPlayingEffect = true;

        GameObject target = step.targetObject;
        RectTransform rect = target.GetComponent<RectTransform>();
        CanvasGroup group = target.GetComponent<CanvasGroup>();

        Vector2 endPos = originalPositions[target];
        Vector2 startPos = endPos;

        // 슬라이드 시작 위치 설정
        switch (step.effectType)
        {
            case CutsceneEffectType.SlideLeftToRight: startPos += Vector2.left * step.slideDistance; break;
            case CutsceneEffectType.SlideRightToLeft: startPos += Vector2.right * step.slideDistance; break;
            case CutsceneEffectType.SlideTopToBottom: startPos += Vector2.up * step.slideDistance; break;
            case CutsceneEffectType.SlideBottomToTop: startPos += Vector2.down * step.slideDistance; break;
        }

        float elapsed = 0f;

        // 초기 상태 설정
        if (rect != null) rect.anchoredPosition = startPos;

        if (group != null && (step.effectType == CutsceneEffectType.FadeIn || step.useFadeWithSlide))
        {
            group.alpha = 0f;
        }
        else if (group != null)
        {
            group.alpha = 1f;
        }

        while (elapsed < step.duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / step.duration);

            float easeT = 1 - (1 - t) * (1 - t);

            if (rect != null)
            {
                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, easeT);
            }

            if (group != null && (step.effectType == CutsceneEffectType.FadeIn || step.useFadeWithSlide))
            {
                group.alpha = easeT;
            }

            yield return null;
        }

        CompleteStep(currentIndex);
    }

    private void CompleteStep(int index)
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);

        CutsceneStep step = steps[index];
        ResetObjectState(step.targetObject);

        isPlayingEffect = false;
    }

    private void ResetObjectState(GameObject target)
    {
        if (target == null) return;

        RectTransform rect = target.GetComponent<RectTransform>();
        CanvasGroup group = target.GetComponent<CanvasGroup>();

        if (originalPositions.ContainsKey(target) && rect != null)
        {
            rect.anchoredPosition = originalPositions[target];
        }

        if (group != null) group.alpha = 1f;
    }

    public void LoadGame()
    {
        SceneController.Instance.ReloadGameScene();
    }
}