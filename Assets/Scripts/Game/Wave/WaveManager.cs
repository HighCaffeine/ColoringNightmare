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

    public int TotalWaveCount => waves.Count;
    public int CurrentWaveIndex => currentWaveIndex;

    [Header("Boss Settings")]
    [SerializeField] private float bossSpawnDelay = 3.0f; // 마지막 웨이브 후 보스 등장 전 딜레이
    [SerializeField] private MonsterDataName bossMonsterName; // 보스 데이터 이름 (Enum or String)
    [SerializeField] private Transform bossSpawnPoint; // 보스 소환 위치
    [SerializeField] private UnityEngine.Events.UnityEvent OnAllWavesClear;

    [Header("Test Event")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnWaveStart;
    [SerializeField] private UnityEngine.Events.UnityEvent OnWaveEnd;

    [SerializeField] private List<WaveData> waves;
    [SerializeField] private float waveInterval = 2.0f;

    [Header("Spawn Areas")]
    [SerializeField] private List<AreaData> spawnAreas;

    [Space(5f)]
    [Header("TEST_CurrentWave")]
    [SerializeField] private int currentWaveIndex = 0;

    public int GetWaveCount() => waves.Count;
    public int GetCurrentWaveIndex() => currentWaveIndex;

    private Coroutine spawnCoroutine;

    public delegate void OnAddEliminateCount();
    public OnAddEliminateCount onAddEliminateCount;
    private int eliminateCount;
    private float time;

    private bool isFirst = true;

    public void AddEliminateCount() { eliminateCount++; onAddEliminateCount?.Invoke(); }

    public void TEST_WaveStart()
    {
        if (!isFirst) return;
        isFirst = false;
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        InitWaveValue();

        // 현재 인덱스가 웨이브 리스트보다 많거나 같으면 -> 보스전 진입
        if (currentWaveIndex >= waves.Count)
        {
            spawnCoroutine = StartCoroutine(SpawnBossCoroutine());
        }
        else
        {
            spawnCoroutine = StartCoroutine(SpawnWaveCoroutine(currentWaveIndex));
        }
    }


    private IEnumerator SpawnWaveCoroutine(int waveIndex)
    {
        OnWaveStart?.Invoke();

        WaveData wave = waves[waveIndex];

        foreach (var group in wave.monsterGroups)
        {
            if (group.delayAfterGroup > 0)
                yield return new WaitForSeconds(group.delayAfterGroup);

            for (int i = 0; i < group.spawnCount; i++)
            {
                SpawnMonster(group);
                yield return new WaitForSeconds(group.spawnInterval);
            }
        }

        OnWaveEnd?.Invoke();
        yield return new WaitForSeconds(waveInterval);

        currentWaveIndex++;
        StartNextWave();
    }

    private void SpawnMonster(MonsterGroupData group)
    {
        if (spawnAreas.Count == 0) return;

        Vector2 spawnPos;
        AreaData selectedArea;

        if (group.spawnPointIndex == -1 || group.spawnPointIndex >= spawnAreas.Count)
        {
            selectedArea = spawnAreas[Random.Range(0, spawnAreas.Count)];
        }
        else
        {
            selectedArea = spawnAreas[group.spawnPointIndex - 1];
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
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
    }

    private IEnumerator SpawnBossCoroutine()
    {
        OnAllWavesClear?.Invoke();

        yield return new WaitForSeconds(bossSpawnDelay);

        Debug.Log("Spawn Boss!");
        MonsterManager.Instance.TEST_SpawnBoss();
    }

    private void InitWaveValue()
    {
        eliminateCount = 0;
        time = 0.0f;
    }

    public void BossSpawn_BGMStop()
    {
        SoundManager.Instance.StopBGM();
    }

    // private IEnumerator SpawnWaveCoroutine(int waveIndex)
    // {
    //     OnWaveStart?.Invoke();

    //     if (waveIndex < 0 || waveIndex >= waves.Count)
    //     {
    //         yield break;
    //     }

    //     WaveData wave = waves[waveIndex];

    //     foreach (var group in wave.monsterGroups)
    //     {
    //         yield return new WaitForSeconds(group.delayAfterGroup);

    //         for (int i = 0; i < group.spawnCount; i++)
    //         {
    //             Vector2 spawnPos;
    //             if (spawnAreas.Count == 0)
    //             {
    //                 yield break;
    //             }

    //             AreaData selectedArea;
    //             if (group.spawnPointIndex == -1)
    //             {
    //                 selectedArea = spawnAreas[Random.Range(0, spawnAreas.Count)];
    //             }
    //             else
    //             {
    //                 if (group.spawnPointIndex < 0 || group.spawnPointIndex >= spawnAreas.Count)
    //                 {
    //                     selectedArea = spawnAreas[Random.Range(0, spawnAreas.Count)];
    //                 }
    //                 else
    //                 {
    //                     selectedArea = spawnAreas[group.spawnPointIndex - 1];
    //                 }
    //             }

    //             float halfX = selectedArea.size.x * 0.5f;
    //             float halfY = selectedArea.size.y * 0.5f;
    //             Vector2 pivot = selectedArea.pos;
    //             spawnPos = new Vector2(Random.Range(pivot.x - halfX, pivot.x + halfX), Random.Range(pivot.y - halfY, pivot.y + halfY));

    //             MonsterDataName monsterName = (MonsterDataName)group.monsterCode;

    //             SODataLoader.Instance.LoadSO<MonsterData>(monsterName.ToString(), so =>
    //             {
    //                 MonsterManager.Instance.SpawnMonster(so, spawnPos);
    //             });

    //             yield return new WaitForSeconds(group.spawnInterval);
    //         }
    //     }

    //     //check wave clear 
    //     if (CheckWaveClear(wave.waveType))
    //     {
    //         OnWaveEnd?.Invoke();
    //     }

    //     currentWaveIndex++;

    //     //임의로 0.5초 딜레이 후 다음 웨이브
    //     Invoke(nameof(TestWaveStart), 0.5f);
    // }

    private bool CheckWaveClear(WaveType waveType)
    {
        switch (waveType)
        {
            case WaveType.Elimination:

                break;
            case WaveType.TimeLimit:

                break;
        }

        return true;
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