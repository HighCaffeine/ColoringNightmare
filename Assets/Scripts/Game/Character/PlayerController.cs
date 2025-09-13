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
    [Header("Move Area")]
    [SerializeField]
    private AreaData moveArea;

    private PlayerInputActions playerInput;
    private InputAction moveAction;

    [SerializeField] private PlayerType playerType;
    private PlayerMouseHandler mouseHandler;

    public float axisX { private set; get; }
    public float axisY { private set; get; }

    protected new void Awake()
    {
        base.Awake();

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
                playerInput.Player1.Move.performed += callback => Move();
                playerInput.Player1.Move.canceled += callback => { ChangeState(StateType.Idle); MoveCharacter(moveArea, Vector2.zero); };

                moveAction = playerInput.FindAction("Move");
                break;
            case PlayerType.Player2:
                playerInput.Player2.Enable();
                Debug.Log("[Player2] : Active");

                //Move Callback
                playerInput.Player2.Move.started += callback => { ChangeState(StateType.Move); };
                playerInput.Player2.Move.performed += callback => Move();
                playerInput.Player2.Move.canceled += callback => { ChangeState(StateType.Idle); MoveCharacter(moveArea, Vector2.zero); };

                moveAction = playerInput.FindAction("Move");

                if (mouseHandler)
                {
                    mouseHandler.SetupMouse(this);
                }
                break;
        }
    }


    private Vector2 input;

    protected new void Idle()
    {
        base.Idle();
    }

    protected new void Attack()
    {
        base.Attack();
    }

    protected new void Move()
    {
        base.Move();

        input = moveAction.ReadValue<Vector2>();

        axisX = input.x;
        axisY = input.y;

        MoveCharacter(moveArea, input);
    }

    protected new void Dead()
    {
        base.Dead();
    }

    public PlayerInputActions GetPlayerInput() => playerInput;
}
