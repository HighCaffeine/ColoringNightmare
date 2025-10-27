using UnityEngine;

public class WeaponDragController : MonoBehaviour
{
    private Weapon weaponObj;

    [SerializeField] private AreaData targetArea;

    [SerializeField] private UnityEngine.Events.UnityEvent OnAreaEnter;
    [SerializeField] private UnityEngine.Events.UnityEvent OnAreaExit;

    [SerializeField] private WeaponController targetWeaponController;

    private Camera mainCamera;
    private bool isAllowWeaponControl;
    private Vector2 defaultPos;

    public void DisallowWeaponControl()
    {
        isAllowWeaponControl = false;
        if (weaponObj != null)
        {
            weaponObj.transform.position = defaultPos;
        }
    }

    public void AllowWeaponControl()
    {
        isAllowWeaponControl = true;
    }

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void OnBeginWeaponCheck()
    {
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Weapon"))
        {
            isAllowWeaponControl = true;
            weaponObj = hit.collider.GetComponent<Weapon>();
            if (weaponObj != null)
            {
                defaultPos = weaponObj.transform.position;
            }
        }
        else
        {
            isAllowWeaponControl = false;
            weaponObj = null;
        }
    }

    public void OnDragEvent()
    {
        if (!isAllowWeaponControl || weaponObj == null) return;

        weaponObj.transform.position = DrawWeapon.Instance.GetMousePos();

        if (targetArea.GetBounds().Contains(DrawWeapon.Instance.GetMousePos()))
        {
            OnAreaEnter?.Invoke();
        }
        else
        {
            OnAreaExit?.Invoke();
        }
    }

    public void OnDropWeaponEvent()
    {
        if (!isAllowWeaponControl || weaponObj == null) return;

        if (targetArea.GetBounds().Contains(DrawWeapon.Instance.GetMousePos()))
        {
            if (targetWeaponController != null)
            {
                isAllowWeaponControl = false;
                targetWeaponController.SetupWeapon(weaponObj);
                OnAreaExit?.Invoke();
                weaponObj = null;
            }
        }
        else
        {
            weaponObj.transform.position = defaultPos;
        }
    }
}