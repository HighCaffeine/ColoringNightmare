using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public enum SceneName
{
    Main,
    Loading,
    Game,


    Count
}

public class SceneController : GenericSingleton<SceneController>
{
    private new void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        Time.timeScale = 1.0f;

        if (GameManager.Instance != null && GameManager.Instance.IsShowCredits)
        {
            GameObject creditObj = GameObject.FindGameObjectWithTag("Credit");

            if (creditObj != null)
            {
                creditObj.transform.GetChild(0).gameObject.SetActive(true);
            }
            GameManager.Instance.OffCreditsFlag();
        }
    }

    public static bool IsLoadGameScene => isLoadGameScene;

    private static bool isLoadGameScene = false;

    public void GoToScene(string sceneName)
    {
        StartCoroutine(StartLoad(sceneName));
    }

    //TEST
    public void ReloadGameScene()
    {
        isLoadGameScene = false; // 초기화
        StartCoroutine(StartLoad(SceneName.Game.ToString()));
    }

    public void LoadCutScene()
    {
        SceneManager.LoadSceneAsync("IntroScene");
    }

    public void GameOff()
    {
        Application.Quit();
    }
    public delegate void LoadingBarProgress(float progress);
    public LoadingBarProgress loadingBarProgress;

    IEnumerator StartLoad(string sceneName)
    {
        loadingBarProgress = null;

        Time.timeScale = 1.0f;
        SceneManager.LoadSceneAsync("Loading");

        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);

        async.allowSceneActivation = false;
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        while (!async.isDone)
        {
            loadingBarProgress?.Invoke(async.progress);

            if (async.progress >= 0.9f)
            {
                loadingBarProgress?.Invoke(1f);

                yield return new WaitForSeconds(1f);

                async.allowSceneActivation = true;

                isLoadGameScene = true;

                //if (SoundManager.Instance != null) SoundManager.Instance.PlaySound(SoundManager.BGM.BGM_1.ToString(), true);

                break;
            }

            yield return wait;
        }

        yield return null;
    }
}
