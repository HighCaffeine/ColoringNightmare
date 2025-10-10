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

    void Awake()
    {
        if (skeleton == null)
            skeleton = GetComponent<SkeletonAnimation>();
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
            spineCoroutine = StartCoroutine(PlaySpine(AniName.idle, true));
        }));
    }

    public void TestPlayRunSpine()
    {
        CheckCoroutine();
        spineCoroutine = StartCoroutine(PlaySpine(AniName.walk, true));
    }

    public void TestPlayIdleSpine()
    {
        CheckCoroutine();
        spineCoroutine = StartCoroutine(PlaySpine(AniName.idle, true));
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

        TrackEntry entry = skeleton.AnimationState.SetAnimation(currentTrack, ani, isLoop);

        if (!isLoop && onComplete != null)
        {
            // 애니메이션 한 번 재생 끝나면 onComplete 실행
            entry.Complete += _ => onComplete();
        }

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
                Debug.Log("none defined ani");
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
        attackEffect?.Invoke();
    }
}
