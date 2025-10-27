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
    private Coroutine spineCoroutine;

    [Header("Dependencies")]
    [SerializeField] private SkillController skillController;
    private PlayerController playerController;

    void Awake()
    {
        if (skeleton == null) skeleton = GetComponent<SkeletonAnimation>();
        playerController = GetComponent<PlayerController>();
    }

    void Start()
    {
        if (skeleton != null) skeleton.AnimationState.Event += HandleSpineEvent;
        TestPlayIdleSpine();
    }

    public void TestPlayAttackSpine()
    {
        CheckCoroutine();
        spineCoroutine = StartCoroutine(PlaySpine(AniName.attack1, false, onComplete: () =>
        {
            TestPlayIdleSpine();
            playerController?.ResetAttackCooldown();
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
        if (spineCoroutine != null) StopCoroutine(spineCoroutine);
        spineCoroutine = null;
    }

    private IEnumerator PlaySpine(AniName aniName, bool isLoop, System.Action onComplete = null)
    {
        if (skeleton == null) yield break;
        var ani = skeleton.Skeleton.Data.FindAnimation(aniName.ToString());
        if (ani == null)
        {
            Debug.LogError($"Animation '{aniName}' not found");
            yield break;
        }
        TrackEntry entry = skeleton.AnimationState.SetAnimation(0, ani, isLoop);
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
            skillController?.OnAnimationHit();
        }
    }
}