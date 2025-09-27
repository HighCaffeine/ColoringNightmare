using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Character/BossData")]
public class BossMonsterData : MonsterData
{
    public float attackRange;
    public float skillCooldown;
    public int phase; //패턴 단계
}
