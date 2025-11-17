using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class SpineTest : MonoBehaviour
{
    public enum SpineEvent
    {
        None,
        AttackEffect,
    }

    public enum AniName
    {
        //공용
        attack1,
        idle,
        walk,

        //Sheep
        idle_axe,
        attack_axe2,
        run_axe,

        idle_spear,
        attack_spear,
        run_spear,

        idle_normal,
        attack_normal,
        run_normal,

        Groggy,
        Groggy2,
        Groggy3,
        Groggy4,    //respawn
    }

    public SkeletonAnimation skeleton;

    [Header("Dependencies")]
    [SerializeField] private SkillController skillController;
    private PlayerController playerController;
    private AniName CurrentAttackAnimation => (!isSheep) ? AniName.attack1 : currentAttackAnimation;
    private AniName CurrentWalkAnimation => (!isSheep) ? AniName.walk : currentWalkAnimation;
    private AniName CurrentIdleAnimation => (!isSheep) ? AniName.idle : currentIdleAnimation;

    private AniName currentAttackAnimation = AniName.attack_normal;
    private AniName currentWalkAnimation = AniName.run_normal;
    private AniName currentIdleAnimation = AniName.idle_normal;

    [SerializeField] private bool isSheep = false;

    void Awake()
    {
        if (skeleton == null) skeleton = GetComponent<SkeletonAnimation>();
        playerController = GetComponent<PlayerController>();
    }

    void Start()
    {
        if (skeleton != null) skeleton.AnimationState.Event += HandleSpineEvent;
        TestPlayIdleSpine();
    }

    public void TestPlayAttackSpine()
    {
        if (playerController.IsAttacking) return;
        playerController.IsAttacking = true;

        PlaySpine(CurrentAttackAnimation, false, onComplete: () =>
        {
            playerController?.ResetAttackCooldown();
            TestPlayIdleSpine();
        });
    }

    public void SetAttackAnimationByWeaponType(WeaponManager.WeaponType type)
    {
        switch (type)
        {
            case WeaponManager.WeaponType.Sword:
                currentAttackAnimation = AniName.attack_normal;
                currentWalkAnimation = AniName.run_normal;
                currentIdleAnimation = AniName.idle_normal;
                break;
            case WeaponManager.WeaponType.Axe:
                currentAttackAnimation = AniName.attack_axe2;
                currentWalkAnimation = AniName.run_axe;
                currentIdleAnimation = AniName.idle_axe;
                break;
            case WeaponManager.WeaponType.Spear:
                currentAttackAnimation = AniName.attack_spear;
                currentWalkAnimation = AniName.run_spear;
                currentIdleAnimation = AniName.idle_spear;
                break;
            default:
                currentAttackAnimation = AniName.attack1; // 기본값
                break;
        }
    }
    public void ClearAttackAnimation()
    {
        currentAttackAnimation = AniName.attack1;
    }

    public void TestPlayRunSpine()
    {
        if (playerController.IsAttacking) return;

        if (skeleton.AnimationName != CurrentWalkAnimation.ToString())
        {
            PlaySpine(CurrentWalkAnimation, true);
        }
    }

    public void TestPlayIdleSpine()
    {
        if (playerController.IsAttacking) return;

        if (skeleton.AnimationName != CurrentIdleAnimation.ToString())
        {
            PlaySpine(CurrentIdleAnimation, true);
        }
    }

    private void PlaySpine(AniName aniName, bool isLoop, System.Action onComplete = null)
    {
        if (skeleton == null) return;
        var ani = skeleton.Skeleton.Data.FindAnimation(aniName.ToString());
        if (ani == null)
        {
            Debug.LogError($"Animation '{aniName}' not found");
            return;
        }
        TrackEntry entry = skeleton.AnimationState.SetAnimation(0, ani, isLoop);
        if (!isLoop && onComplete != null)
        {
            entry.Complete += _ => onComplete();
        }
    }

    private void HandleSpineEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (e.Data.Name == SpineEvent.AttackEffect.ToString())
        {
            skillController?.OnAnimationHit();
        }
    }
}