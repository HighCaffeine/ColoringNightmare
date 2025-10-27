using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class SpineTest : MonoBehaviour
{
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
        spineCoroutine = StartCoroutine(PlaySpine(AniName.idle, true));
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
        CheckCoroutine();
        spineCoroutine = StartCoroutine(PlaySpine(AniName.walk, true));
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
        if (e.Data.Name == "AttackEffect")
        {
            attackEffect?.Invoke();
        }
    }
}