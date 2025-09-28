using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Weapon weapon;

    [SerializeField] private Transform weaponPivot;

    [Header("Spine Skeleton Target")]
    [SerializeField] private Spine.Unity.SkeletonAnimation skeletonAni;

    private PlayerController playerController;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void SetupWeapon(Weapon weapon)
    {
        playerController.EquipWeapon(weapon);

        this.weapon = weapon;

        this.weapon.transform.SetParent(weaponPivot);

        // this.weapon.transform.localPosition = Vector3.zero;
        // this.weapon.transform.localRotation = Quaternion.identity;

        var boneFollower = weapon.gameObject.AddComponent<Spine.Unity.BoneFollower>();
        boneFollower.SkeletonRenderer = skeletonAni;
        boneFollower.boneName = "sword";
        boneFollower.followBoneRotation = true;
        boneFollower.followLocalScale = true;
        boneFollower.followSkeletonFlip = true;

        boneFollower.followLocalScale = false;
    }
}
