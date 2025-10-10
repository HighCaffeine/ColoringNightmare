using UnityEngine;

public class WorkStationEventTrigger : MonoBehaviour
{
    [SerializeField] private UnityEngine.Events.UnityEvent OnEnterEvent;
    [SerializeField] private UnityEngine.Events.UnityEvent OnExitEvent;
    [SerializeField] private UnityEngine.Events.UnityEvent OnClickEvent;

    private bool isAllow = false;

    public void SetEventAllow(bool isAllow) { this.isAllow = isAllow; if (!isAllow) OnExit(); }
    public void OnEnter() { if (isAllow) OnEnterEvent?.Invoke(); }
    public void OnExit() { OnExitEvent?.Invoke(); }
    public void OnClick() { if (isAllow) OnClickEvent?.Invoke(); }
}
