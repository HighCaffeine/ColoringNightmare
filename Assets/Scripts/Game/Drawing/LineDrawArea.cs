using UnityEngine;

[ExecuteAlways]
public class LineDrawArea : MonoBehaviour
{
    [SerializeField] private AreaData areaData;

    public Bounds GetBounds()
    {
        return areaData.GetBounds();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(areaData.pos + areaData.offset, areaData.size);
    }
}