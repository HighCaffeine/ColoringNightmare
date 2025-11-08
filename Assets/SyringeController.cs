using UnityEngine;

public class SyringeController : MonoBehaviour
{
    [SerializeField] private Transform minPoint;

    private int maxCount => ColorMixer.MAX_INK_COUNT;
}
