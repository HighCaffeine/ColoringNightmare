using UnityEngine;

[CreateAssetMenu(menuName = "Character/MonsterData")]
public class MonsterData : CharacterData
{
    [Header("MonsterType")]
    [SerializeField] private ColorMixer.ColorType colorType;
    [SerializeField] private MonsterManager.MonsterType type;
    [SerializeField] private float attackRange = 1.5f;

    public ColorMixer.ColorType ColorType => colorType;
    public MonsterManager.MonsterType Type => type;
    public float AttackRange => attackRange;
}
