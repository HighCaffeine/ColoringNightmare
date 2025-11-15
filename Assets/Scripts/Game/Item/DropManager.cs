using UnityEngine;
using System.Collections.Generic;

public enum ItemType
{
    Ink,
    Enhance,
    Count,
}

public class DropManager : ObjectPooling<DropManager, ItemController>
{
    [SerializeField] private List<ItemData> items;
    public static float ItemMoveSpeed = 5;

    public void SpawnItem(ItemType itemType)
    {
        ItemController item = GetPool();
    }

    public void SpawnInk(ItemType itemType, ColorMixer.ColorType colorType)
    {

    }
}
