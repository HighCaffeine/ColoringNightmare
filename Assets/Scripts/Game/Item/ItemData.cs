using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Item/ItemData")]
public class ItemData : ScriptableObject
{
    public ItemType itemType;
    public Sprite itemImage;
    public string itemName;
}
