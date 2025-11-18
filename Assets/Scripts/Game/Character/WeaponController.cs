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
    [SerializeField] private SpineTest spineTest;

    private GameObject weaponFollowerHolder; // 홀더 참조

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
        // 1. 기존 무기가 있다면 파괴
        if (this.weapon != null && this.weapon.transform.parent != null)
        {
            Destroy(this.weapon.transform.parent.gameObject);
        }
        else if (this.weapon != null)
        {
            this.weapon.DestroyWeapon();
        }

        this.weapon = newWeapon;
        if (this.weapon == null)
        {
            spineTest?.ClearAttackAnimation();
            return;
        }

        // 2. BoneFollower를 위한 새 부모 오브젝트(홀더) 생성
        weaponFollowerHolder = new GameObject(newWeapon.name + "_Follower");
        weaponFollowerHolder.transform.SetParent(weaponPivot, false);
        weaponFollowerHolder.transform.localPosition = Vector3.zero;
        weaponFollowerHolder.transform.localRotation = Quaternion.identity;
        weaponFollowerHolder.transform.localScale = Vector3.one;

        // 3. BoneFollower 컴포넌트를 홀더에 추가
        var boneFollower = weaponFollowerHolder.AddComponent<Spine.Unity.BoneFollower>();
        boneFollower.SkeletonRenderer = skeletonAni;
        boneFollower.boneName = "sword";
        if (this.weapon.GetWeaponType() == WeaponManager.WeaponType.Spear) boneFollower.boneName = "sword2";
        boneFollower.followBoneRotation = true;
        boneFollower.followLocalScale = false;
        boneFollower.followSkeletonFlip = false;

        // 4. Rigidbody Kinematic 설정
        var weaponRigidBody = this.weapon.GetComponent<Rigidbody2D>();
        if (weaponRigidBody != null)
        {
            weaponRigidBody.bodyType = RigidbodyType2D.Kinematic;
        }

        // 5. 무기를 '홀더'의 자식으로 설정
        this.weapon.transform.SetParent(weaponFollowerHolder.transform, false);
        this.weapon.transform.localPosition = Vector3.zero;
        this.weapon.transform.localRotation = Quaternion.identity;

        // 6. 스케일 계산 적용
        float parentScaleX = weaponPivot.lossyScale.x;
        float parentScaleY = weaponPivot.lossyScale.y;

        if (parentScaleX != 0 && parentScaleY != 0)
        {
            float desiredRatio = this.weapon.relativeScaleRatio;

            this.weapon.transform.localScale = new Vector3(
                desiredRatio / Mathf.Abs(parentScaleX),
                desiredRatio / Mathf.Abs(parentScaleY),
                1f
            );
        }
        else
        {
            this.weapon.transform.localScale = Vector3.one * this.weapon.relativeScaleRatio;
        }

        // 7. SpriteRenderer 설정
        spriteRenderer = weapon.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 0;
        }

        // 8. SkillController 설정
        if (skillController != null && this.weapon != null)
        {
            skillController.SetCurrentSkill(this.weapon.GetSkillLogic());
        }

        if (spineTest != null)
        {
            WeaponManager.WeaponType type = this.weapon.GetWeaponType();
            spineTest.SetAttackAnimationByWeaponType(type);
        }

        Flip(skeletonAni.skeleton.ScaleX < 0);
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

            if (spineTest != null)
            {
                spineTest.ClearAttackAnimation();
            }
        }
    }

    public void Flip(bool isRight)
    {
        if (weaponFollowerHolder != null)
        {
            float scaleY = isRight ? 1f : -1f;
            weaponFollowerHolder.transform.localScale = new Vector3(-1, scaleY, 1);
        }
    }
}