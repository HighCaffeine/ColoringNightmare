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


    protected new void Awake()
    {
        // 일반 몬스터 풀
        generalPool = gameObject.AddComponent<ObjectPooling<MonsterManager, MonsterController>>();
        generalPool.SetParent(generalPoolParent);
        generalPool.SetPrefab(generalPrefab);
        generalPool.SetPoolCount(generalPoolCount);

        // 스페셜 몬스터 풀
        specialPool = gameObject.AddComponent<ObjectPooling<MonsterManager, SpecialMonsterController>>();
        specialPool.SetParent(specialPoolParent);
        specialPool.SetPrefab(specialPrefab);
        specialPool.SetPoolCount(specialPoolCount);
    }

    // 풀에서 가져오기
    public MonsterController GetGeneralMonster(Vector2 pos)
    {
        var monster = generalPool.GetPool();
        monster.transform.position = pos;
        monster.gameObject.SetActive(true);
        return monster;
    }

    public SpecialMonsterController GetSpecialMonster(Vector2 pos)
    {
        var monster = specialPool.GetPool();
        monster.transform.position = pos;
        monster.gameObject.SetActive(true);
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
