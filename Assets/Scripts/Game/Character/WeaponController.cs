using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Weapon weapon;

    [SerializeField] private Transform weaponPivot;

    [Header("Spine Skeleton Target")]
    [SerializeField] private Spine.Unity.SkeletonAnimation skeletonAni;

    private SpriteRenderer spriteRenderer;

    public bool isAllowAttack { private set; get; }

    private void InitAllowAttack() { isAllowAttack = true; }
    public bool GetAllowAttack() { return isAllowAttack; }

    public bool IsEquip() => weapon != null;

    private bool isFirst = true;

    public delegate bool IsAllowAttack();
    public IsAllowAttack isAllowAttackEvent;

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

    public void SetupWeapon(Weapon weapon)
    {
        if (this.weapon) this.weapon.DestroyWeapon();
        this.weapon = weapon;

        this.weapon.transform.SetParent(weaponPivot);

        // this.weapon.transform.localPosition = Vector3.zero;
        // this.weapon.transform.localRotation = Quaternion.identity;

        var boneFollower = weapon.gameObject.AddComponent<Spine.Unity.BoneFollower>();
        spriteRenderer = weapon.GetComponent<SpriteRenderer>();
        boneFollower.SkeletonRenderer = skeletonAni;
        boneFollower.boneName = "sword";
        boneFollower.followBoneRotation = true;
        boneFollower.followLocalScale = true;
        boneFollower.followSkeletonFlip = false;

        isAllowAttack = true;
        spriteRenderer.sortingOrder = 0;

        boneFollower.followLocalScale = false;
        isFirst = true;

        weapon.InitEvent(GetAllowAttack);

        Flip(false);

        isFirst = false;
    }

    public void SubDurability()
    {
        if (weapon == null) return;
        if (weapon.DecreaseDurability() == 0) weapon = null;

        Invoke(nameof(InitAllowAttack), 1f);
    }

    public void Flip(bool isRight)
    {
        if (weapon == null) return;

        Vector3 weaponScale = weapon.transform.localScale;
        if (isFirst) weaponScale.x = isRight ? Mathf.Abs(weaponScale.x) : -Mathf.Abs(weaponScale.x);
        weaponScale.y = isRight ? Mathf.Abs(weaponScale.y) : -Mathf.Abs(weaponScale.y);
        weapon.transform.localScale = weaponScale;
    }
}
