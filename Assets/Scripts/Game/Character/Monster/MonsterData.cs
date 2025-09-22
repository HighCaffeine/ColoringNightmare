using UnityEngine;

[CreateAssetMenu(menuName = "Character/WallMonsterData")]
public class MonsterData : CharacterData
{
    [Header("MonsterType")]
    [SerializeField] protected ColorMixer.ColorType colorType;
    [SerializeField] protected MonsterManager.MonsterType type;

    public ColorMixer.ColorType ColorType => colorType;
    public MonsterManager.MonsterType Type => type;

}

[CreateAssetMenu(menuName = "Character/DetectMonsterData")]
public class DetectMonsterData : MonsterData
{
    [SerializeField] protected float attackRange = 1.5f;
    [SerializeField] protected float dashSpeed = 1.0f;
    public float AttackRange => attackRange;
    public float DashSpeed => dashSpeed;
}

[CreateAssetMenu(menuName = "Character/BossData")]
public class BossData : MonsterData
{
    public float attackRange;
    public float skillCooldown;
    public int phase; //패턴 단계
}

