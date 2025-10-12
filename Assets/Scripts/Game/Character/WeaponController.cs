using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Weapon weapon;

    [SerializeField] private Transform weaponPivot;

    [Header("Spine Skeleton Target")]
    [SerializeField] private Spine.Unity.SkeletonAnimation skeletonAni;

    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    public bool IsEquip() => weapon != null;

    private bool isFirst = true;

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

        spriteRenderer.sortingOrder = 0;

        boneFollower.followLocalScale = false;
        isFirst = true;

        Flip(false);

        isFirst = false;
    }

    public void SubDurability()
    {
        if (weapon.DecreaseDurability() == 0) weapon = null;
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
