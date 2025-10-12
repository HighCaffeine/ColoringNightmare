using UnityEngine;

public class GameManager : GenericSingleton<GameManager>
{
    [SerializeField] private int targetFrameRate = 60;  //Default - 60

    private new void Awake()
    {
        base.Awake();

        Application.targetFrameRate = targetFrameRate;

        DontDestroyOnLoad(this);
    }

    public void GameOver()
    {
        Time.timeScale = 0.0f;
    }

    public void Restart()
    {
        SceneController.Instance.ReloadGameScene();
    }

    public void ResetTimeScale()
    {
        Time.timeScale = 1.0f;
    }

    public void GameStart()
    {
        ResetTimeScale();
    }
}