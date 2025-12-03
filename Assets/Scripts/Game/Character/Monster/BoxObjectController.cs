using UnityEngine;
using Spine.Unity;

public class BoxObjectController : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation skeletonAnimation;

    private const string ANI_SPAWN = "SPAWN";
    private const string ANI_IDLE = "IDLE";
    private const string ANI_ATTACK_START = "ATTACK_START";
    private const string ANI_ATTACK_END = "ATTACK_END";

    private void Awake()
    {
        if (skeletonAnimation == null)
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
        }
    }

    public void PlaySpawn()
    {
        if (skeletonAnimation == null) return;
        // 소환 모션 후 Idle 대기
        skeletonAnimation.AnimationState.SetAnimation(0, ANI_SPAWN, false);
        skeletonAnimation.AnimationState.AddAnimation(0, ANI_IDLE, true, 0);
    }

    public void PlayAttack()
    {
        if (skeletonAnimation == null) return;
        skeletonAnimation.AnimationState.SetAnimation(0, ANI_ATTACK_START, false);
    }

    public void PlayEnd()
    {
        if (skeletonAnimation == null) return;
        skeletonAnimation.AnimationState.SetAnimation(0, ANI_ATTACK_END, false);
    }

    public void SetFlip(bool isLeft)
    {
        if (skeletonAnimation == null) return;
        skeletonAnimation.skeleton.ScaleX = isLeft ? -1 : 1;
    }
}