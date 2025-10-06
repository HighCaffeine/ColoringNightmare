using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneName
{
    Menu,
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
        StartCoroutine(StartLoadTEST(SceneName.Game.ToString()));
    }

    IEnumerator StartLoadTEST(string sceneName)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PauseBGM();

        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        while (!async.isDone)
        {
            loadingBarProgress?.Invoke(async.progress);

            if (async.progress >= 0.9f)
            {
                yield return null;

                // Game 씬일 때만 플래그 true
                isLoadGameScene = (sceneName == SceneName.Game.ToString());

                async.allowSceneActivation = true;

                break;
            }

            yield return wait;
        }
    }
    //TEST

    public delegate void LoadingBarProgress(float progress);
    public LoadingBarProgress loadingBarProgress;

    IEnumerator StartLoad(string sceneName)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PauseBGM();

        //SceneManager.LoadSceneAsync("Loading");

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

                if (SoundManager.Instance != null) SoundManager.Instance.PlaySound(SoundManager.BGM.BGM_1.ToString(), true);

                break;
            }

            yield return wait;
        }

        yield return null;
    }
}
