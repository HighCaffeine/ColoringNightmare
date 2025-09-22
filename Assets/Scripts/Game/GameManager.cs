using UnityEngine;

public class GameManager : GenericSingleton<GameManager>
{
    [SerializeField] private int targetFrameRate = 60;  //Default - 60

    private new void Awake()
    {
        base.Awake();

        Application.targetFrameRate = targetFrameRate;
    }

    public void GameOver()
    {
        Time.timeScale = 0.0f;
    }
}