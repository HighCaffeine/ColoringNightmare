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

    public void DisallowWeaponControl() { isAllowWeaponControl = false; weaponObj.transform.position = defaultPos; }
    public void AllowWeaponControl() { isAllowWeaponControl = true; }

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    //무기 드래그 및 bounds 체크

    public void OnBeginWeaponCheck()
    {
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            Debug.Log("Hit: " + hit.collider.name);

            if (hit.collider.CompareTag("Weapon"))
            {
                isAllowWeaponControl = true;
                weaponObj = hit.collider.GetComponent<Weapon>();
                Debug.Log("Weapon selected");

                defaultPos = weaponObj.transform.position;
            }
        }
        else
        {
            weaponObj = null;
        }
    }

    public void OnDragEvent()
    {
        if (!isAllowWeaponControl) return;

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
        if (!isAllowWeaponControl) return;

        if (targetArea.GetBounds().Contains(DrawWeapon.Instance.GetMousePos()))
        {
            if (targetWeaponController != null)
            {
                isAllowWeaponControl = false;
                targetWeaponController.SetupWeapon(weaponObj);
                OnAreaExit?.Invoke();
            }
        }
        else
        {
            weaponObj.transform.position = defaultPos;
        }
    }
}
