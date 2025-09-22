using UnityEngine;

public class WorldHpController : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI txt;

    [SerializeField] private int worldHP;

    [SerializeField] private UnityEngine.Events.UnityEvent OnGameOver;

    private int maxHP;

    void Awake()
    {
        maxHP = worldHP;
        txt.text = string.Format($"HP : {worldHP}/{maxHP}");
    }

    public void SubHP()
    {
        worldHP--;

        if (worldHP <= 0)
        {
            OnGameOver?.Invoke();
            worldHP = 0;
        }

        txt.text = string.Format($"HP : {worldHP}/{maxHP}");
    }
}
