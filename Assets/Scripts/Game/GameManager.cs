using System.Collections;
using UnityEngine;

public class GameManager : GenericSingleton<GameManager>
{
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject endingPanel;

    public bool IsShowCredits { get; private set; } = false;

    private Coroutine gameoverCoroutine;

    private new void Awake()
    {
        base.Awake();
        Application.targetFrameRate = targetFrameRate;
        DontDestroyOnLoad(this);
    }

    public void PauseGame()
    {
        if (gameoverCoroutine != null) StopCoroutine(gameoverCoroutine);

        Time.timeScale = 0.0f;
    }

    public void GameOver(float delay = 1.5f)
    {
        if (gameoverCoroutine != null) StopCoroutine(gameoverCoroutine);

        gameOverPanel.gameObject.SetActive(true);

        if (delay > 0f)
        {
            gameoverCoroutine = StartCoroutine(TimeScaleDelayRoutine(delay));
        }
        else
        {
            PauseGame();
        }

        Debug.Log($"Game Over Requested (Delay: {delay})");
    }

    private IEnumerator TimeScaleDelayRoutine(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 0.0f;
    }

    public void ResetTimeScale()
    {
        if (gameoverCoroutine != null)
        {
            StopCoroutine(gameoverCoroutine);
            gameoverCoroutine = null;
        }

        Time.timeScale = 1.0f;
    }

    public void Restart()
    {
        ResetTimeScale();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.ReloadGameScene();
        }
    }

    public void GameStart()
    {
        ResetTimeScale();
    }

    public void Exit(bool showCredits = false)
    {
        ResetTimeScale();

        IsShowCredits = showCredits;
        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToScene(SceneName.Menu.ToString());
        }
    }

    public void OnEndingPanel() { endingPanel.SetActive(true); }
    public void OffCreditsFlag() { IsShowCredits = false; }
}