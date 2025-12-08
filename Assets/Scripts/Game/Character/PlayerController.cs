using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Spine.Unity;

public enum PlayerType
{
    NONE,
    Player1,
    Player2,
}

public class PlayerController : Character
{
    [Header("UI References")]
    [SerializeField] private PlayerHPController hpController;

    [Header("TEST Event")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnPlayerDead;
    [SerializeField] private UnityEngine.Events.UnityEvent OnPlayerRespawn;

    [Header("Interactive")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnInteractive;

    [Header("Damaged")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnDamaged;
    [SerializeField] private float damageDelay = 2.0f;

    [Header("Attack")]
    [SerializeField] private GameObject punchHitboxObj;
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

    [Header("Groggy Spine Assets")]
    [SerializeField] private SkeletonDataAsset groggySkeletonData;
    [SerializeField] private Material groggyMaterial;

    private SkeletonDataAsset normalSkeletonData;
    private Material normalMaterial;

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
    private MeshRenderer meshRenderer;


    protected new void Awake()
    {
        base.Awake();
        playerCollider = GetComponent<BoxCollider2D>();
        spine = GetComponent<SpineTest>();
        meshRenderer = GetComponent<MeshRenderer>();

        SODataLoader.Instance.LoadSO<CharacterData>(addressableName.ToString(), so =>
        {
            if (so != null)
            {
                CharacterData cloneData = Instantiate(so);
                OnCharacterDataLoaded(cloneData);
            }
        });

        playerCollider = GetComponent<BoxCollider2D>();
        spine = GetComponent<SpineTest>();
        effectController = GetComponent<EffectController>();

        playerInput = new PlayerInputActions();
        mouseHandler = GetComponent<PlayerMouseHandler>();
        ActivePlayerInput();

        if (skeleton != null)
        {
            normalSkeletonData = skeleton.skeletonDataAsset;
            normalMaterial = meshRenderer.material;
        }
    }

    void Start()
    {
        if (hpController != null && info != null)
        {
            hpController.Init(1, 1);
        }
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
                //playerInput.Player2.Mouse.started += callback => { OnMouseClick(); };

                //playerInput.Player2.Mouse.started += callback => { WolfWorkStation.Instance.CheckAreaEnter(Mouse.current.position.ReadValue()); };

                mainCam = Camera.main;
                //playerInput.Player2.Interact.started += callback => { OnInteractive?.Invoke(); };
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

            int newSortingOrder = 11 + Mathf.RoundToInt(transform.position.y * -100f);
            meshRenderer.sortingOrder = newSortingOrder;

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

    public new void Flip(bool isRight)
    {
        base.Flip(isRight);
    }

    private void OnMousePerformed()
    {
        //늑대 마우스 영역 체크
    }

    private void PerformAttack()
    {
        if (isGroggy || IsAttacking) return;

        StartCoroutine(AttackRoutine());
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        ChangeState(StateType.Attack);
        OnAttack?.Invoke();

        float currentAnimSpeed = 1.0f;
        float finalWaitTime = 0.5f;

        if (weaponController != null) currentAnimSpeed = weaponController.GetAttackSpeed();
        if (skeleton != null) skeleton.timeScale = currentAnimSpeed;
        string soundName = SoundManager.Effect.SFX_Weapon_Attack_Light.ToString(); // 기본(노랑, 파랑)

        if (weaponController != null && weaponController.IsEquip())
        {
            var inkData = weaponController.GetEquippedWeapon()?.GetInkData();
            if (inkData != null)
            {
                if (inkData.inkData.color == ColorMixer.ColorType.Red || inkData.inkData.color == ColorMixer.ColorType.Red)
                {
                    soundName = SoundManager.Effect.SFX_Weapon_Attack_Heavy.ToString();
                }
            }
        }

        SoundManager.Instance.PlaySound(soundName, false);

        if (weaponController != null && weaponController.IsEquip())
        {
            weaponController.EnableHitbox();

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

            effectController?.NoneWeapon(playerNoneEffect); // 이펙트

            if (punchHitboxObj != null)
            {
                punchHitboxObj.SetActive(true); // 히트박스 켜기
                // 아주 짧은 순간만 켜서 판정 (0.1초)
                StartCoroutine(DisablePunchHitboxAfter(0.1f));
            }
        }

        if (skeleton != null)
        {
            yield return null;

            var currentTrack = skeleton.AnimationState.GetCurrent(0);

            if (currentTrack != null && currentTrack.Animation != null)
            {
                float animDuration = currentTrack.Animation.Duration / currentAnimSpeed;
                finalWaitTime = Mathf.Max(animDuration, finalWaitTime);
            }
        }

        yield return new WaitForSeconds(finalWaitTime);

        if (skeleton != null) skeleton.timeScale = 1.0f;

        ChangeState(StateType.Idle);
    }

    private System.Collections.IEnumerator DisablePunchHitboxAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (punchHitboxObj != null) punchHitboxObj.SetActive(false);
    }

    public void ResetAttackCooldown()
    {
        IsAttacking = false;
        weaponController.DisableHitbox();
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

        UpdateHP();

        OnDamaged?.Invoke();
        isAllowDamaged = false; // 짧은 시간 동안 무적

        Invoke(nameof(InitAllowDamaged), damageDelay);

        if (isDead)
        {
            isGroggy = true;
            onPlayerStateUpdate?.Invoke(true);
            OnPlayerDead?.Invoke();
            LockMovement(); // 이동 및 공격 입력 비활성화

            ChangeState(StateType.Dead);

            StopAllCoroutines();

            IsAttacking = false;

            if (WolfWorkStation.Instance != null)
            {
                WolfWorkStation.Instance.ForceExitAll();
            }

            // 충돌 판정 비활성화
            if (playerCollider != null)
            {
                playerCollider.enabled = false;
            }

            rigid.linearVelocity = Vector2.zero;
            if (skeleton != null && groggySkeletonData != null && meshRenderer != null && groggyMaterial != null)
            {
                skeleton.timeScale = 1.0f;
                skeleton.skeletonDataAsset = groggySkeletonData;
                meshRenderer.material = groggyMaterial;
                skeleton.Initialize(true);

                skeleton.AnimationState.AddAnimation(0, SpineTest.AniName.Groggy.ToString(), false, 0);
                skeleton.AnimationState.AddAnimation(0, SpineTest.AniName.Groggy2.ToString(), false, 0);
                skeleton.AnimationState.AddAnimation(0, SpineTest.AniName.Groggy3.ToString(), false, 0);
            }

            LockMovement();

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
                ResetAttackCooldown();
                break;
            case PlayerType.Player2:
                playerInput.Player2.Enable();
                break;
        }
    }

    public void UpdateHP()
    {
        hpController.Init(currentHP, info.maxHp);
    }

    private System.Collections.IEnumerator RespawnCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        isGroggy = false;
        currentHP = info.maxHp;
        OnPlayerRespawn?.Invoke();

        state = StateType.Idle;

        if (skeleton != null && normalSkeletonData != null && meshRenderer != null && normalMaterial != null)
        {
            skeleton.Initialize(true);
            skeleton.AnimationState.AddAnimation(0, SpineTest.AniName.Groggy4.ToString(), false, 0);
        }

        yield return new WaitForSeconds(1.333f);

        if (skeleton != null && normalSkeletonData != null && meshRenderer != null && normalMaterial != null)
        {
            skeleton.skeletonDataAsset = normalSkeletonData;
            meshRenderer.material = normalMaterial;
            skeleton.Initialize(true);
            skeleton.AnimationState.SetAnimation(0, SpineTest.AniName.idle_normal.ToString(), true);
        }

        ChangeState(StateType.Idle);
        UnlockMovement();

        onPlayerStateUpdate?.Invoke(false);
        playerCollider.enabled = true;

        SetDamageImmune(true);
        StartCoroutine(GroggyCoroutine(invincibilityDuration));

        UpdateHP();
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
        if (playerInput == null) return;
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

    public override void Heal(int amount)
    {
        base.Heal(amount); // 부모 클래스에서 체력 증가 처리

        // UI 갱신
        if (hpController != null)
        {
            hpController.UpdateHpGauge(currentHP, info.maxHp);
        }
    }

    protected override void Dead()
    {
        base.Dead();
    }

    public PlayerInputActions GetPlayerInput() => playerInput;
}