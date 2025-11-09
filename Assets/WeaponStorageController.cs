using UnityEngine;

public class WeaponStorageController : GenericSingleton<WeaponStorageController>
{
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private Transform storedWeaponPivot;

    private GameObject storedObj = null;
    private Weapon storedWeapon = null;

    public void StoreNewWeapon(GameObject newWeapon)
    {
        if (storedObj != null)
        {
            Destroy(storedObj);
        }

        storedObj = newWeapon;
        storedWeapon = newWeapon.GetComponent<Weapon>();

        if (storedWeapon == null)
        {
            Destroy(storedObj);
            return;
        }

        storedObj.transform.SetParent(storedWeaponPivot);
        storedObj.transform.localPosition = Vector3.zero;
        storedObj.transform.localRotation = Quaternion.identity;
        storedObj.transform.localScale = Vector3.one;
        storedObj.transform.rotation = new Quaternion(0, 0, 0, 0);
    }

    public void SwapWeapons()
    {
        Weapon currentEquippedWeapon = weaponController.GetEquippedWeapon();

        weaponController.SetupWeapon(storedWeapon);

        //장착중인 무기를 storage로 이동
        if (currentEquippedWeapon != null)
        {
            storedObj = currentEquippedWeapon.gameObject;
            storedWeapon = currentEquippedWeapon;

            storedObj.transform.SetParent(storedWeaponPivot);
            storedObj.transform.localPosition = Vector3.zero;
            storedObj.transform.localScale = Vector3.one;
            storedObj.transform.rotation = new Quaternion(0, 0, 0, 0);
        }
        else
        {
            storedObj = null;
            storedWeapon = null;
        }
    }
}