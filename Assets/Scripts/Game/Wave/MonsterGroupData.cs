using UnityEngine;

[CreateAssetMenu(fileName = "MonsterGroupData", menuName = "Wave/MonsterGroupData")]
public class MonsterGroupData : ScriptableObject
{
    [Header("Monster Spawn Group")]
    public int groupID;                 // GroupID (그룹 번호)
    public MonsterDataName monsterCode;             // Monster_Code (몬스터 식별 코드)
    public int spawnCount;              // Spawn_Count (스폰할 몬스터 수)
    public float spawnInterval;         // Spawn_Interval (몬스터 개별 스폰 간격)
    public int spawnPointIndex;         // Spawn_Point_Index (스폰 지점 인덱스)
    public float delayAfterGroup;       // Delay_After_Group (다음 그룹까지의 대기 시간)
}
