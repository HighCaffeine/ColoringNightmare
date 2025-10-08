using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerType
{
    Player1,
    Player2,

}

public class PlayerController : Character
{

    [Header("TEST Event")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnPlayerDead;
    [SerializeField] private UnityEngine.Events.UnityEvent OnPlayerRespawn;

    [Header("Interactive")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnInteractive;

    [Header("Attack")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnAttack;

    [Header("Weapon")]
    [SerializeField] private WeaponController weaponController;

    [Header("Groggy")]
    [SerializeField] private float groggyDuration = 5f;
    [SerializeField] private float invincibilityDuration = 2f;
    private bool isGroggy = false;
    private BoxCollider2D playerCollider;

    [Header("Move Area")]
    [SerializeField]
    private AreaData moveArea;

    [Header("Info Data Name")]
    [SerializeField] protected PlayerDataName addressableName;

    private PlayerInputActions playerInput;
    private InputAction moveAction;

    [SerializeField] private PlayerType playerType;
    private PlayerMouseHandler mouseHandler;

    public float axisX { private set; get; }
    public float axisY { private set; get; }


    private MonsterManager.OnPlayerStateUpdate onPlayerStateUpdate;
    private SpineTest spine;

    protected new void Awake()
    {
        base.Awake();
        playerCollider = GetComponent<BoxCollider2D>();
        spine = GetComponent<SpineTest>();

        SODataLoader.Instance.LoadSO<CharacterData>(addressableName.ToString(), so =>
        {
            CharacterData data = so;

            if (data != null)
            {
                OnCharacterDataLoaded(data);
            }
        });

        playerInput = new PlayerInputActions();
        mouseHandler = GetComponent<PlayerMouseHandler>();
        ActivePlayerInput();
    }


    private void ActivePlayerInput()
    {
        switch (playerType)
        {
            case PlayerType.Player1:
                playerInput.Player1.Enable();
                Debug.Log("[Player1] : Active");

                //Move Callback
                playerInput.Player1.Move.started += callback => { ChangeState(StateType.Move); };
                playerInput.Player1.Move.performed += callback => Move(Vector2.zero);
                playerInput.Player1.Move.canceled += callback => { ChangeState(StateType.Idle); StopMove(); if (spine != null) spine.TestPlayIdleSpine(); };

                playerInput.Player1.Attack.started += callback => { OnAttack?.Invoke(); ChangeState(StateType.Attack); };

                moveAction = playerInput.FindAction("Move");

                onPlayerStateUpdate += MonsterManager.Instance.PlayerStateUpdate;
                break;
            case PlayerType.Player2:
                playerInput.Player2.Enable();
                Debug.Log("[Player2] : Active");

                //늑대 움직일 때 워크 스테이션에 들어가는지 검사용
                OnMoveAction = WolfWorkStation.Instance.CheckAreaEnter;

                //Move Callback
                playerInput.Player2.Move.started += callback => { ChangeState(StateType.Move); };
                playerInput.Player2.Move.performed += callback => { Move(Vector2.zero); };
                playerInput.Player2.Move.canceled += callback => { ChangeState(StateType.Idle); StopMove(); if (spine != null) spine.TestPlayIdleSpine(); };

                playerInput.Player2.Interact.started += callback => { OnInteractive?.Invoke(); };

                moveAction = playerInput.FindAction("Move");

                if (mouseHandler)
                {
                    mouseHandler.SetupMouse(this);
                }
                break;
        }
    }


    private Vector2 input;
    private Action<Vector2> OnMoveAction;

    void FixedUpdate()
    {
        if (axisX == 0.0f && axisY == 0.0f) return;

        MoveCharacter(moveArea.GetBounds(), input, OnMoveAction);
    }

    protected override void Idle()
    {
        base.Idle();

        axisX = 0.0f;
        axisY = 0.0f;
    }

    public override void TakeDamage(int amount)
    {
        if (isGroggy) return;

        base.TakeDamage(amount);

        if (isDead)
        {
            // 그로기 상태 진입
            isGroggy = true;
            onPlayerStateUpdate?.Invoke(true);
            OnPlayerDead?.Invoke();
            LockMovement();

            // 충돌 판정 비활성화
            if (playerCollider != null)
            {
                playerCollider.enabled = false;
            }

            StartCoroutine(RespawnCoroutine(groggyDuration));
        }
    }
    public bool IsGroggy() => isGroggy;

    public void OnGameOver()
    {
        //OnPlayerDead?.Invoke();
        LockMovement();
    }

    private void UnlockMovement()
    {
        switch (playerType)
        {
            case PlayerType.Player1:
                playerInput.Player1.Enable();
                break;
            case PlayerType.Player2: //2번은 죽지 않음
                playerInput.Player2.Enable();
                break;
        }
    }

    private System.Collections.IEnumerator RespawnCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        isGroggy = false;
        OnPlayerRespawn?.Invoke();
        currentHP = info.maxHp;
        ChangeState(StateType.Idle);
        UnlockMovement();

        onPlayerStateUpdate?.Invoke(false);
        playerCollider.enabled = true;

        StartCoroutine(GroggyCoroutine(invincibilityDuration));
    }

    private System.Collections.IEnumerator GroggyCoroutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            //그로기 상태 출력
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
    }

    private void LockMovement()
    {
        switch (playerType)
        {
            case PlayerType.Player1:
                playerInput.Player1.Disable();
                break;
            case PlayerType.Player2:
                playerInput.Player2.Disable();
                break;
        }
    }

    protected override void Attack()
    {
        base.Attack();
    }

    protected new void Move(Vector2 dir)
    {
        //base.Move(dir);

        input = moveAction.ReadValue<Vector2>();

        axisX = input.x;
        axisY = input.y;

        if (input.magnitude > 0)
        {
            if (spine != null) spine.TestPlayRunSpine();

        }

        if (axisX != 0.0f)
        {
            Flip(axisX > 0);
            if (weaponController != null) weaponController.Flip(axisX > 0);
        }
    }

    protected override void Dead()
    {
        base.Dead();
    }

    public PlayerInputActions GetPlayerInput() => playerInput;
}
