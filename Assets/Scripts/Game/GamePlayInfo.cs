using UnityEngine;

public class GamePlayInfo : MonoBehaviour
{
    private int currentIndex = 0;

    [SerializeField] private Transform infoPanel;

    [SerializeField] private UnityEngine.UI.Image targetImage;
    [SerializeField] private Sprite[] playInfo;

    [SerializeField] private Transform endButton;
    [SerializeField] private Transform nextButton;

    [SerializeField] private Transform startPanel;

    [SerializeField] private float panelOff = 1.0f;
    [SerializeField] private float startTime = 5.0f;
    [SerializeField] private UnityEngine.Events.UnityEvent OnWaitTimeEndEvent;


    [Header("TEST")][SerializeField] private bool isAllowGameStart;

    private bool isFirst = true;

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

    private void OnWaitTimeEnd()
    {
        OnWaitTimeEndEvent?.Invoke();
    }

    public void OnStartPanel()
    {
        if (!isAllowGameStart) return;
        if (!isFirst) return;
        isFirst = true;

        startPanel.gameObject.SetActive(true);
        Invoke(nameof(PanelOff), panelOff);
        Invoke(nameof(OnWaitTimeEnd), startTime);
    }

    private void PanelOff()
    {
        startPanel.gameObject.SetActive(false);
    }

    public void PrevInfo()
    {
        if (currentIndex == 0) return;
        if (currentIndex == playInfo.Length - 1)
        {
            nextButton.gameObject.SetActive(true);
            endButton.gameObject.SetActive(false);
        }
        currentIndex--;
        UpdateInfo();
    }

    public void NextInfo()
    {
        currentIndex++;

        if (currentIndex == playInfo.Length - 1)
        {
            nextButton.gameObject.SetActive(false);
            endButton.gameObject.SetActive(true);
        }

        UpdateInfo();
    }

    private void UpdateInfo()
    {
        targetImage.sprite = playInfo[currentIndex];
    }
}
