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
        idle,
        run,
        attack1,
        attack2,
        attack_sword,
        attack_doublehit,

        idle_axe,
        run_axe,
        attack_axe,
        idle_spear,
        run_spear,
        attack_spear,

        Groggy,
        Groggy_Revive
    }

    public enum SpineEventName
    {
        AttackEffect
    }

    [Header("스파인 캐릭터 (총 4개)")]
    public SkeletonAnimation swordSkeleton;
    public SkeletonAnimation axeSpearBaseSkeleton;
    public SkeletonAnimation axeAttackSkeleton;
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

    private void SwitchCharacter(SkeletonAnimation targetSkeleton)
    {
        swordSkeleton?.gameObject.SetActive(false);
        axeSpearBaseSkeleton?.gameObject.SetActive(false);
        axeAttackSkeleton?.gameObject.SetActive(false);
        groggySkeleton?.gameObject.SetActive(false);

        currentSkeleton = targetSkeleton;
        if (currentSkeleton != null)
        {
            currentSkeleton.gameObject.SetActive(true);
        }
    }

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
        ChangeWeapon(WeaponType.Sword);
    }

    public void TestPlayAttackSpine()
    {
        if (playerController.IsAttacking || currentWeapon == WeaponType.Groggy) return;
        playerController.IsAttacking = true;

        if (currentWeapon == WeaponType.Sword)
        {
            PlaySpine(AniName.attack1, false, OnAttackComplete);
        }
        else if (currentWeapon == WeaponType.Axe)
        {
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

    private void PlayAxeAttack()
    {
        SwitchCharacter(axeAttackSkeleton);
        currentWeapon = WeaponType.Axe;

        PlaySpine(AniName.attack_axe, false, onComplete: () =>
        {
            playerController?.ResetAttackCooldown();
            ChangeWeapon(WeaponType.Axe);
        });
    }

    private void OnAttackComplete()
    {
        isFirst = true;
        playerController?.ResetAttackCooldown();
        TestPlayIdleSpine();
    }


    public void TestPlaySwordDoubleHit()
    {
        if (playerController.IsAttacking || currentWeapon != WeaponType.Sword) return;
        playerController.IsAttacking = true;

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

        if (animationNameString == "groggy_1") animationNameString = "Groggy";
        if (animationNameString == "groggy_4") animationNameString = "Groggy_Revive";


        var ani = currentSkeleton.Skeleton.Data.FindAnimation(animationNameString);
        if (ani == null)
        {
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
    private bool isFirst = true;

    private void HandleSpineEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (e.Data.Name == SpineEventName.AttackEffect.ToString())
        {
            if (!isFirst) return;
            isFirst = false;
            skillController?.OnAnimationHit();
        }
    }
}