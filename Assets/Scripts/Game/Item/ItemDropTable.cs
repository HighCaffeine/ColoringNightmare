using System;
using UnityEngine;

[Serializable]
public struct ItemDropData
{
    public ItemData itemData;

    [Range(0f, 1f)]
    public float dropChance;
    public int minDropAmount;
    public int maxDropAmount;
}

[CreateAssetMenu(fileName = "ItemDropTable", menuName = "Item/ItemDropTable")]
public class ItemDropTable : ScriptableObject
{
    public System.Collections.Generic.List<ItemDropData> itemDropTable;
}
