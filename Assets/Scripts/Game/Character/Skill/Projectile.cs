using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 2f;
    private Vector3 direction;

    public void Init(Vector3 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, lifetime); // lifetime 후 자동 삭제
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 몬스터 태그
        if (other.CompareTag("Monster"))
        {
            // 몬스터에 데미지 주기
            Debug.Log($"Hit Monster: {other.name}");
            Destroy(gameObject);
        }
    }
}
