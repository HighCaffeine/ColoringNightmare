using UnityEngine;
using Spine.Unity;

public class DreamBearController : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation skeleton;

    [Header("Animation Names")]
    [SerializeField] private string animIdle = "idle";
    [SerializeField] private string animResult = "result";
    [SerializeField] private string animSelectIdle = "select_idle";

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

    public void PlayResult()
    {
        if (skeleton == null) return;
        skeleton.AnimationState.SetAnimation(0, animResult, false);
        skeleton.AnimationState.AddAnimation(0, animIdle, true, 0);
    }

    public void PlaySelectIdle()
    {
        if (skeleton == null) return;
        skeleton.AnimationState.SetAnimation(0, animSelectIdle, false);
        skeleton.AnimationState.AddAnimation(0, animIdle, true, 0);
    }

    private void SetAnim(string name, bool loop)
    {
        if (skeleton != null && skeleton.Skeleton.Data.FindAnimation(name) != null)
        {
            skeleton.AnimationState.SetAnimation(0, name, loop);
        }

        Spine.TrackEntry entry = skeleton.AnimationState.SetAnimation(0, name, loop);
        if (!loop)
        {
            entry.Complete += _ => PlayIdle();
        }
    }
}