using UnityEngine;
using Spine.Unity;
using System.Collections;

public class BossSpineController : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation skeletonAnimation;

    // 보스 애니메이션 이름 정의
    private const string ANIM_IDLE = "idle";
    private const string ANIM_GROGGY = "groggy";

    // 패턴별 애니메이션 이름 (접두어)
    // 예: P1_Start, P1_Middle, P1_End

    private void Awake()
    {
        if (skeletonAnimation == null)
            skeletonAnimation = GetComponent<SkeletonAnimation>();
    }

    public void PlayIdle()
    {
        SetAnimation(ANIM_IDLE, true);
    }

    public void PlayGroggy()
    {
        SetAnimation(ANIM_GROGGY, true);
    }

    /// <summary>
    /// 3단계 패턴 애니메이션을 재생하는 코루틴 (Start -> Middle -> End)
    /// </summary>
    /// <param name="patternName">패턴 이름 (예: "P1", "P2")</param>
    /// <param name="duration">Middle(유지) 애니메이션을 재생할 시간</param>
    public IEnumerator PlayPatternAnimation(string patternName, float duration)
    {
        // 1. Start (시전 동작)
        string startAnim = $"{patternName}_Start";
        float startDuration = GetAnimationDuration(startAnim);

        if (startDuration > 0)
        {
            SetAnimation(startAnim, false);
            yield return new WaitForSeconds(startDuration);
        }

        // 2. Middle (유지 동작 - 루프)
        string middleAnim = $"{patternName}_Middle";
        if (HasAnimation(middleAnim))
        {
            SetAnimation(middleAnim, true);
        }

        // 지정된 시간(패턴 지속 시간)만큼 대기
        yield return new WaitForSeconds(duration);

        // 3. End (종료 동작)
        string endAnim = $"{patternName}_End";
        float endDuration = GetAnimationDuration(endAnim);

        if (endDuration > 0)
        {
            SetAnimation(endAnim, false);
            yield return new WaitForSeconds(endDuration);
        }

        // 끝나면 Idle로 복귀
        PlayIdle();
    }

    private void SetAnimation(string name, bool loop)
    {
        if (skeletonAnimation == null || skeletonAnimation.Skeleton == null) return;

        var anim = skeletonAnimation.Skeleton.Data.FindAnimation(name);
        if (anim != null)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, anim, loop);
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