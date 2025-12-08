using System.Collections;
using UnityEngine;

public class GameManager : GenericSingleton<GameManager>
{
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject endingPanel;

    public bool IsShowCredits { get; private set; } = false;

    private Coroutine gameoverCoroutine;

    public static GameManager Instance;

    private new void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        Application.targetFrameRate = targetFrameRate;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void PauseGame()
    {
        if (gameoverCoroutine != null) StopCoroutine(gameoverCoroutine);

        Time.timeScale = 0.0f;
    }

    public void GameOver(float delay = 1.5f)
    {
        if (gameoverCoroutine != null) StopCoroutine(gameoverCoroutine);

        if (delay > 0f)
        {
            gameoverCoroutine = StartCoroutine(TimeScaleDelayRoutine(delay));
        }
        else
        {
            // 딜레이 없으면 바로 켜고 정지
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            PauseGame();
        }

        Debug.Log($"Game Over Requested (Delay: {delay})");
    }

    private IEnumerator TimeScaleDelayRoutine(float delay)
    {
        // 1. 딜레이 대기
        yield return new WaitForSecondsRealtime(delay);

        // 2. [★이동] 패널 켜기 (딜레이 끝난 후)
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("Game Over Panel Activated!"); // 로그 확인용
        }
        else
        {
            Debug.LogError("GameOver Panel is NOT assigned in GameManager!");
        }

        // 3. 시간 정지
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
        gameoverCoroutine = null;

        IsShowCredits = showCredits;
        if (SceneController.Instance != null)
        {
            Time.timeScale = 1.0f;
            SceneController.Instance.GoToScene(SceneName.Main.ToString());
        }
    }

    public void OnEndingPanel() { endingPanel.SetActive(true); }
    public void OffCreditsFlag() { IsShowCredits = false; }
}