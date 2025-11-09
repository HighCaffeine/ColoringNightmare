using Spine;
using Spine.Unity;
using UnityEngine;

public class PlayerSpineManager : MonoBehaviour
{
    public enum WeaponType
    {
        Sword,
        Axe,
        Spear,
        Groggy
    }

    public enum AniName
    {
        // Sword (sheep2_1.json)
        idle,
        run,
        attack1,            // (Sword 일반 공격으로 추정)
        attack2,            // (Sword 더블히트로 추정)
        attack_sword,       // (이 이름일 수도 있음)
        attack_doublehit,   // (이 이름일 수도 있음)

        // Axe (Base: sheep2_3.json / Attack: axe_attack.zip)
        idle_axe,
        run_axe,
        attack_axe,         // (AxeAttack_GO에 있을 애니메이션)

        // Spear (sheep2_3.json)
        idle_spear,
        run_spear,
        attack_spear,

        // Groggy (sheep2_groggy.json)
        Groggy,
        Groggy_Revive
    }

    public enum SpineEventName
    {
        AttackEffect
    }

    [Header("스파인 캐릭터 (총 4개)")]
    public SkeletonAnimation swordSkeleton;
    public SkeletonAnimation axeSpearBaseSkeleton; // [이름변경] (axe_idle/run, spear 전부)
    public SkeletonAnimation axeAttackSkeleton;    // [신규] (axe_attack 전용)
    public SkeletonAnimation groggySkeleton;

    [Header("Dependencies")]
    [SerializeField] private SkillController skillController;
    private PlayerController playerController;

    private SkeletonAnimation currentSkeleton;
    private WeaponType currentWeapon;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();

