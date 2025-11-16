using UnityEngine;
using UnityEngine.UI;

public class DesignButton : MonoBehaviour
{
    public UnityEngine.Events.UnityEvent OnClickEvent;
    [SerializeField] private Image buttonImage;

    private bool isDisabled => !buttonImage.raycastTarget;

    public void OnButtonClick()
    {
        if (isDisabled) return;
        OnClickEvent?.Invoke();
    }

    public void ConvertAbleState()
    {
        if (isDisabled)
        {
            buttonImage.raycastTarget = true;
            buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 255f);
        }
        else
        {
            buttonImage.raycastTarget = false;
            buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 125f);
        }
    }

    public void Init()
    {
        buttonImage.raycastTarget = true;
    }
}
