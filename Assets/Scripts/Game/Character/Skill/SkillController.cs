using UnityEngine;

public class SkillController : MonoBehaviour
{
    public GameObject projectilePrefab;
    [SerializeField] private Transform effectPivot; // 이펙트가 생성될 위치
    [SerializeField] private WeaponController weaponController;
    private Character character; // 캐릭터의 방향을 알기 위해 참조

    // 추후 SkillData에서 가져올 임시 데이터들
    public float test_lifeTime = 5f;
    [SerializeField] private float skillSpeed = 5f;

    void Awake()
    {
        // SkillController가 붙어있는 게임 오브젝트에서 Character 컴포넌트를 가져옴
        character = GetComponent<Character>();
    }

    public void UseSkill()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile Prefab is missing");
            return;
        }
        if (!weaponController.IsEquip()) return;

        // 1. 캐릭터의 현재 방향을 정확하게 가져옴 (1: 오른쪽, -1: 왼쪽)
        // 스켈레톤의 ScaleX를 확인하는 것이 가장 확실한 방법입니다.
        float facingDirection = 1f;
        if (character != null && character.skeleton != null)
        {
            facingDirection = Mathf.Sign(character.skeleton.skeleton.ScaleX);
        }

        // 캐릭터가 바라보는 정면 방향 벡터
        Vector3 forward = transform.right * facingDirection;

        // 3방향 발사 로직 (현재는 중앙 1방향만 활성화)
        float[] angles = { 0f /*, -15f, 15f */ };

        foreach (float angle in angles)
        {
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Vector3 dir = rot * forward;

            // 2. 이펙트를 effectPivot의 위치에서 생성하여 항상 캐릭터 앞에 나타나도록 수정
            GameObject proj = Instantiate(projectilePrefab, effectPivot.position, Quaternion.identity);

            // 3. 현재 장착된 무기의 크기를 가져와 이펙트에 적용
            Weapon currentWeapon = weaponController.GetEquippedWeapon();
            if (currentWeapon != null)
            {
                proj.transform.localScale = currentWeapon.transform.localScale;
            }

            // 투사체 초기화
            proj.GetComponent<Projectile>().Init(test_lifeTime, skillSpeed, dir);
        }
    }
}