using System.Collections;
using UnityEngine;

public class Sound : MonoBehaviour, OnReturnPool<Sound>,
                                    SoundManager.OnEndBGM,
                                    SoundManager.OnChangeVol,
                                    SoundManager.RegistrationSound
{
    private OnReturnPoolEvent<Sound> OnReturnPool;
    private SoundManager.OnEndBGMEvent OnEndBGMEvent;

    private SoundManager.OnChangeVolEvent OnChangeVolEvent;
    private SoundManager.OnRegistrationSound OnRegistrationSound;
    private AudioSource audioSource;

    SoundManager.SoundType type;
    bool isMainBGM;
    private Coroutine disableCoroutine;

    private void OnEnable()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }
    // public void Play(AudioClip clip, float vol, SoundManager.SoundType type, bool isMainBGM, bool playNoOffBGM)
    // {
    //     audioSource.loop = false;
    //     audioSource.clip = clip;
    //     audioSource.volume = vol;
    //     audioSource.Play();
    //     this.type = type;
    //     this.isMainBGM = isMainBGM;

    //     if (isMainBGM) audioSource.loop = true;

    //     StartCoroutine(Playing(playNoOffBGM));
    // }

    public void Play(AudioClip clip, float volume, SoundManager.SoundType type, bool isMainBGM, bool multiBGM)
    {
        gameObject.SetActive(true);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();

        // 기존 코루틴이 있다면 정지
        if (disableCoroutine != null) StopCoroutine(disableCoroutine);

        if (type == SoundManager.SoundType.Effect)
        {
            // [★핵심 수정] Invoke 대신 코루틴 사용 -> 시간 정지(0배속) 상태에서도 정상 반환됨
            disableCoroutine = StartCoroutine(DisableSoundRoutine(clip.length + 0.1f));
        }
        else // BGM
        {
            audioSource.loop = true;
        }
    }

    private IEnumerator DisableSoundRoutine(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        DisableSound();
    }

    private void DisableSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ReturnSound(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void TestOnReturn()
    {
        OnReturnPool?.Invoke(this);
        gameObject.SetActive(false);
    }

    public void SetVol()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        float vol = OnChangeVolEvent(type);

        audioSource.volume = vol;
    }

    public AudioSource GetAudioSource()
    {
        return audioSource;
    }

    public void Init(OnReturnPoolEvent<Sound> OnReturnPool)
    {
        this.OnReturnPool = OnReturnPool;

        audioSource = GetComponent<AudioSource>();

        // SetEndBGMEvent(SoundManager.Instance.EndBGM);
        // SetOnChangeVol(SoundManager.Instance.VolChangeEvent);
        // SetRegistrationSound(SoundManager.Instance.RegistrationSoundComponent);

        //OnRegistrationSound(this);
    }

    public void SetEndBGMEvent(SoundManager.OnEndBGMEvent OnEndBGMEvent)
    {
        this.OnEndBGMEvent = OnEndBGMEvent;
    }

    public void SetOnChangeVol(SoundManager.OnChangeVolEvent OnChangeVolEvent)
    {
        this.OnChangeVolEvent = OnChangeVolEvent;
    }

    public void SetRegistrationSound(SoundManager.OnRegistrationSound OnRegistrationSound)
    {
        this.OnRegistrationSound = OnRegistrationSound;
    }
}