using UnityEngine;

public class GachaTest : MonoBehaviour
{
    public ItemDropTable testTable;
    public Transform testPos;

    public void DropTestTable()
    {
        MonsterManager.Instance.TestItemDrop(testTable, testPos.position);
    }
}
