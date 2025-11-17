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

    [SerializeField] private UnityEngine.Events.UnityEvent OnSubWorldHP;

    [Header("Monster Obj Parent")]
    [SerializeField] private Transform monsterObjParent;
    [SerializeField] private AreaData monsterMoveArea;

    [Header("Pool Parents")]
    [SerializeField] private Transform detectPoolParent;
    [SerializeField] private Transform wallPoolParent;
    [SerializeField] private Transform bossPoolParent;

    [Header("Prefabs")]
    [Header("Detect Prefabs")]
    [SerializeField] private DetectMonsterBaseController catNormalPrefab;
    [SerializeField] private DetectMonsterBaseController catElitePrefab;
    [SerializeField] private DetectMonsterBaseController rabbitNormalPrefab;
    [SerializeField] private DetectMonsterBaseController rabbitElitePrefab;
    [Header("Wall Prefabs")]
    [SerializeField] private WallMonsterBaseController<MonsterData> bearNormalPrefab;
    [SerializeField] private WallMonsterBaseController<MonsterData> bearElitePrefab;
    [Header("Boss Prefab")]
    [SerializeField] private BossMonsterController bossPrefab;

    [Header("Pool Count")]
    [SerializeField] private int detectPoolCount = 10;
    [SerializeField] private int wallPoolCount = 5;
    [SerializeField] private int bossPoolCount = 1;

    private ObjectPooling<MonsterManager, DetectMonsterBaseController> catNormalPool;
    private ObjectPooling<MonsterManager, DetectMonsterBaseController> catElitePool;
    private ObjectPooling<MonsterManager, DetectMonsterBaseController> rabbitNormalPool;
    private ObjectPooling<MonsterManager, DetectMonsterBaseController> rabbitElitePool;
    private ObjectPooling<MonsterManager, WallMonsterBaseController<MonsterData>> bearNormalPool;
    private ObjectPooling<MonsterManager, WallMonsterBaseController<MonsterData>> bearElitePool;
    private ObjectPooling<MonsterManager, BossMonsterController> bossPool;

    public Bounds GetAreaBound() => monsterMoveArea.GetBounds();

    protected new void Awake()
    {
        base.Awake();

        catNormalPool = CreateDetectPool(catNormalPrefab, detectPoolCount);
        catElitePool = CreateDetectPool(catElitePrefab, detectPoolCount);
        rabbitNormalPool = CreateDetectPool(rabbitNormalPrefab, detectPoolCount);
        rabbitElitePool = CreateDetectPool(rabbitElitePrefab, detectPoolCount);

        bearNormalPool = CreateWallPool(bearNormalPrefab, wallPoolCount);
        bearElitePool = CreateWallPool(bearElitePrefab, wallPoolCount);

        bossPool = new ObjectPooling<MonsterManager, BossMonsterController>();
        bossPool.SetParent(bossPoolParent);
        bossPool.SetPrefab(bossPrefab);
        bossPool.SetPoolCount(bossPoolCount);
        bossPool.Setup();

        InitializeAcquireCallbacks();
    }

    private ObjectPooling<MonsterManager, DetectMonsterBaseController> CreateDetectPool(DetectMonsterBaseController prefab, int count)
    {
        var pool = new ObjectPooling<MonsterManager, DetectMonsterBaseController>();
        pool.SetParent(detectPoolParent);
        pool.SetPrefab(prefab);
        pool.SetPoolCount(count);
        pool.Setup();
        return pool;
    }

    private ObjectPooling<MonsterManager, WallMonsterBaseController<MonsterData>> CreateWallPool(WallMonsterBaseController<MonsterData> prefab, int count)
    {
        var pool = new ObjectPooling<MonsterManager, WallMonsterBaseController<MonsterData>>();
        pool.SetParent(wallPoolParent);
        pool.SetPrefab(prefab);
        pool.SetPoolCount(count);
        pool.Setup();
        return pool;
    }

    public Character GetMonsterFromPool(MonsterDataName monsterName, Vector2 pos)
    {
        Character monster = null;

        switch (monsterName)
        {
            case MonsterDataName.CatNormal: monster = catNormalPool.GetPool(); break;
            case MonsterDataName.CatElite: monster = catElitePool.GetPool(); break;
            case MonsterDataName.RabbitNormal: monster = rabbitNormalPool.GetPool(); break;
            case MonsterDataName.RabbitElite: monster = rabbitElitePool.GetPool(); break;
            case MonsterDataName.BearNormal: monster = bearNormalPool.GetPool(); break;
            case MonsterDataName.BearElite: monster = bearElitePool.GetPool(); break;
            case MonsterDataName.TestBossData: monster = bossPool.GetPool(); break;
            default:
                Debug.LogError($"{monsterName}에 해당하는 풀링 없음.");
                return null;
        }

        if (monster == null)
        {
            Debug.LogError($"{monsterName} pool loaded fail");
            return null;
        }

        monster.transform.position = pos;
        monster.gameObject.SetActive(true);
        monster.transform.SetParent(monsterObjParent);

        return monster;
    }

    public void SpawnMonster(MonsterData data, Vector2 pos)
    {
        if (data == null)
        {
            Debug.LogError("[SpawnMonster] data is null");
            return;
        }

        Character monster = GetMonsterFromPool(data.monsterDataName, pos);
        if (monster == null) return;

        switch (data.Type)
        {
            case MonsterType.Wall:
                (monster as WallMonsterBaseController<MonsterData>)?.Setup(data);
                break;

            case MonsterType.Detect:
                (monster as DetectMonsterBaseController)?.Setup(data as DetectMonsterData);
                break;

            case MonsterType.Boss:
                (monster as BossMonsterController)?.Setup(data as BossMonsterData);
                break;
        }

        ColorTest(monster.transform, data.ColorType);

        if (monster != null)
        {
            SetSortingOrderByYPos(monster, data.isSpine);
        }
    }

    private void ColorTest(Transform t, ColorMixer.ColorType colorType)
    {
        MonsterColorChanger m = t.GetComponent<MonsterColorChanger>();
        m.SetColorEnum(colorType);
    }

    public Vector2 GetCalibrationSpawnPos(Transform monster, Collider2D collider)
    {
        Vector2 result = Vector2.zero;
        if (collider != null)
        {
            Vector2 monsterHalfSize = collider.bounds.extents;
            Bounds area = monsterMoveArea.GetBounds();
            float caliX = Mathf.Clamp(monster.position.x, area.min.x + monsterHalfSize.x, area.max.x - monsterHalfSize.x);
            float caliY = Mathf.Clamp(monster.position.y, area.min.y + monsterHalfSize.y, area.max.y - monsterHalfSize.y);
            result = new Vector2(caliX, caliY);
        }
        return result;
    }

    private System.Collections.Generic.Dictionary<ItemType, System.Action<ItemData>> acquireCallbacks;


    public void NotifyMonsterDeath(Character monster, MonsterData data)
    {
        HandleItemDrop(monster, data);
    }

    public void TestItemDrop(ItemDropTable itemDropTable, Vector3 pos)
    {
        DropManager.Instance.ProcessLootTable(
                    itemDropTable,             // 몬스터의 드랍 아이템 목록
                    pos, // 몬스터가 죽은 위치
                    acquireCallbacks            // 아이템 획득 시 실행될 이벤트 묶음
                );
    }

    private void HandleItemDrop(Character monster, MonsterData data)
    {
        if (data.lootTable == null || data.lootTable.itemDropTable.Count == 0) return;

        DropManager.Instance.ProcessLootTable(
            data.lootTable,             // 몬스터의 드랍 아이템 목록
            monster.transform.position, // 몬스터가 죽은 위치
            acquireCallbacks            // 아이템 획득 시 실행될 이벤트 묶음
        );
    }

    private void InitializeAcquireCallbacks()
    {
        acquireCallbacks = new System.Collections.Generic.Dictionary<ItemType, System.Action<ItemData>>
        {
            { ItemType.Ink, (itemData) =>
            {
                InkItem inkData = itemData as InkItem;

                if (inkData != null)
                {
                    Debug.Log($"{inkData.colorType} 잉크 획득");
                    MixerButtonController.Instance.AddInk(inkData.colorType);
                }
            }},

            { ItemType.Enhance, (itemData) =>
            {
                Debug.Log($"{itemData.itemName} 획득");
            }}
        };
    }

    [SerializeField] private bool isPlayerDead = false;
    public bool IsPlayerDead() { return isPlayerDead; }
    public delegate bool OnPlayerIsDead();

    public delegate void OnPlayerStateUpdate(bool isDead);
    public void PlayerStateUpdate(bool isDead) { isPlayerDead = isDead; }

    public void SubWorldHpEvent() { OnSubWorldHP?.Invoke(); }

    void OnDrawGizmos()
    {
        if (monsterMoveArea != null)
        {
            Gizmos.color = Color.grey;
            Gizmos.DrawWireCube(monsterMoveArea.pos + monsterMoveArea.offset, monsterMoveArea.size);
        }
    }

    private const int MONSTER_BASE_SORTING_ORDER = 10;

    private void SetSortingOrderByYPos(Character monster, bool isSpine)
    {
        int newSortingOrder = MONSTER_BASE_SORTING_ORDER + Mathf.RoundToInt(monster.transform.position.y * -100f);

        if (isSpine)
        {
            if (monster.skeleton != null)
            {
                var meshRenderer = monster.skeleton.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.sortingOrder = newSortingOrder;
                }
            }
        }
        else
        {
            var spriteRenderer = monster.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = newSortingOrder;
            }
        }
    }
}