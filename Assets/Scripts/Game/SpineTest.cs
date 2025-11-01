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
        if (playerController.IsAttacking) return;
        playerController.IsAttacking = true;

        PlaySpine(AniName.attack1, false, onComplete: () =>
        {
            playerController?.ResetAttackCooldown();
            TestPlayIdleSpine();
        });
    }

    public void TestPlayRunSpine()
    {
        if (playerController.IsAttacking) return;

        if (skeleton.AnimationName != AniName.walk.ToString())
        {
            PlaySpine(AniName.walk, true);
        }
    }

    public void TestPlayIdleSpine()
    {
        if (playerController.IsAttacking) return;

        if (skeleton.AnimationName != AniName.idle.ToString())
        {
            PlaySpine(AniName.idle, true);
        }
    }

    private void PlaySpine(AniName aniName, bool isLoop, System.Action onComplete = null)
    {
        if (skeleton == null) return;
        var ani = skeleton.Skeleton.Data.FindAnimation(aniName.ToString());
        if (ani == null)
        {
            Debug.LogError($"Animation '{aniName}' not found");
            return;
        }
        TrackEntry entry = skeleton.AnimationState.SetAnimation(0, ani, isLoop);
        if (!isLoop && onComplete != null)
        {
            entry.Complete += _ => onComplete();
        }
    }

    private void HandleSpineEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (e.Data.Name == SpineEvent.AttackEffect.ToString())
        {
            skillController?.OnAnimationHit();
        }
    }
}