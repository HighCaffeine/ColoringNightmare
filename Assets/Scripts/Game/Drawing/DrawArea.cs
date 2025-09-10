using UnityEngine;

[ExecuteAlways]
public class LineDrawArea : MonoBehaviour
{
    public Vector3 size = new Vector3(5, 5, 0);
    public Vector3 offset = Vector3.zero;

    public Bounds GetBounds()
    {
        return new Bounds(transform.position + offset, size);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + offset, size);
    }
}