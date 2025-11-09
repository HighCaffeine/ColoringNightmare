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

    [Header("Damaged")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnDamaged;
    [SerializeField] private float damageDelay = 2.0f;

    [Header("Attack")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnAttack;

    [Header("Weapon")]
    [Header("Weapon")]
    [SerializeField] private WeaponController weaponController;

    [Header("Skill")]
    [SerializeField] private SkillController skillController;

    [Header("NoneWeapon")]
    [SerializeField] private EffectVisualData playerNoneEffect;
    private EffectController effectController;

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
    public int GetHP() { return currentHP; }
    public int GetMaxHP() { return info.maxHp; }
    public bool IsAttacking { get; set; } = false;

    private MonsterManager.OnPlayerStateUpdate onPlayerStateUpdate;
    private SpineTest spine;


    protected new void Awake()
    {
        base.Awake();
        playerCollider = GetComponent<BoxCollider2D>();
        spine = GetComponent<SpineTest>();

        SODataLoader.Instance.LoadSO<CharacterData>(addressableName.ToString(), so =>
        {
            if (so != null)
            {
                OnCharacterDataLoaded(so);
            }
        });

        playerCollider = GetComponent<BoxCollider2D>();
        spine = GetComponent<SpineTest>();
        effectController = GetComponent<EffectController>();

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

                playerInput.Player1.Move.started += callback => { ChangeState(StateType.Move); };
                playerInput.Player1.Move.performed += callback => Move(Vector2.zero);
                playerInput.Player1.Move.canceled += callback => { ChangeState(StateType.Idle); StopMove(); if (spine != null) spine.TestPlayIdleSpine(); };

                // 공격 입력을 받으면 PerformAttack 메서드를 호출
                playerInput.Player1.Attack.started += callback => { PerformAttack(); };

                moveAction = playerInput.FindAction("Move");
                onPlayerStateUpdate += MonsterManager.Instance.PlayerStateUpdate;
                break;
            case PlayerType.Player2:
                playerInput.Player2.Enable();
                //OnMoveAction = WolfWorkStation.Instance.CheckAreaEnter;
                playerInput.Player2.Mouse.started += callback => { OnMouseClick(); };

                //playerInput.Player2.Mouse.performed += callback => { WolfWorkStation.Instance.CheckAreaEnter(Mouse.current.position.ReadValue()); };

                mainCam = Camera.main;
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
    private Vector2 targetPos;
    private bool isMouseMoving;
    private Camera mainCam;

    void FixedUpdate()
    {
        if (playerType == PlayerType.Player1)
        {
            if (IsAttacking || isGroggy || isDead)
            {
                StopMove();
                return;
            }

            Vector2 input = moveAction.ReadValue<Vector2>();

            if (input.magnitude > 0.1f)
            {
                ChangeState(StateType.Move);
                if (spine != null) spine.TestPlayRunSpine();
            }
            else
            {
                ChangeState(StateType.Idle);
                if (spine != null) spine.TestPlayIdleSpine();
            }

            MoveCharacter(moveArea.GetBounds(), input, OnMoveAction);

            if (input.x != 0.0f)
            {
                Flip(input.x > 0);
                if (weaponController != null) weaponController.Flip(input.x > 0);
                if (effectController != null) effectController.Flip(input.x > 0);
            }
        }
        else if (playerType == PlayerType.Player2 && isMouseMoving)
        {
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            transform.position = Vector2.MoveTowards(transform.position, targetPos, info.speed * Time.fixedDeltaTime);
            OnMoveAction?.Invoke(transform.position);

            if (Vector2.Distance(transform.position, targetPos) < 0.1f)
            {
                isMouseMoving = false;
                ChangeState(StateType.Idle);
                if (spine != null) spine.TestPlayIdleSpine();
            }
        }
    }

    private void OnMouseClick()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if (WolfWorkStation.Instance.IsOnInteractive) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 newPos = mainCam.ScreenToWorldPoint(mousePos);

        if (!moveArea.GetBounds().Contains(newPos))
        {
            return;
        }

        isMouseMoving = true;
        targetPos = newPos;

        Vector2 moveDirection = (targetPos - (Vector2)transform.position).normalized;
        Flip(moveDirection.x < 0);

        ChangeState(StateType.Move);
        if (spine != null) spine.TestPlayRunSpine();
    }

    private void OnMousePerformed()
    {
        //늑대 마우스 영역 체크
    }

    private void PerformAttack()
    {
        if (isGroggy || IsAttacking) return;

        ChangeState(StateType.Attack);
        OnAttack?.Invoke();

        if (weaponController != null && weaponController.IsEquip())
        {
            if (skillController != null)
            {
                BaseSkillLogic currentSkillLogic = weaponController.GetEquippedWeaponSkillData();
                if (currentSkillLogic != null)
                {
                    skillController.SetCurrentSkill(currentSkillLogic);
                    skillController.UseSkill();
                }
            }
        }
        else
        {
            effectController?.NoneWeapon(playerNoneEffect);
        }
    }

    public void ResetAttackCooldown()
    {
        IsAttacking = false;
    }

    protected override void Attack()
    {
        base.Attack();
    }

    protected override void Idle()
    {
        base.Idle();
        axisX = 0.0f;
        axisY = 0.0f;
    }

    private bool isAllowDamaged = true;

    public override void TakeDamage(int amount)
    {
        if (!isAllowDamaged || isGroggy) return;

        base.TakeDamage(amount);

        OnDamaged?.Invoke();
        isAllowDamaged = false; // 짧은 시간 동안 무적

        Invoke(nameof(InitAllowDamaged), damageDelay);

        if (isDead)
        {
            isGroggy = true;
            onPlayerStateUpdate?.Invoke(true);
            OnPlayerDead?.Invoke();
            LockMovement(); // 이동 및 공격 입력 비활성화

            // 충돌 판정 비활성화
            if (playerCollider != null)
            {
                playerCollider.enabled = false;
            }

            rigid.linearVelocity = Vector2.zero; // 움직임 즉시 정지

            // 부활 코루틴 시작
            StartCoroutine(RespawnCoroutine(groggyDuration));
        }
    }
    private void InitAllowDamaged() { isAllowDamaged = true; }
    public bool IsGroggy() => isGroggy;

    public void OnGameOver()
    {
        LockMovement();
    }

    private void UnlockMovement()
    {
        switch (playerType)
        {
            case PlayerType.Player1:
                playerInput.Player1.Enable();
                break;
            case PlayerType.Player2:
                playerInput.Player2.Enable();
                break;
        }
    }

    private System.Collections.IEnumerator RespawnCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        isGroggy = false;
        currentHP = info.maxHp;
        OnPlayerRespawn?.Invoke();

        ChangeState(StateType.Idle);
        UnlockMovement();

        onPlayerStateUpdate?.Invoke(false);
        playerCollider.enabled = true;

        SetDamageImmune(true);
        StartCoroutine(GroggyCoroutine(invincibilityDuration));
    }

    private System.Collections.IEnumerator GroggyCoroutine(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        SetDamageImmune(false);
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

        ChangeState(StateType.Idle);
        if (spine != null) spine.TestPlayIdleSpine();
    }

    protected new void Move(Vector2 dir)
    {
        if (IsAttacking) return;

        input = moveAction.ReadValue<Vector2>();
        axisX = input.x;
        axisY = input.y;
        if (input.magnitude > 0)
        {
            if (spine != null) spine.TestPlayRunSpine();
        }
    }

    protected override void Dead()
    {
        base.Dead();
    }

    public PlayerInputActions GetPlayerInput() => playerInput;
}