using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Weapon weapon;
    [SerializeField] private Transform weaponPivot;

    [Header("Spine Skeleton Target")]
    [SerializeField] private Spine.Unity.SkeletonAnimation skeletonAni;

    private SpriteRenderer spriteRenderer;

    public bool IsEquip() => weapon != null;

    public SkillData GetEquippedWeaponSkillData()
    {
        if (weapon != null)
        {
            return weapon.GetSKillData();
        }
        return null;
    }

    public WeaponInkData GetEquippedWeaponInkData()
    {
        if (weapon != null)
        {
            return weapon.GetInkData();
        }
        return null;
    }

    public void SetupWeapon(Weapon newWeapon)
    {
        if (this.weapon) this.weapon.DestroyWeapon();
        this.weapon = newWeapon;

        this.weapon.transform.SetParent(weaponPivot, false);
        this.weapon.transform.localPosition = Vector3.zero;
        this.weapon.transform.localRotation = Quaternion.identity;
        this.weapon.transform.localScale = Vector3.one;

        var boneFollower = weapon.gameObject.AddComponent<Spine.Unity.BoneFollower>();
        spriteRenderer = weapon.GetComponent<SpriteRenderer>();
        boneFollower.SkeletonRenderer = skeletonAni;
        boneFollower.boneName = "sword";
        boneFollower.followBoneRotation = true;
        boneFollower.followLocalScale = false;
        boneFollower.followSkeletonFlip = true;
        spriteRenderer.sortingOrder = 0;

        CurrentColliderSetActive(false);
    }

    public Weapon GetEquippedWeapon()
    {
        return weapon;
    }

    public void CurrentColliderSetActive(bool activate)
    {
        if (weapon != null)
            weapon.SetActiveCollider(activate);
    }

    public void SubDurability()
    {
        if (weapon == null) return;
        if (weapon.DecreaseDurability() <= 0) weapon = null;
    }

    public void Flip(bool isRight)
    {
    }
}
