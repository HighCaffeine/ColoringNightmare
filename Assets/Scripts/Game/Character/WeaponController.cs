using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Weapon weapon;

    [SerializeField] private Transform weaponPivot;

    public void SetupWeapon(Weapon weapon)
    {
        this.weapon = weapon;

        this.weapon.transform.SetParent(weaponPivot);

        this.weapon.transform.localPosition = Vector3.zero;
        this.weapon.transform.localRotation = Quaternion.identity;
    }
}
