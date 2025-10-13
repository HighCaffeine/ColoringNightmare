using System;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Assertions.Must;
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
    [SerializeField] private WeaponController weaponController;

    // 신규 추가: 스킬 컨트롤러 연결
    [Header("Skill")]
    [SerializeField] private SkillController skillController;

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


    [SerializeField][Header("NoneWeapon")] private Transform noneWeaponPivot;
    [SerializeField] private PlayerNoneWeapon playerNoneWeapon;

    [SerializeField] private EffectVisualData playerNoneEffect;
    private EffectController effectController;


    public float axisX { private set; get; }
    public float axisY { private set; get; }
    public int GetHP() { return currentHP; }
    public int GetMaxHP() { return info.maxHp; }


    private MonsterManager.OnPlayerStateUpdate onPlayerStateUpdate;
    private SpineTest spine;

    private SkeletonAnimation skeletonAnimation;

    protected new void Awake()
    {
        base.Awake();
        playerCollider = GetComponent<BoxCollider2D>();
        effectController = GetComponent<EffectController>();
        spine = GetComponent<SpineTest>();
        skeletonAnimation = GetComponent<SkeletonAnimation>();

        // SkillController 초기화 (같은 오브젝트에 있다고 가정)
        if (skillController == null)
        {
            statusEffectManager = GetComponent<StatusEffectManager>();
            skillController = GetComponent<SkillController>();
        }


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

                // Attack 호출 시, ChangeState(Attack) -> Attack() -> SkillController.UseSkill() 호출
                playerInput.Player1.Attack.started += callback => { if (isGroggy || !CanAttack()) return; ChangeState(StateType.Attack); OnAttack?.Invoke(); };

                moveAction = playerInput.FindAction("Move");

                onPlayerStateUpdate += MonsterManager.Instance.PlayerStateUpdate;
                break;
            case PlayerType.Player2:
                playerInput.Player2.Enable();
                Debug.Log("[Player2] : Active");

                //늑대 움직일 때 워크 스테이션에 들어가는지 검사용
                OnMoveAction = WolfWorkStation.Instance.CheckAreaEnter;

                //Move Callback
                // playerInput.Player2.Move.started += callback => { ChangeState(StateType.Move); };
                // playerInput.Player2.Move.performed += callback => { Move(Vector2.zero); };
                // playerInput.Player2.Move.canceled += callback => { ChangeState(StateType.Idle); StopMove(); if (spine != null) spine.TestPlayIdleSpine(); };
                playerInput.Player2.Mouse.started += callback => { OnMouseClick(); };
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
            if (isDead) return;
            input = moveAction.ReadValue<Vector2>();
            if (input.x == 0.0f && input.y == 0.0f)
            {
                rigid.linearVelocity = Vector2.zero;
                return;
            }
            MoveCharacter(moveArea.GetBounds(), input, OnMoveAction);
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

        if (!moveArea.GetBounds().Contains(newPos)) return;

        targetPos = newPos;
        Vector2 moveDirection = (targetPos - (Vector2)transform.position).normalized;
        Flip(moveDirection.x < 0);

        isMouseMoving = true;
        ChangeState(StateType.Move);


        if (spine != null) spine.TestPlayRunSpine();
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
        if (!isAllowDamaged) return;

        if (isGroggy) return;

        Debug.Log($"[TakeDamage]{transform.name} : {amount} {currentHP}");

        currentHP -= amount;

        if (currentHP <= 0)
        {
            currentHP = 0;
        }


        if (isDead)
        {
            gameObject.layer = LayerMask.NameToLayer("IgnoreHit");

            skeletonAnimation.enabled = false;
            playerCollider.enabled = false;
            rigid.simulated = false;


            rigid.linearVelocity = Vector2.zero;

            StartCoroutine(RespawnCoroutine(groggyDuration));

            // 그로기 상태 진입
            isGroggy = true;
            onPlayerStateUpdate?.Invoke(true);
            OnPlayerDead?.Invoke();
            LockMovement();
        }

        OnDamaged?.Invoke();
        Invoke(nameof(InitAllowDamaged), damageDelay);
    }

    private void InitAllowDamaged() { isAllowDamaged = true; }
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
        currentHP = info.maxHp;
        OnPlayerRespawn?.Invoke();
        ChangeState(StateType.Idle);
        UnlockMovement();

        onPlayerStateUpdate?.Invoke(false);

        gameObject.layer = LayerMask.NameToLayer("Player");
        skeletonAnimation.enabled = true;
        playerCollider.enabled = true;
        rigid.simulated = true;

        SetDamageImmune(true);

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

        SetDamageImmune(false);
    }

    private void LockMovement()
    {
        // if (rigid == null || playerCollider == null) return;

        // rigid.linearVelocity = Vector2.zero;
        // rigid.angularVelocity = 0f;
        // rigid.simulated = false;
        // playerCollider.enabled = false;

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
        if (!CanAttack()) return;

        base.Attack();

        if (spine != null)
        {
            spine.TestPlayAttackSpine();
        }

        if (!weaponController.IsEquip())
        {
            playerNoneWeapon.OnResetAttack();
            effectController.NoneWeapon(playerNoneEffect);
            //noneWeaponPivot.gameObject.SetActive(true);
        }
        else
        {
            //noneWeaponPivot.gameObject.SetActive(false);
            if (weaponController != null && skillController != null)
            {
                weaponController.CurrentColliderSetActive(true);

                SkillData currentSkillData = weaponController.GetEquippedWeaponSkillData();
                skillController.UseSkill(currentSkillData.colorType);
            }
        }
    }


    protected new void Move(Vector2 dir)
    {
        //base.Move(dir);
        if (isGroggy) return;
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
