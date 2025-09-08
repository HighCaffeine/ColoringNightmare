using UnityEngine;

public class ColorButton : MonoBehaviour
{
    public UnityEngine.Events.UnityEvent OnClickEvent;

    [SerializeField] private GameObject disablePanel;

    public void OnButtonClick()
    {
        OnClickEvent?.Invoke();
    }

    public void ConvertAbleState()
    {
        if (disablePanel.activeSelf)
        {
            disablePanel.SetActive(false);
        }
        else
        {
            disablePanel.SetActive(true);
        }
    }
}
