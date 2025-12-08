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

    public enum SoundType
    {
        Master,
        Bgm,
        Effect,
    }

    public float masterVol { get; private set; }
    public float bgmVol { get; private set; }
    public float effectVol { get; private set; }

    private const string KEY_MASTER = "MasterVol";
    private const string KEY_BGM = "BgmVol";
    private const string KEY_SFX = "SFXVol";

    [SerializeField] private AudioClip[] bgms;
    [SerializeField] private AudioClip[] effects;

    [Header("Source")]
    [SerializeField] private AudioSource nowPlaySource;
    [SerializeField] private List<Sound> playSoundList;

    [SerializeField] private AudioMixer mixer;

    // 씬별 BGM 매핑
    private Dictionary<string, string> sceneBGMMap;

    private new void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(this);

        storageParent.gameObject.SetActive(true);

        playSoundList = new List<Sound>();
        nowPlaySource = gameObject.GetComponent<AudioSource>();

        // 씬별 BGM 설정
        InitializeSceneBGMMap();

        // 씬 로드 이벤트 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitializeSceneBGMMap()
    {
        sceneBGMMap = new Dictionary<string, string>
        {
            { "Main", BGM.BGM_Main_1.ToString() },  // 메인 화면 씬 이름
            { "Game", BGM.BGM_Game_1.ToString() },  // 게임 씬 이름
            // 추가 씬들...
        };
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 기존 BGM 중지 및 정리
        StopAllBGM();

        // 씬에 맞는 BGM 재생
        if (sceneBGMMap.ContainsKey(scene.name))
        {
            string bgmName = sceneBGMMap[scene.name];
            PlaySound(bgmName, true);
        }
    }

    private void OnEnable()
    {
        masterVol = PlayerPrefs.HasKey(KEY_MASTER) ? PlayerPrefs.GetFloat(KEY_MASTER) : 1.0f;
        bgmVol = PlayerPrefs.HasKey(KEY_BGM) ? PlayerPrefs.GetFloat(KEY_BGM) : 1.0f;
        effectVol = PlayerPrefs.HasKey(KEY_SFX) ? PlayerPrefs.GetFloat(KEY_SFX) : 1.0f;
    }

    private new void Start()
    {
        base.Start();

        // OnSceneLoaded에서 처리하므로 Start에서는 재생하지 않음
        // 단, 씬 로드 이벤트보다 Start가 먼저 실행될 수 있으므로
        // 현재 씬에 BGM이 없으면 기본 BGM 재생
        if (playSoundList.Count == 0)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            if (sceneBGMMap.ContainsKey(currentScene))
            {
                PlaySound(sceneBGMMap[currentScene], true);
            }
            else
            {
                PlaySound(BGM.BGM_Main_1.ToString(), true);
            }
        }
    }

    public void OnChangedVol(SoundType type, float value)
    {
        switch (type)
        {
            case SoundType.Master:
                masterVol = value;
                PlayerPrefs.SetFloat(KEY_MASTER, value);
                break;
            case SoundType.Bgm:
                bgmVol = value;
                PlayerPrefs.SetFloat(KEY_BGM, value);
                break;
            case SoundType.Effect:
                effectVol = value;
                PlayerPrefs.SetFloat(KEY_SFX, value);
                break;
        }

        foreach (Sound sound in playSoundList)
        {
            sound.SetVol();
        }

        nowPlaySource.volume = masterVol * bgmVol;
    }

    public void RegistrationSoundComponent(Sound sound)
    {
        playSoundList.Add(sound);
    }

    public float VolChangeEvent(SoundType type)
    {
        float value = SoundType.Effect == type ? effectVol : bgmVol;
        return masterVol * value;
    }

    public AudioClip GetClip(SoundType type, string name, bool multiBGM)
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
                if (type == SoundType.Bgm)
                {
                    PauseBGM();

                    if (!multiBGM)
                    {
                        nowPlaySource.clip = null;
                    }
                }

                return clip;
            }
        }

        Debug.LogWarning($"SoundManager: AudioClip not found for name '{name}' (checked as '{nameWithoutExtension}')");
        return null;
    }

    public void PauseBGM()
    {
        nowPlaySource.Pause();
    }

    public void UnPauseBGM()
    {
        nowPlaySource.UnPause();
    }

    public void PlaySound(string name, bool isMainBGM, bool multiBGM = false)
    {
        if (Instance == null || playSoundList == null)
        {
            return;
        }

        if (!multiBGM && name.StartsWith("BGM"))
        {
            StopAllBGM();
        }

        Sound sound = GetPool();

        string[] soundType = name.Split('_');
        playSoundList.Add(sound);

        SoundType type = soundType[0] == "SFX" ? SoundType.Effect : SoundType.Bgm;

        sound.Play(GetClip(type, name, multiBGM), masterVol * effectVol, type, isMainBGM, multiBGM);

        nowPlaySource.volume = masterVol * bgmVol;

        AudioSource source = sound.GetAudioSource();
        source.loop = false;

        if (type == SoundType.Effect)
        {
            source.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        }

        if (multiBGM)
        {
            nowPlaySource?.Pause();
            this.multiBGM = source;
        }

        if (soundType[0] == "BGM")
        {
            source.loop = true;
        }
    }

    // 모든 BGM 중지 및 정리
    private void StopAllBGM()
    {
        // nowPlaySource 정리
        if (nowPlaySource != null)
        {
            nowPlaySource.Stop();
            nowPlaySource.clip = null;
        }

        // playSoundList에서 BGM만 찾아서 정리
        for (int i = playSoundList.Count - 1; i >= 0; i--)
        {
            Sound sound = playSoundList[i];
            AudioSource source = sound.GetAudioSource();

            // BGM인지 확인 (clip 이름이 BGM으로 시작하거나, loop가 true인 경우)
            if (source != null && source.clip != null)
            {
                bool isBGM = source.clip.name.Contains("BGM") || source.loop;

                if (isBGM)
                {
                    source.Stop();
                    playSoundList.RemoveAt(i);
                    sound.TestOnReturn(); // 풀로 반환
                }
            }
        }

        // multiBGM도 정리
        if (multiBGM != null)
        {
            multiBGM.Stop();
            multiBGM = null;
        }
    }

    private AudioSource multiBGM;

    public void EndBGM(AudioSource audioSource)
    {
        nowPlaySource = audioSource;
    }

    public void EndMultiAudio()
    {
        multiBGM?.Pause();
    }

    public void ReplayAudio()
    {
        if (nowPlaySource != null)
        {
            nowPlaySource.gameObject.SetActive(true);
        }

        nowPlaySource?.UnPause();
    }

    public string[] TestGetSound(SoundType type)
    {
        string[] audioClips;
        int count = type == SoundType.Effect ? effects.Length : bgms.Length;

        audioClips = type == SoundType.Effect ? new string[effects.Length] : new string[bgms.Length];

        for (int i = 0; i < count; i++)
        {
            audioClips[i] = type == SoundType.Effect ? effects[i].name : bgms[i].name;
        }

        return audioClips;
    }
}