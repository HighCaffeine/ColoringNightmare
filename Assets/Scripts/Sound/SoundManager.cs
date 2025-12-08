using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : ObjectPooling<SoundManager, Sound>
{
    public enum BGM
    {
        BGM_Main_1,
        BGM_Game_1,
        BGM_Boss_1,
        BGM_Clear_1,

        Count,

    }

    public enum Effect
    {
        UI_Default,
        SFX_Weapon_Attack_Light,  // Yellow, Blue (가벼운 휙 소리)
        SFX_Weapon_Attack_Heavy,  // Red, Black (무거운 붕 소리)

        SFX_Weapon_Damaged_Yellow, // 기본 타격
        SFX_Weapon_Damaged_Red,    // 폭발/둔탁한 소리
        SFX_Weapon_Damaged_Blue,   // 베는/날카로운 소리

        SFX_Weapon_Damaged_Black,  // 강렬한 타격

        //WorkStation

        SFX_WorkStation_Mix,       // 잉크 섞을 때 (물방울/찰박 소리)
        SFX_WorkStation_Sketch,    // 그릴 때 소리
        SFX_WorkStation_Weapon_Fail,
        SFX_WorkStation_Weapon_Suc_Good,
        SFX_WorkStation_Weapon_Suc_Great,
        SFX_WorkStation_Weapon_Suc_Perfect,

        SFX_1,
        Count,
    }
    public enum SoundType
    {
        Master,
        Bgm,
        Effect,
    }

    public float masterVol { get; private set; } = 1.0f;
    public float bgmVol { get; private set; } = 1.0f;
    public float effectVol { get; private set; } = 1.0f;

    private const string KEY_MASTER = "MasterVol";
    private const string KEY_BGM = "BgmVol";
    private const string KEY_SFX = "SFXVol";

    public interface OnEndBGM
    {
        public void SetEndBGMEvent(OnEndBGMEvent OnEndBGMEvent);
    }
    public interface OnChangeVol
    {
        public void SetOnChangeVol(OnChangeVolEvent OnChangeVolEvent);
    }
    public interface RegistrationSound
    {
        public void SetRegistrationSound(OnRegistrationSound OnRegistrationSound);
    }
    public delegate void OnRegistrationSound(Sound sound);

    public delegate float OnChangeVolEvent(SoundType soundType);

    public delegate void OnEndBGMEvent(AudioSource audioSource);

    [SerializeField] private AudioClip[] bgms;
    [SerializeField] private AudioClip[] effects;

    [Header("Source")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private List<Sound> activeSFXList;

    private Dictionary<string, string> sceneBGMMap;
    public static SoundManager Instance;

    private new void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            base.Awake();

            storageParent.gameObject.SetActive(true);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        activeSFXList = new List<Sound>();

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        InitializeSceneBGMMap();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnEnable()
    {
        masterVol = PlayerPrefs.GetFloat(KEY_MASTER, 1.0f);
        bgmVol = PlayerPrefs.GetFloat(KEY_BGM, 1.0f);
        effectVol = PlayerPrefs.GetFloat(KEY_SFX, 1.0f);
    }

    private new void Start()
    {
        base.Start();
        // 시작 시 현재 씬 BGM 재생
        PlaySceneBGM(SceneManager.GetActiveScene());
    }

    private void InitializeSceneBGMMap()
    {
        sceneBGMMap = new Dictionary<string, string>
        {
            { "Main", BGM.BGM_Main_1.ToString() },
            { "Game", BGM.BGM_Game_1.ToString() },
        };
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlaySceneBGM(scene);
    }

    private void PlaySceneBGM(Scene scene)
    {
        if (sceneBGMMap.ContainsKey(scene.name))
        {
            PlaySound(sceneBGMMap[scene.name], false);
        }
    }

    public void PlaySound(string name, bool isBGM = true)
    {
        if (isBGM || name.StartsWith("BGM"))
        {
            PlayBGM(name);
        }
        else
        {
            PlaySFX(name);
        }
    }

    private void PlayBGM(string name)
    {
        AudioClip clip = GetClip(SoundType.Bgm, name);
        if (clip == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.volume = masterVol * bgmVol;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    private void PlaySFX(string name)
    {
        AudioClip clip = GetClip(SoundType.Effect, name);
        if (clip == null) return;

        Sound sfxSound = GetPool();
        activeSFXList.Add(sfxSound);

        AudioSource source = sfxSound.GetAudioSource();
        source.loop = false;
        source.clip = clip;
        source.volume = masterVol * effectVol;
        source.pitch = Random.Range(0.95f, 1.05f);
        source.Play();

        StartCoroutine(ReturnSFXCoroutine(sfxSound, clip.length));
    }

    private System.Collections.IEnumerator ReturnSFXCoroutine(Sound sound, float time)
    {
        yield return new WaitForSeconds(time);
        ReturnSound(sound);
    }

    public void ReturnSound(Sound sound)
    {
        if (activeSFXList.Contains(sound))
        {
            activeSFXList.Remove(sound);
        }
        sound.TestOnReturn();
    }

    // 클립 찾기
    public AudioClip GetClip(SoundType type, string name)
    {
        AudioClip[] clips = (type == SoundType.Bgm) ? bgms : effects;
        string nameWithoutExtension = name;
        int dotIndex = name.LastIndexOf('.');

        if (dotIndex > 0)
        {
            nameWithoutExtension = name.Substring(0, dotIndex);
        }
        foreach (AudioClip clip in clips)
        {
            if (clip.name == nameWithoutExtension)
            {
                return clip;
            }
        }
        return null;
    }

    // public AudioClip GetClip(SoundType type, string name)
    // {
    //     AudioClip[] clips = (type == SoundType.Bgm) ? bgms : effects;
    //     foreach (var clip in clips)
    //     {
    //         if (clip.name.Contains(name)) return clip; // 이름 포함 여부로 검색
    //     }
    //     return null;
    // }

    public void OnChangedVol(SoundType type, float value)
    {
        switch (type)
        {
            case SoundType.Master: masterVol = value; PlayerPrefs.SetFloat(KEY_MASTER, value); break;
            case SoundType.Bgm: bgmVol = value; PlayerPrefs.SetFloat(KEY_BGM, value); break;
            case SoundType.Effect: effectVol = value; PlayerPrefs.SetFloat(KEY_SFX, value); break;
        }

        if (bgmSource != null) bgmSource.volume = masterVol * bgmVol;

        foreach (var sound in activeSFXList)
        {
            if (sound != null) sound.GetAudioSource().volume = masterVol * effectVol;
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

}