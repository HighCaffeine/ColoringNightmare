using UnityEngine;
using Spine.Unity;

public class PaletteDollController : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation skeleton;

    [Header("Animations")]
    [SerializeField] private string animIdle = "idle";
    [SerializeField] private string animSelect = "ink"; // 선택됐을 때 모션

    private void Awake()
    {
        if (skeleton == null) skeleton = GetComponent<SkeletonAnimation>();
    }

    private void Start()
    {
        PlayIdle();
    }

    public void PlayIdle()
    {
        SetAnim(animIdle, true);
    }

    public void PlaySelect()
    {
        // 선택 모션 후 Idle 복귀
        if (skeleton == null) return;
        skeleton.AnimationState.SetAnimation(0, animSelect, false);
        skeleton.AnimationState.AddAnimation(0, animIdle, true, 0);
    }

    private void SetAnim(string name, bool loop)
    {
        if (skeleton != null && skeleton.Skeleton.Data.FindAnimation(name) != null)
        {
            skeleton.AnimationState.SetAnimation(0, name, loop);
        }

        if (name == animIdle) return;

        Spine.TrackEntry entry = skeleton.AnimationState.SetAnimation(0, name, loop);
        if (!loop)
        {
            entry.Complete += _ => PlayIdle();
        }
    }
}