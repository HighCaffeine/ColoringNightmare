using UnityEngine;

public class MonsterManager : GenericSingleton<MonsterManager>
{
    public enum MonsterType
    {
        Wall,
        Detect,
        Boss,
        Count,
    }

    [Header("Monster Obj Parent")]
    [SerializeField] private Transform monsterObjParent;
    [SerializeField] private AreaData monsterMoveArea;

    [Header("Each Parent")]
    [SerializeField] private Transform detectPoolParent;
    [SerializeField] private Transform wallPoolParent;
    [SerializeField] private Transform bossPoolParent;

    [Header("Prefabs")]
    [SerializeField] private DetectMonsterBaseController detectPrefab;
    [SerializeField] private WallMonsterBaseController<MonsterData> wallPrefab;
    [SerializeField] private BossMonsterController bossPrefab;

    [Header("Pool Count")]
    [SerializeField] private int detectPoolCount = 10;
    [SerializeField] private int wallPoolCount = 5;
    [SerializeField] private int bossPoolCount = 1; // 한 마리만 풀링

    private ObjectPooling<MonsterManager, DetectMonsterBaseController> detectPool;
    private ObjectPooling<MonsterManager, WallMonsterBaseController<MonsterData>> wallPool;
    private ObjectPooling<MonsterManager, BossMonsterController> bossPool;

    public Bounds GetAreaBound() => monsterMoveArea.GetBounds();

    protected new void Awake()
    {
        // 탐지 몬스터 풀
        detectPool = new ObjectPooling<MonsterManager, DetectMonsterBaseController>();
        detectPool.SetParent(detectPoolParent);
        detectPool.SetPrefab(detectPrefab);
        detectPool.SetPoolCount(detectPoolCount);
        detectPool.Setup();

        // 벽 몬스터 풀
        wallPool = new ObjectPooling<MonsterManager, WallMonsterBaseController<MonsterData>>();
        wallPool.SetParent(wallPoolParent);
        wallPool.SetPrefab(wallPrefab);
        wallPool.SetPoolCount(wallPoolCount);
        wallPool.Setup();

        // 보스 풀 (한 마리)
        bossPool = new ObjectPooling<MonsterManager, BossMonsterController>();
        bossPool.SetParent(bossPoolParent);
        bossPool.SetPrefab(bossPrefab);
        bossPool.SetPoolCount(bossPoolCount);
        bossPool.Setup();
    }

    // 풀에서 가져오기
    public DetectMonsterBaseController GetDetectMonster(Vector2 pos)
    {
        var monster = detectPool.GetPool();
        monster.transform.position = pos;
        monster.gameObject.SetActive(true);
        monster.transform.SetParent(monsterObjParent);

        return monster;
    }

    public WallMonsterBaseController<MonsterData> GetWallMonster(Vector2 pos)
    {
        var monster = wallPool.GetPool();
        monster.transform.position = pos;
        monster.gameObject.SetActive(true);
        monster.transform.SetParent(monsterObjParent);

        return monster;
    }

    public BossMonsterController GetBoss(Vector2 pos)
    {
        var boss = bossPool.GetPool();
        boss.transform.position = pos;
        boss.gameObject.SetActive(true);
        boss.transform.SetParent(monsterObjParent);

        return boss;
    }

    // 몬스터 스폰
    public void SpawnMonster(MonsterData data, Vector2 pos)
    {
        switch (data.Type)
        {
            case MonsterType.Wall:
                var wall = GetWallMonster(pos);
                wall.Setup(data);
                break;

            case MonsterType.Detect:
                var detectData = data as DetectMonsterData;
                if (detectData == null) return;

                var detect = GetDetectMonster(pos);
                detect.Setup(detectData);
                break;

            case MonsterType.Boss:
                var bossData = data as BossData;
                if (bossData == null) return;

                var boss = GetBoss(pos);
                boss.Setup(bossData);

                break;
        }
    }
}
