using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Character/DetectMonsterData")]
public class DetectMonsterData : MonsterData
{
    [SerializeField] protected float attackRange = 1.5f;
    [SerializeField] protected float dashSpeed = 1.0f;
    public float AttackRange => attackRange;
    public float DashSpeed => dashSpeed;
}