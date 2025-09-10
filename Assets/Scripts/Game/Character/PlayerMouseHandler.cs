using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMouseHandler : MonoBehaviour
{
    [Space(5f)]
    [Header("Mouse Events")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnMouseStarted;
    [SerializeField] private UnityEngine.Events.UnityEvent OnMousePerformed;
    [SerializeField] private UnityEngine.Events.UnityEvent OnMouseCanceled;

    private InputAction mouseAction;
    private bool isDrawing = false;
    public void SetupMouse(PlayerController controller)
    {
        mouseAction = controller.GetPlayerInput().Player2.Mouse;
        mouseAction.started += ctx => { isDrawing = true; OnMouseStarted?.Invoke(); };
        mouseAction.canceled += ctx => { isDrawing = false; OnMouseCanceled?.Invoke(); };
        mouseAction.Enable();
    }

    private void OnDisable()
    {
        mouseAction.started -= callback => { isDrawing = true; OnMouseStarted?.Invoke(); };
        mouseAction.canceled -= callback => { isDrawing = false; OnMouseCanceled?.Invoke(); };
        mouseAction.Disable();
    }


    private void Update()
    {
        if (isDrawing)
        {
            OnMousePerformed?.Invoke();
        }
    }

    private void OnMouseClickStarted()
    {
        OnMouseStarted?.Invoke();
    }

    private void OnMouseClickCanceled()
    {
        OnMouseCanceled?.Invoke();
    }

}
