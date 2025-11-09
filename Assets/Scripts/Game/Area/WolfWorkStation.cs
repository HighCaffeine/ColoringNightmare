using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // PlayerInput을 제어하기 위해 추가

[ExecuteAlways]
public class WolfWorkStation : GenericSingleton<WolfWorkStation>
{
    private enum WorkStationType
    {
        None,
        Palette, Sketch, Enhance,
    }

    [Header("Player")]
    [Tooltip("자동으로 이동시킬 플레이어 (Player2 - Wolf)")]
    [SerializeField] private PlayerController playerController;

    [Header("Work Station Interactive Point")]
    [SerializeField] private Transform palettePoint;
    [SerializeField] private Transform sketchPoint;
    [SerializeField] private Transform enhancePoint;

    [Header("Movement Settings")]
    [Tooltip("상호작용 지점으로 자동 이동하는 속도")]
    [SerializeField] private float autoMoveSpeed = 5f; // 플레이어의 자체 속도와 별개로 이 스크립트에서 사용할 속도

    [Space(5f)]
    [Header("Palette Event")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnPalettePanelOn;
    [SerializeField] private UnityEngine.Events.UnityEvent OnPalettePanelOff;

    [Space(5f)]
    [Header("Sketch Event")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnSketchOn;
    [SerializeField] private UnityEngine.Events.UnityEvent OnSketchOff;

    [Space(5f)]
    [Header("Enhance Event")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnEnhancePanelOn;
    [SerializeField] private UnityEngine.Events.UnityEvent OnEnhancePanelOff;

    private WorkStationType workStationType = WorkStationType.None;
    private bool isOnInteractive = false;
    private SpineTest playerSpine; // 플레이어의 스파인 제어를 위한 변수

    public bool IsOnInteractive => isOnInteractive;

    public void SetIsOnInteractive(bool isOnInteractive) { this.isOnInteractive = isOnInteractive; }

    public void SetPaletteType() { workStationType = WorkStationType.Palette; }
    public void SetEnhanceType() { workStationType = WorkStationType.Enhance; }
    public void SetSketchType() { workStationType = WorkStationType.Sketch; }

    private Coroutine moveCoroutine;
    public void OnMoveToInteractivePoint()
    {
        if (playerController == null)
        {
            return;
        }

        if (playerSpine == null) playerSpine = playerController.GetComponent<SpineTest>();

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveToInteractivePoint());
    }

    private IEnumerator MoveToInteractivePoint()
    {
        Transform targetPoint = null;
        switch (workStationType)
        {
            case WorkStationType.Palette:
                targetPoint = palettePoint;
                break;
            case WorkStationType.Sketch:
                targetPoint = sketchPoint;
                break;
            case WorkStationType.Enhance:
                targetPoint = enhancePoint;
                break;
        }

        if (targetPoint == null)
        {
            yield break;
        }

        Vector3 targetPosition = targetPoint.position;

        playerController.GetPlayerInput()?.Player2.Disable();

        playerSpine?.TestPlayRunSpine();

        while (Vector3.Distance(playerController.transform.position, targetPosition) > 0.01f)
        {
            playerController.transform.position = Vector3.MoveTowards(
                playerController.transform.position,
                targetPosition,
                autoMoveSpeed * Time.deltaTime);

            yield return null;
        }

        playerController.transform.position = targetPosition;
        SetIsOnInteractive(true);

        playerSpine?.TestPlayIdleSpine();

        switch (workStationType)
        {
            case WorkStationType.Palette:
                OnPalettePanelOn?.Invoke();
                break;
            case WorkStationType.Sketch:
                OnSketchOn?.Invoke();
                break;
            case WorkStationType.Enhance:
                OnEnhancePanelOn?.Invoke();
                break;
        }

        playerController.GetPlayerInput()?.Player2.Enable();
    }


    public void AllWorkStationOff()
    {
        OnPalettePanelOff?.Invoke();
        OnSketchOff?.Invoke();
        OnEnhancePanelOff?.Invoke();
    }

    public void TEST_DisableDrawing()
    {
        OnSketchOff?.Invoke();
    }

    public void Interactive()
    {
        switch (workStationType)
        {
            case WorkStationType.None:
                break;
            case WorkStationType.Palette:
                if (isOnInteractive) OnPalettePanelOn?.Invoke();
                else OnPalettePanelOff?.Invoke();
                break;
            case WorkStationType.Sketch:
                if (isOnInteractive) OnSketchOn?.Invoke();
                else OnSketchOff?.Invoke();
                break;
            case WorkStationType.Enhance:
                if (isOnInteractive) OnEnhancePanelOn?.Invoke();
                else OnEnhancePanelOff?.Invoke();
                break;
        }
    }
}