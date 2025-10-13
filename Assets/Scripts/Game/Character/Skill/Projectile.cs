using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float speed;
    private Vector3 direction;

    public void Init(float lifeTime, float speed, Vector2 dir)
    {
        this.speed = speed;
        this.direction = dir;
        transform.localScale = new Vector3(dir.x < 0 ? -1 : 1, 1, 1);
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
            Character monster = other.GetComponent<Character>();
            if (monster != null)
            {
                monster.TakeDamage(1);  //테스트 1데미지
            }
            //Destroy(gameObject);
        }
    }
}
