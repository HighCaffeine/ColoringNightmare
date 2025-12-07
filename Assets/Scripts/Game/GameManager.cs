using UnityEngine;

public class GameManager : GenericSingleton<GameManager>
{
    [SerializeField] private int targetFrameRate = 60;  // Default - 60
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject endingPanel;

    public bool IsShowCredits { get; private set; } = false;

    private new void Awake()
    {
        base.Awake();
        Application.targetFrameRate = targetFrameRate;
        DontDestroyOnLoad(this);
    }

    public void GameOver()
    {
        Time.timeScale = 0.0f;

        Debug.Log("Game Over");
    }

    public void Restart()
    {
        Time.timeScale = 1.0f;

        if (SceneController.Instance != null)
        {
            SceneController.Instance.ReloadGameScene();
        }
    }

    public void ResetTimeScale()
    {
        Time.timeScale = 1.0f;
    }

    public void GameStart()
    {
        ResetTimeScale();
    }

    public void Exit(bool showCredits = false)
    {
        Time.timeScale = 1.0f;

        IsShowCredits = showCredits;

        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToScene(SceneName.Menu.ToString());
        }
    }

    public void OnEndingPanel()
    {
        endingPanel.SetActive(true);
    }

    public void OffCreditsFlag()
    {
        IsShowCredits = false;
    }
}