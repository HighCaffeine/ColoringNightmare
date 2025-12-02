using UnityEngine;

public class GameManager : GenericSingleton<GameManager>
{
    [SerializeField] private int targetFrameRate = 60;  //Default - 60
    [SerializeField] private GameObject gameOverPanel;

    private bool first = true;

    private new void Awake()
    {
        base.Awake();
        Application.targetFrameRate = targetFrameRate;
        DontDestroyOnLoad(this);
    }

    public void GameOver()
    {
        Time.timeScale = 0.0f;

        if (gameOverPanel != null && !first)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            first = false;
        }

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

    public void Exit()
    {
        SceneController.Instance.GoToScene(SceneName.Menu.ToString());
    }
}