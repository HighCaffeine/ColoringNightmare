using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CharacterArea : MonoBehaviour
{
    [SerializeField] private AreaData sheepArea;
    [SerializeField] private AreaData wolfArea;


    public Bounds GetSheepArea() => sheepArea.GetBounds();
    public Bounds GetWolfArea() => wolfArea.GetBounds();

    void OnDrawGizmos()
    {
        if (sheepArea != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(sheepArea.pos + sheepArea.offset, sheepArea.size);
        }

        if (wolfArea != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(wolfArea.pos + wolfArea.offset, wolfArea.size);
        }
    }
}
