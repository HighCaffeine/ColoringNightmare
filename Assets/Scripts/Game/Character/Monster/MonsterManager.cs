using UnityEngine;

public class MonsterManager : GenericSingleton<MonsterManager>
{
    public enum MonsterType
    {
        General,
        Special,
        Boss,
        Count,
    }

    [Header("Monster Obj Parent")]
    [SerializeField] private Transform monsterObjParent;
    [SerializeField] private AreaData monsterMoveArea;

    [Header("EachParent")]
    [SerializeField] private Transform generalPoolParent;
    [SerializeField] private Transform specialPoolParent;

    [Header("Prefabs")]
    [SerializeField] private MonsterController generalPrefab;
    [SerializeField] private SpecialMonsterController specialPrefab;

    [Header("Pool Count")]
    [SerializeField] private int generalPoolCount = 10;
    [SerializeField] private int specialPoolCount = 5;

    private ObjectPooling<MonsterManager, MonsterController> generalPool;
    private ObjectPooling<MonsterManager, SpecialMonsterController> specialPool;

    public Bounds GetAreaBound() => monsterMoveArea.GetBounds();


    protected new void Awake()
    {
        // 일반 몬스터 풀
        generalPool = new ObjectPooling<MonsterManager, MonsterController>();
        generalPool.SetParent(generalPoolParent);
        generalPool.SetPrefab(generalPrefab);
        generalPool.SetPoolCount(generalPoolCount);

        generalPool.Setup();

        // 스페셜 몬스터 풀
        specialPool = new ObjectPooling<MonsterManager, SpecialMonsterController>();
        specialPool.SetParent(specialPoolParent);
        specialPool.SetPrefab(specialPrefab);
        specialPool.SetPoolCount(specialPoolCount);

        specialPool.Setup();
    }

    // 풀에서 가져오기
    public MonsterController GetGeneralMonster(Vector2 pos)
    {
        var monster = generalPool.GetPool();
        monster.transform.position = pos;
        monster.gameObject.SetActive(true);
        monster.transform.SetParent(monsterObjParent);

        return monster;
    }

    public SpecialMonsterController GetSpecialMonster(Vector2 pos)
    {
        var monster = specialPool.GetPool();
        monster.transform.position = pos;
        monster.gameObject.SetActive(true);
        monster.transform.SetParent(monsterObjParent);

        return monster;
    }

    // 스폰 함수
    public void SpawnMonster(MonsterData data, Vector2 pos)
    {
        switch (data.Type)
        {
            case MonsterType.Special:
                var special = GetSpecialMonster(pos);
                special.Setup(data);
                break;
            default:
                var general = GetGeneralMonster(pos);
                general.Setup(data);
                break;
        }
    }
}
