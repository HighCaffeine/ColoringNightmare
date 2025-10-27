using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class SpineTest : MonoBehaviour
{
    public enum SpineEvent
    {
        None,
        AttackEffect,
    }

    public enum AniName
    {
        attack1,
        idle,
        walk,
    }

    public SkeletonAnimation skeleton;
    public AniName spineAniName;
    public int currentTrack = 0;

    private Coroutine spineCoroutine;

    [Header("AttackEffect")]
    [SerializeField] private UnityEngine.Events.UnityEvent attackEffect;

    private PlayerController playerController;

    void Awake()
    {
        if (skeleton == null)
            skeleton = GetComponent<SkeletonAnimation>();
        playerController = GetComponent<PlayerController>();
    }

    void Start()
    {
        if (skeleton != null)
            skeleton.AnimationState.Event += HandleSpineEvent;
        TestPlayIdleSpine();
    }

    public void TestPlayAttackSpine()
    {
        CheckCoroutine();
        spineCoroutine = StartCoroutine(PlaySpine(AniName.attack1, false, onComplete: () =>
        {
            TestPlayIdleSpine();
            if (playerController != null)
            {
                playerController.ResetAttackCooldown();
            }
        }));
    }

    public void TestPlayRunSpine()
    {
        if (skeleton.AnimationName != AniName.walk.ToString())
        {
            CheckCoroutine();
            spineCoroutine = StartCoroutine(PlaySpine(AniName.walk, true));
        }
    }

    public void TestPlayIdleSpine()
    {
        if (skeleton.AnimationName != AniName.idle.ToString())
        {
            CheckCoroutine();
            spineCoroutine = StartCoroutine(PlaySpine(AniName.idle, true));
        }
    }

    private void CheckCoroutine()
    {
        if (spineCoroutine != null)
        {
            StopCoroutine(spineCoroutine);
            spineCoroutine = null;
        }
    }

    private IEnumerator PlaySpine(AniName aniName, bool isLoop, System.Action onComplete = null)
    {
        if (skeleton == null) yield break;

        Spine.Animation ani = skeleton.Skeleton.Data.FindAnimation(aniName.ToString());
        if (ani == null)
        {
            Debug.LogError($"Animation '{aniName}' not found");
            yield break;
        }

        TrackEntry entry = skeleton.AnimationState.SetAnimation(currentTrack, ani, isLoop);
        if (!isLoop && onComplete != null)
        {
            entry.Complete += _ => onComplete();
        }
        yield return null;
    }

    private void HandleSpineEvent(TrackEntry trackEntry, Spine.Event e)
    {
        SpineEvent eventType;
        try
        {
            eventType = (SpineEvent)System.Enum.Parse(typeof(SpineEvent), e.Data.Name);
        }
        catch
        {
            Debug.LogWarning($"Undefined Spine Event: '{e.Data.Name}'");
            eventType = SpineEvent.None;
        }

        // enum 값을 기준으로 분기 처리
        switch (eventType)
        {
            case SpineEvent.AttackEffect:
                attackEffect?.Invoke();
                break;

            case SpineEvent.None:
            default:
                break;
        }
    }
}