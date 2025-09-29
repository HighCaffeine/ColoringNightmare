using UnityEngine;

public class SkillController : MonoBehaviour
{
    //아래 데이터들 skilldata로 추후 수정
    public GameObject projectilePrefab;
    [SerializeField] private float skillSpeed = 5f;
    [SerializeField] private Transform effectPivot;

    [SerializeField] private WeaponController weaponController;

    public float test_lifeTime = 5f;

    public void UseSkill()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile Prefab is missing");
            return;
        }
        if (!weaponController.IsEquip()) return;

        // 기준 방향 캐릭터 앞
        Vector3 forward = transform.right * -1 * Mathf.Sign(effectPivot.localScale.x);

        // 3방향 중앙, 15도 플마
        float[] angles = { 0f, -15f, 15f };

        foreach (float angle in angles)
        {
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Vector3 dir = rot * forward;  // forward가 이제 캐릭터 방향에 맞춰짐

            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            proj.GetComponent<Projectile>().Init(test_lifeTime, skillSpeed, dir);
        }
    }

}
