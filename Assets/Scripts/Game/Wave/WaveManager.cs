using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class WaveManager : GenericSingleton<WaveManager>
{
    public enum WaveType
    {
        TimeLimit, Elimination, Boss,
    }

    [SerializeField] private List<WaveData> waves;

    [Header("Spawn Areas")]
    [SerializeField] private List<AreaData> spawnAreas;

    [Space(5f)]
    [Header("TEST_CurrentWave")]
    [SerializeField] private int currentWaveIndex = 1;

    private Coroutine spawnCoroutine;

    public delegate void OnAddEliminateCount();
    public OnAddEliminateCount onAddEliminateCount;
    private int eliminateCount;
    private float time;

    public void AddEliminateCount() { eliminateCount++; onAddEliminateCount?.Invoke(); }

    public void TEST_WaveStart()
    {
        WaveStart(currentWaveIndex);
    }

    public void WaveStart(int waveIndex)
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        InitWaveValue();
        spawnCoroutine = StartCoroutine(SpawnWaveCoroutine(waveIndex));
    }

    private void InitWaveValue()
    {
        eliminateCount = 0;
        time = 0.0f;
    }

    private IEnumerator SpawnWaveCoroutine(int waveIndex)
    {
        if (waveIndex < 0 || waveIndex >= waves.Count)
        {
            yield break;
        }

        WaveData wave = waves[waveIndex];

        foreach (var group in wave.monsterGroups)
        {
            yield return new WaitForSeconds(group.delayAfterGroup);

            for (int i = 0; i < group.spawnCount; i++)
            {
                Vector2 spawnPos;
                if (spawnAreas.Count == 0)
                {
                    yield break;
                }

                AreaData selectedArea;
                if (group.spawnPointIndex == -1)
                {
                    selectedArea = spawnAreas[Random.Range(0, spawnAreas.Count)];
                }
                else
                {
                    if (group.spawnPointIndex < 0 || group.spawnPointIndex >= spawnAreas.Count)
                    {
                        selectedArea = spawnAreas[Random.Range(0, spawnAreas.Count)];
                    }
                    else
                    {
                        selectedArea = spawnAreas[group.spawnPointIndex];
                    }
                }

                float halfX = selectedArea.size.x * 0.5f;
                float halfY = selectedArea.size.y * 0.5f;
                Vector2 pivot = selectedArea.pos;
                spawnPos = new Vector2(Random.Range(pivot.x - halfX, pivot.x + halfX), Random.Range(pivot.y - halfY, pivot.y + halfY));

                MonsterDataName monsterName = (MonsterDataName)group.monsterCode;

                SODataLoader.Instance.LoadSO<MonsterData>(monsterName.ToString(), so =>
                {
                    MonsterManager.Instance.SpawnMonster(so, spawnPos);
                });

                // Wait for the interval between individual monsters
                yield return new WaitForSeconds(group.spawnInterval);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (spawnAreas.Count > 0)
        {
            foreach (var area in spawnAreas)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(area.pos + area.offset, area.size);
            }
        }
    }
}