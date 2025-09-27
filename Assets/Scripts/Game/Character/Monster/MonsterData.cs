using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Character/WallMonsterData")]
public class MonsterData : CharacterData
{
    [Header("MonsterType")]
    [SerializeField] protected ColorMixer.ColorType colorType;
    [SerializeField] protected MonsterManager.MonsterType type;

    public ColorMixer.ColorType ColorType => colorType;
    public MonsterManager.MonsterType Type => type;

}