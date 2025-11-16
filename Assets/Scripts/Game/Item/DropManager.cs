using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum ItemType
{
    Ink,
    Enhance,
    Count,
}

public class DropManager : ObjectPooling<DropManager, ItemController>
{
    [SerializeField] private Transform itemGetPos;

    [Range(1.0f, 10.0f)][SerializeField] private float ItemMoveSpeed = 5;

    public float GetItemMoveSpeed() => ItemMoveSpeed;

    [Tooltip("몬스터 위치에서 얼마나 떨어진 곳에 스폰될지 최대 반경")]
    [SerializeField] private float spawnOffsetRadius = 0.5f;

    protected new void Awake()
    {
        base.Awake();

        storageParent.gameObject.SetActive(true);
    }

    // 최소치에 더 가깝게 랜덤값 뽑음 weight값(제곱 값으로 넣음)
    public static int GetRandomMinSide(int min, int max, float weight = 2.0f)
    {
        float t = Random.value;
        float _weight = Mathf.Pow(t, weight);
        int range = max - min + 1;
        int result = min + Mathf.FloorToInt(range * _weight);

        return Mathf.Min(result, max);
    }

    public Transform TestPos_Cotton;
    public Transform TestPos_Red;
    public Transform TestPos_Black;
    public Transform TestPos_Blue;
    public Transform TestPos_Yellow;

    public Transform TestPos_Gacha;
    private bool gacha = false;
    public void TransGachaPos()
    {
        gacha = !gacha;
    }

    public void ProcessLootTable(ItemDropTable lootTable, Vector3 spawnOrigin, Dictionary<ItemType, System.Action<ItemData>> eventCallbacks)
    {
        if (lootTable.itemDropTable == null)
        {
            Debug.Log($"drop table 없음");
        }
        foreach (ItemDropData itemDropData in lootTable.itemDropTable)
        {
            if (itemDropData.itemData == null) continue;

            int count = GetRandomMinSide(itemDropData.minDropAmount, itemDropData.maxDropAmount);

            for (int i = 0; i < count; i++)
            {
                if (Random.value <= itemDropData.dropChance)
                {
                    Vector3 TestPos = Vector3.zero;
                    switch (itemDropData.itemData.itemType)
                    {
                        case ItemType.Ink:
                            InkItem inkData = itemDropData.itemData as InkItem;
                            switch (inkData.colorType)
                            {
                                case ColorMixer.ColorType.Black:
                                    TestPos = TestPos_Black.position;
                                    break;
                                case ColorMixer.ColorType.Red:
                                    TestPos = TestPos_Red.position;
                                    break;
                                case ColorMixer.ColorType.Yellow:
                                    TestPos = TestPos_Yellow.position;
                                    break;
                                case ColorMixer.ColorType.Blue:
                                    TestPos = TestPos_Blue.position;
                                    break;
                            }
                            break;
                        case ItemType.Enhance:
                            TestPos = TestPos_Cotton.position;
                            break;
                    }

                    Vector2 randomOffset = Random.insideUnitCircle * spawnOffsetRadius;
                    Vector3 spawnPosition = gacha ? TestPos_Gacha.position + (Vector3)randomOffset : spawnOrigin + (Vector3)randomOffset;

                    eventCallbacks.TryGetValue(itemDropData.itemData.itemType, out System.Action<ItemData> OnEvent);

                    SpawnItem(itemDropData.itemData, spawnPosition, TestPos, OnEvent);
                }
            }
        }
    }

    private void SpawnItem(ItemData data, Vector3 spawnPosition, Vector3 targetPos, System.Action<ItemData> OnEvent)
    {
        ItemController item = GetPool();
        item.transform.position = spawnPosition;

        item.SetItem(data, OnEvent);
        item.MoveToTarget(targetPos);
    }

    public void SpawnInk(ItemData data, ColorMixer.ColorType colorType, Vector3 spawnPosition, Vector3 targetPos, System.Action<ItemData> OnEvent)
    {
        SpawnItem(data, spawnPosition, targetPos, OnEvent);
    }
}
