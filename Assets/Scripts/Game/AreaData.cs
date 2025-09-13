using UnityEngine;

[CreateAssetMenu(menuName = "Area/AreaData")]
public class AreaData : ScriptableObject
{
    public Vector3 pos = new Vector3(0, 0, 0);
    public Vector3 size = new Vector3(5, 5, 0);
    public Vector3 offset = Vector3.zero;

    public Bounds GetBounds()
    {
        return new Bounds(pos + offset, size);
    }
}