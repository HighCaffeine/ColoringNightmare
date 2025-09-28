using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

//Pooling으로 변경
public class WaveManager : GenericSingleton<WaveManager>
{
    public enum WaveType
    {
        TimeLimit, Elimination, Boss,
    }

    [SerializeField] private List<WaveData> waves;
    [SerializeField] private float spawnDelay = 2f;

    [Space(5f)]
    [Header("TEST_CurrentWave")]
    [SerializeField] private int currentWaveIndex = 1;

    private int currentWave;
    private bool waveClear;
    private Coroutine spawnCoroutine;

    ///////////////Wave Value////////////////
    public delegate void OnAddEliminateCount();
    public OnAddEliminateCount onAddEliminateCount;
    private int eliminateCount;
    private float time;

    public void AddEliminateCount() { eliminateCount++; onAddEliminateCount?.Invoke(); }

    ///////////////Wave Value////////////////

    private new void Awake()
    {
        base.Awake();
    }

    private void InitWaveValue()
    {
        waveClear = false;
        eliminateCount = 0;
        time = 0.0f;
    }

    public void TEST_WaveStart()
    {
        WaveStart(currentWaveIndex);
    }

    //GameManager 이벤트 등록
    public void WaveStart(int waveIndex)
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);

        InitWaveValue();
        spawnCoroutine = StartCoroutine(SpawnMonsterCoroutine(waveIndex));
    }

    private IEnumerator SpawnMonsterCoroutine(int waveIndex)
    {
        WaveData wave = waves[waveIndex];
        WaitForSeconds wait = new WaitForSeconds(spawnDelay);

        float waveTimer = 0f;
        int eliminateCounter = 0;

        while (true)
        {
            // 종료 조건 체크
            switch (wave.waveType)
            {
                case WaveType.Elimination:
                    if (eliminateCounter >= wave.typeValue) yield break;
                    break;
                case WaveType.TimeLimit:
                    waveTimer += spawnDelay;
                    if (waveTimer >= wave.typeValue) yield break;
                    break;
                case WaveType.Boss:
                    //boss 체력 체크 필요
                    break;
            }

            // 몬스터 스폰
            Vector2 newPos = wave.GetRandomAreaPoint();
            SetupMonster(waveIndex, newPos);

            yield return wait;
        }

        currentWaveIndex++;
    }


    private void SetupMonster(int waveIndex, Vector2 pos)
    {
        MonsterDataName dataName = waves[waveIndex].monsterDatas[Random.Range(0, waves[waveIndex].monsterDatas.Count)];
        MonsterData data = null;

        SODataLoader.Instance.LoadSO<MonsterData>(dataName.ToString(), so =>
        {
            data = so as MonsterData;

            if (data != null)
            {
                MonsterManager.Instance.SpawnMonster(data, pos);
            }
        });
    }

    public void ClearCurrentWave()
    {
        waveClear = true;
    }

    void OnDrawGizmos()
    {
        // if (currentWaveIndex < 0) return;
        // if (currentWaveIndex < waves.Count && waves[currentWaveIndex] != null)
        // {
        //     WaveData w = waves[currentWaveIndex];
        //     Gizmos.color = Color.red;

        //     Gizmos.DrawWireCube(w.spawnArea.pos + w.spawnArea.offset, w.spawnArea.size);
        // }

        if (waves[0] != null)
        {
            WaveData w = waves[0];
            Gizmos.color = Color.red;

            Gizmos.DrawWireCube(w.spawnArea.pos + w.spawnArea.offset, w.spawnArea.size);
        }
    }
}