        if (swordSkeleton != null) swordSkeleton.AnimationState.Event += HandleSpineEvent;
        if (axeSpearBaseSkeleton != null) axeSpearBaseSkeleton.AnimationState.Event += HandleSpineEvent;
        if (axeAttackSkeleton != null) axeAttackSkeleton.AnimationState.Event += HandleSpineEvent;
        if (groggySkeleton != null) groggySkeleton.AnimationState.Event += HandleSpineEvent;
    }

    void Start()
    {
        ChangeWeapon(WeaponType.Sword);
    }

    /// <summary>
    /// 스켈레톤 오브젝트를 교체하는 함수
    /// </summary>
    private void SwitchCharacter(SkeletonAnimation targetSkeleton)
    {
        // 모든 스켈레톤을 끈다
        swordSkeleton?.gameObject.SetActive(false);
        axeSpearBaseSkeleton?.gameObject.SetActive(false);
        axeAttackSkeleton?.gameObject.SetActive(false); // [추가]
        groggySkeleton?.gameObject.SetActive(false);

        // 타겟만 켠다
        currentSkeleton = targetSkeleton;
        if (currentSkeleton != null)
        {
            currentSkeleton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 무기/상태를 교체합니다.
    /// </summary>
    public void ChangeWeapon(WeaponType newWeapon)
    {
        currentWeapon = newWeapon;

        if (newWeapon == WeaponType.Sword)
        {
            SwitchCharacter(swordSkeleton);
            PlaySpine(AniName.idle, true);
        }
        else if (newWeapon == WeaponType.Axe)
        {
            // [수정] 도끼는 'AxeSpearBase' 스켈레톤을 사용합니다.
            SwitchCharacter(axeSpearBaseSkeleton);
            PlaySpine(AniName.idle_axe, true);
        }
        else if (newWeapon == WeaponType.Spear)
        {
            SwitchCharacter(axeSpearBaseSkeleton);
            PlaySpine(AniName.idle_spear, true);
        }
        else if (newWeapon == WeaponType.Groggy)
        {
            SwitchCharacter(groggySkeleton);
        }
    }

    // --- 그로기 ---
    public void PlayGroggySequence()
    {
        playerController.IsAttacking = true;
        ChangeWeapon(WeaponType.Groggy);

        var state = groggySkeleton.AnimationState;
        TrackEntry entry = state.SetAnimation(0, AniName.Groggy.ToString(), false);
        entry.Complete += OnGroggyComplete;
    }

    public void PlayGroggyRevive()
    {
        if (currentWeapon != WeaponType.Groggy) return;
        playerController.IsAttacking = true;
        var state = groggySkeleton.AnimationState;
        TrackEntry entry = state.SetAnimation(0, AniName.Groggy_Revive.ToString(), false);
        entry.Complete += OnGroggyComplete;
    }

    private void OnGroggyComplete(TrackEntry trackEntry)
    {
        trackEntry.Complete -= OnGroggyComplete;
        playerController.IsAttacking = false;
        ChangeWeapon(WeaponType.Sword); // 기본 무기(검)로 복귀
    }

    // --- 상태별 애니메이션 재생 ---

    public void TestPlayAttackSpine()
    {
        if (playerController.IsAttacking || currentWeapon == WeaponType.Groggy) return;
        playerController.IsAttacking = true;

        if (currentWeapon == WeaponType.Sword)
        {
            // 'attack1' 또는 'attack_sword' 사용 (json 파일 확인 필요)
            PlaySpine(AniName.attack1, false, OnAttackComplete);
        }
        else if (currentWeapon == WeaponType.Axe)
        {
            // [수정] 도끼 공격은 별도 함수로 처리
            PlayAxeAttack();
        }
        else if (currentWeapon == WeaponType.Spear)
        {
            PlaySpine(AniName.attack_spear, false, OnAttackComplete);
        }
        else
        {
            playerController.IsAttacking = false;
        }
    }

    /// <summary>
    /// [신규] 도끼 공격 전용 함수 (스켈레톤 교체 발생)
    /// </summary>
    private void PlayAxeAttack()
    {
        // 1. 공격용 스켈레톤으로 교체
        SwitchCharacter(axeAttackSkeleton);
        currentWeapon = WeaponType.Axe; // (상태 유지를 위해)

        // 2. 도끼 공격 재생
        // (AniName에 'attack_axe'가 있어야 함)
        PlaySpine(AniName.attack_axe, false, onComplete: () =>
        {
            // 3. 공격이 끝나면,
            playerController?.ResetAttackCooldown();

            // 4. 다시 Axe 기본 상태(idle)로 복귀
            // (ChangeWeapon이 'axeSpearBaseSkeleton'로 교체하고 'idle_axe'를 재생함)
            ChangeWeapon(WeaponType.Axe);
        });
    }

    // (Sword, Spear 공격 완료 시 호출되는 일반 콜백)
    private void OnAttackComplete()
    {
        playerController?.ResetAttackCooldown();
        TestPlayIdleSpine(); // 현재 무기에 맞는 Idle 재생
    }


    public void TestPlaySwordDoubleHit()
    {
        if (playerController.IsAttacking || currentWeapon != WeaponType.Sword) return;
        playerController.IsAttacking = true;

        // 'attack2' 또는 'attack_doublehit' 사용
        PlaySpine(AniName.attack2, false, OnAttackComplete);
    }


    public void TestPlayRunSpine()
    {
        if (playerController.IsAttacking || currentWeapon == WeaponType.Groggy) return;

        AniName runAni;
        if (currentWeapon == WeaponType.Sword)
            runAni = AniName.run;
        else if (currentWeapon == WeaponType.Axe)
            runAni = AniName.run_axe;
        else if (currentWeapon == WeaponType.Spear)
            runAni = AniName.run_spear;
        else
            return;

        if (currentSkeleton.AnimationName != runAni.ToString())
        {
            PlaySpine(runAni, true);
        }
    }

    public void TestPlayIdleSpine()
    {
        if (playerController.IsAttacking || currentWeapon == WeaponType.Groggy) return;

        AniName idleAni;
        if (currentWeapon == WeaponType.Sword)
            idleAni = AniName.idle;
        else if (currentWeapon == WeaponType.Axe)
            idleAni = AniName.idle_axe;
        else if (currentWeapon == WeaponType.Spear)
            idleAni = AniName.idle_spear;
        else
            return;

        if (currentSkeleton.AnimationName != idleAni.ToString())
        {
            PlaySpine(idleAni, true);
        }
    }


    private void PlaySpine(AniName aniName, bool isLoop, System.Action onComplete = null)
    {
        if (currentSkeleton == null)
        {
            Debug.LogError("Current Skeleton is NULL!");
            onComplete?.Invoke();
            return;
        }

        string animationNameString = aniName.ToString();

        // Groggy 애니메이션 이름 변환 (groggy.json에 "Groggy"로 되어있음)
        // (v3에서 수정된 AniName enum을 쓴다면 이 부분은 필요 없을 수 있으나, 안전을 위해 둠)
        if (animationNameString == "groggy_1") animationNameString = "Groggy";
        if (animationNameString == "groggy_4") animationNameString = "Groggy_Revive";


        var ani = currentSkeleton.Skeleton.Data.FindAnimation(animationNameString);
        if (ani == null)
        {
            Debug.LogError($"'{currentSkeleton.name}'에서 '{animationNameString}' 애니메이션을 찾을 수 없습니다.");
            onComplete?.Invoke();
            return;
        }

        TrackEntry entry = currentSkeleton.AnimationState.SetAnimation(0, ani, isLoop);

        if (!isLoop && onComplete != null)
        {
            entry.Complete += OnAniComplete;
            void OnAniComplete(TrackEntry tr)
            {
                tr.Complete -= OnAniComplete;
                onComplete();
            }
        }
    }

    private void HandleSpineEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (e.Data.Name == SpineEventName.AttackEffect.ToString())
        {
            skillController?.OnAnimationHit();
        }
    }
}