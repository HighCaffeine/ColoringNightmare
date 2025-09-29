using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Wave/WaveData")]
public class WaveData : ScriptableObject
{
    public int waveNumber;
    public WaveManager.WaveType waveType;
    public int typeValue;

    [Header("Monster Spawn Groups")]
    public List<MonsterGroupData> monsterGroups;
}