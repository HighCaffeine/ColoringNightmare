using UnityEngine;

public class SkillController : MonoBehaviour
{
    public GameObject projectilePrefab;

    public float test_lifeTime = 5f;

    public void UseSkill()
    {
        Debug.Log("weapon created");

        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile Prefab is missing");
            return;
        }

        // 기준 방향 캐릭터 앞
        Vector3 forward = transform.right * -1;

        // 3방향 중앙, 15도 플마
        float[] angles = { 0f, -15f, 15f };

        foreach (float angle in angles)
        {
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Vector3 dir = rot * forward;

            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            proj.GetComponent<Projectile>().Init(test_lifeTime);
        }
    }

}
