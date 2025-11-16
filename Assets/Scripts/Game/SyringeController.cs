using UnityEngine;

public class SyringeController : MonoBehaviour
{
    [SerializeField] private Transform minPoint;

    private Vector3 defaultPos;

    private int maxCount => ColorMixer.MAX_INK_COUNT;
    private int currentAmount;

    void Awake()
    {
        currentAmount = 0;
        defaultPos = transform.position;
    }

    public void UpdateSyringe()
    {
        float fillPercent = 0f;
        if (maxCount > 0)
        {
            fillPercent = (float)currentAmount / maxCount;
        }

        transform.position = Vector3.Lerp(defaultPos, minPoint.position, fillPercent);
    }

    public bool IsAllowUse()
    {
        if (currentAmount > 0) return false;
        return true;
    }

    public bool AddInk()
    {
        if (currentAmount >= maxCount) return false;

        currentAmount++;
        return true;
    }

    public bool UseInk()
    {
        if (currentAmount > 0) return false;

        currentAmount--;
        return true;
    }
}
