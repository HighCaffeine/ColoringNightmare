using UnityEngine;
using System;
using System.Collections;

public class PlayerNoneWeapon : MonoBehaviour
{
    BoxCollider2D boxCollider2D;
    private bool hitOnce = false;
    private bool onAttack = false;

    void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    public void OnResetAttack() { boxCollider2D.enabled = true; onAttack = false; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hitOnce || onAttack) return; // 이미 한 번 맞았으면 종료
        boxCollider2D.enabled = false;

        if (other.CompareTag("Monster"))
        {
            other.GetComponent<Character>()?.TakeDamage(1);
            hitOnce = true;
            StartCoroutine(ResetHit());
        }
    }

    private IEnumerator ResetHit()
    {
        yield return new WaitForSeconds(0.2f); // 히트박스 지속 동안 반복 공격 방지
        hitOnce = false;
    }
}
