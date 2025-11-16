using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Character/WallMonsterData")]
public class MonsterData : CharacterData
{
    [Header("MonsterType")]
    [SerializeField] protected ColorMixer.ColorType colorType;
    [SerializeField] protected MonsterManager.MonsterType type;

    [SerializeField] protected float pushForce;

    [Header("Loot Settings")]
    [Tooltip("몬스터가 드랍할 아이템 목록")]
    public ItemDropTable lootTable;

    public ColorMixer.ColorType ColorType => colorType;
    public MonsterManager.MonsterType Type => type;
    public float PushForce => pushForce;

}