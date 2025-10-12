using UnityEngine;

public class GamePlayInfo : MonoBehaviour
{
    private int currentIndex = 0;

    [SerializeField] private Transform infoPanel;

    [SerializeField] private UnityEngine.UI.Image targetImage;
    [SerializeField] private Sprite[] playInfo;

    [SerializeField] private Transform endButton;
    [SerializeField] private Transform nextButton;

    void Awake()
    {
        infoPanel.gameObject.SetActive(true);
        UpdateInfo();
    }

    void Start()
    {
        GameManager.Instance.GameOver();
    }

    public void Init()
    {
        GameManager.Instance.GameOver();
        currentIndex = 0;
        UpdateInfo();
    }

    public void PrevInfo()
    {
        if (currentIndex == 0) return;
        if (currentIndex == playInfo.Length - 1)
        {
            nextButton.gameObject.SetActive(false);
            endButton.gameObject.SetActive(true);
        }
        currentIndex--;
        UpdateInfo();
    }

    public void NextInfo()
    {
        currentIndex++;

        if (currentIndex == playInfo.Length - 1)
        {
            nextButton.gameObject.SetActive(true);
            endButton.gameObject.SetActive(false);
        }

        UpdateInfo();
    }

    private void UpdateInfo()
    {
        targetImage.sprite = playInfo[currentIndex];
    }
}
