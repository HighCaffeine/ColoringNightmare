using UnityEngine;
using System.Collections;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Weapon weapon;
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private Spine.Unity.SkeletonAnimation skeletonAni;
    private SpriteRenderer spriteRenderer;

    [Header("Dependencies")]
    [SerializeField] private SkillController skillController;

    public bool IsEquip() => weapon != null;

    public BaseSkillLogic GetEquippedWeaponSkillData()
    {
        return weapon?.GetSkillLogic();
    }

    public Weapon GetEquippedWeapon()
    {
        return weapon;
    }

    public void SetupWeapon(Weapon newWeapon)
    {
        if (this.weapon) this.weapon.DestroyWeapon();
        this.weapon = newWeapon;

        this.weapon.transform.SetParent(weaponPivot, false);
        this.weapon.transform.localPosition = Vector3.zero;
        this.weapon.transform.localRotation = Quaternion.identity;
        //this.weapon.transform.localScale = Vector3.one;

        var boneFollower = weapon.gameObject.AddComponent<Spine.Unity.BoneFollower>();
        spriteRenderer = weapon.GetComponent<SpriteRenderer>();
        boneFollower.SkeletonRenderer = skeletonAni;
        boneFollower.boneName = "sword";
        boneFollower.followBoneRotation = true;
        boneFollower.followLocalScale = false;
        boneFollower.followSkeletonFlip = true;
        spriteRenderer.sortingOrder = 0;

        if (skillController != null && this.weapon != null)
        {
            skillController.SetCurrentSkill(this.weapon.GetSkillLogic());
        }

        CurrentColliderSetActive(false);
    }

    public void CurrentColliderSetActive(bool activate)
    {
        if (weapon != null)
            weapon.SetActiveCollider(activate);
    }

    public void ActivateHitboxForDuration(float duration)
    {
        StartCoroutine(HitboxCoroutine(duration));
    }
    private IEnumerator HitboxCoroutine(float duration)
    {
        CurrentColliderSetActive(true);
        yield return new WaitForSeconds(duration);
        CurrentColliderSetActive(false);
    }
    public void SubDurability()
    {
        if (weapon == null) return;
        if (weapon.DecreaseDurability() <= 0)
        {
            weapon = null;
            if (skillController != null)
            {
                skillController.SetCurrentSkill(null);
            }
        }
    }

    public void Flip(bool isRight)
    {
        // BoneFollower의 followSkeletonFlip이 true이므로 별도 처리 불필요
    }
}