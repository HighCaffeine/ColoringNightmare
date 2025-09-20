using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float speed;
    private Vector3 direction;

    public void Init(float lifeTime, float speed, Vector2 dir)
    {
        this.speed = speed;
        this.direction = dir;
        Destroy(gameObject, lifeTime); // lifetime 후 자동 삭제
    }

    void Update()
    {
        transform.position += direction.normalized * speed * Time.deltaTime;
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
