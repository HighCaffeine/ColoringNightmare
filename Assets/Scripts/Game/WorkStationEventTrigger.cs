using UnityEngine;

public interface IClickCondition
{
    bool IsClickAllowed();
}

public class WorkStationEventTrigger : MonoBehaviour
{
    [SerializeField] private UnityEngine.Events.UnityEvent OnEnterEvent;
    [SerializeField] private UnityEngine.Events.UnityEvent OnExitEvent;
    [SerializeField] private UnityEngine.Events.UnityEvent OnClickEvent;

    [SerializeField] private GameObject conditionSource;

    private IClickCondition clickCondition;

    private void Awake()
    {
        if (conditionSource != null)
        {
            clickCondition = conditionSource.GetComponent<IClickCondition>();
        }
    }

    private bool isAllow = false;

    public void SetEventAllow(bool isAllow) { this.isAllow = isAllow; if (!isAllow) OnExit(); }
    public void OnEnter() { if (isAllow) OnEnterEvent?.Invoke(); }
    public void OnExit() { OnExitEvent?.Invoke(); }
    public void OnClick()
    {
        if (clickCondition != null && clickCondition.IsClickAllowed())
        {
            if (isAllow) OnClickEvent?.Invoke();
        }
        else if (conditionSource == null)
        {
            if (isAllow) OnClickEvent?.Invoke();
        }
    }
}
