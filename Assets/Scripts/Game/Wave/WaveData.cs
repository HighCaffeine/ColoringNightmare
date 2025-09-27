using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Wave/WaveData")]
public class WaveData : ScriptableObject
{
    public int waveNumber;
    public AreaData spawnArea;
    public WaveManager.WaveType waveType;
    public int typeValue;   // 웨이브 타입 값 (TimeLimit 시간초, Elimination 처치수, Boss 보스처치)

    [Header("Monster SO Name")]
    public List<MonsterDataName> monsterDatas;

    //Random Spawn 위치 가져오기
    public Vector2 GetRandomAreaPoint()
    {
        float halfX = spawnArea.size.x * 0.5f;
        float halfY = spawnArea.size.y * 0.5f;

        Vector2 pivot = spawnArea.pos;

        return new Vector2(Random.Range(pivot.x - halfX, pivot.x + halfX),
                            Random.Range(pivot.y - halfY, pivot.y + halfY));
    }
}
