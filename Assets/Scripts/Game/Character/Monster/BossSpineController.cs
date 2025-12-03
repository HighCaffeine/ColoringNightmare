using UnityEngine;
using Spine;
using Spine.Unity;
using System.Collections;
using System;

public class BossSpineController : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation skeletonAnimation;

    // 보스 애니메이션 이름
    public const string IDLE = "idle";
    public const string GROGGY = "groggy";
    public const string DEAD = "Dead";

    private string lastEventName = "";
    private bool eventTriggered = false;

    private void Awake()
    {
        if (skeletonAnimation == null)
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
        }

        if (skeletonAnimation != null)
        {
            skeletonAnimation.AnimationState.Event += HandleSpineEvent;
        }
    }

    private void HandleSpineEvent(TrackEntry trackEntry, Spine.Event e)
    {
        lastEventName = e.Data.Name;
        eventTriggered = true;
    }

    public void PlayIdle() { SetAnimation(IDLE, true); }
    public void PlayGroggy() { SetAnimation(GROGGY, true); }
    public void PlayDead(MonsterManager.OnEndingEvent OnEndEvent)
    {
        SetAnimation(DEAD, false, () => { OnEndEvent?.Invoke(); });
    }

    public IEnumerator PlayStartAndMiddle(string patternPrefix, float midDuration)
    {
        string startAnim = $"{patternPrefix}_start";
        float startLen = GetAnimationDuration(startAnim);
        if (startLen > 0)
        {
            SetAnimation(startAnim, false);
            yield return new WaitForSeconds(startLen);
        }

        string midAnim = $"{patternPrefix}_mid";
        if (HasAnimation(midAnim))
        {
            SetAnimation(midAnim, true);
        }

        if (midDuration > 0)
        {
            yield return new WaitForSeconds(midDuration);
        }
    }

    public IEnumerator PlayEndAndWaitForEvent(string patternPrefix, string targetEventName)
    {
        string endAnim = $"{patternPrefix}_end";

        // 이벤트 플래그 초기화
        eventTriggered = false;
        lastEventName = "";

        // End 애니메이션 재생
        SetAnimation(endAnim, false);

        float timeout = GetAnimationDuration(endAnim) + 0.5f;
        float timer = 0f;

        while (!eventTriggered || lastEventName != targetEventName)
        {
            timer += Time.deltaTime;
            if (timer > timeout) break; // 무한 대기 방지
            yield return null;
        }
    }

    private void SetAnimation(string name, bool loop, Action onComplete = null)
    {
        if (skeletonAnimation == null || skeletonAnimation.Skeleton == null) return;
        var anim = skeletonAnimation.Skeleton.Data.FindAnimation(name);
        if (anim != null)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, anim, loop);
        }

        TrackEntry entry = skeletonAnimation.AnimationState.SetAnimation(0, anim, loop);

        if (!loop && onComplete != null)
        {
            entry.Complete += _ => onComplete();
        }
    }

    private float GetAnimationDuration(string name)
    {
        if (skeletonAnimation == null || skeletonAnimation.Skeleton == null) return 0f;
        var anim = skeletonAnimation.Skeleton.Data.FindAnimation(name);
        return anim != null ? anim.Duration : 0f;
    }

    private bool HasAnimation(string name)
    {
        if (skeletonAnimation == null || skeletonAnimation.Skeleton == null) return false;
        return skeletonAnimation.Skeleton.Data.FindAnimation(name) != null;
    }
}