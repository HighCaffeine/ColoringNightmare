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
    private PlayerController playerController;
    public void SetupMouse(PlayerController controller)
    {
        playerController = controller;

        mouseAction = controller.GetPlayerInput().Player2.Mouse;
        mouseAction.started += callback => { if (playerController.IsGroggy()) return; isDrawing = true; OnMouseStarted?.Invoke(); };
        mouseAction.performed += callback => { if (playerController.IsGroggy()) return; OnMousePerformed?.Invoke(); };
        mouseAction.canceled += callback => { if (playerController.IsGroggy()) return; isDrawing = false; OnMouseCanceled?.Invoke(); };
        mouseAction.Enable();
    }

    private void OnDisable()
    {
        mouseAction.started -= callback => { if (playerController.IsGroggy()) return; isDrawing = true; OnMouseStarted?.Invoke(); };
        mouseAction.performed -= callback => { if (playerController.IsGroggy()) return; OnMousePerformed?.Invoke(); };
        mouseAction.canceled -= callback => { if (playerController.IsGroggy()) return; isDrawing = false; OnMouseCanceled?.Invoke(); };
        mouseAction.Disable();
    }

    private void Update()
    {
        if (isDrawing && !playerController.IsGroggy())
        {
            OnMousePerformed?.Invoke();
        }
    }
}
