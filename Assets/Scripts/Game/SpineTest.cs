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
        run,
    }

    public SkeletonAnimation skeleton; // Spine 캐릭터
    public AniName aniName;
    public int currentTrack = 0;

    void Awake()
    {
        if (skeleton == null)
        {
            skeleton = GetComponent<SkeletonAnimation>();
        }
    }

    void Start()
    {
        if (skeleton != null)
        {
            skeleton.AnimationState.Event += HandleSpineEvent;
        }

        // 코루틴으로 애니메이션 재생
        StartCoroutine(PlaySpine());
    }

    private IEnumerator PlaySpine()
    {
        if (skeleton == null)
        {
            yield break;
        }

        Spine.Animation ani = skeleton.Skeleton.Data.FindAnimation(aniName.ToString());

        if (ani == null)
        {
            Debug.LogError($"Animation '{aniName}' not found");
            yield break;
        }

        skeleton.AnimationState.SetAnimation(currentTrack, ani, false);

        yield return null;
    }

    private void HandleSpineEvent(TrackEntry trackEntry, Spine.Event e)
    {
        Debug.Log($"Spine Event Fired: {e.Data.Name}");

        switch (e.Data.Name)
        {
            case "CreateWeapon":
                SpawnWeapon();
                break;
            case "AttackEffect":
                PlayEffect();
                break;
            default:
                Debug.Log($"none defined ani");
                break;
        }
    }

    private void SpawnWeapon()
    {
        Debug.Log("weapon created");
    }

    private void PlayEffect()
    {
        Debug.Log("play effect");
    }
}
