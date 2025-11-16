using UnityEngine;

public class SyringeController : MonoBehaviour
{
    [SerializeField] private Transform minPoint;
    [SerializeField] private Transform moveTransform;

    private Vector3 defaultPos;

    private int maxCount => ColorMixer.MAX_INK_COUNT;
    private int currentAmount;

    void Awake()
    {
        currentAmount = 1;
        defaultPos = moveTransform.localPosition;

        UpdateSyringe();
    }

    public void UpdateSyringe()
    {
        float fillPercent = 0f;
        if (maxCount > 0)
        {
            fillPercent = (float)currentAmount / maxCount;
        }

        moveTransform.localPosition = Vector3.Lerp(minPoint.localPosition, defaultPos, fillPercent);
    }

    public bool IsAllowUse()
    {
        if (currentAmount > 0) return true;
        return false;
    }

    public bool AddInk()
    {
        if (currentAmount >= maxCount) return false;

        currentAmount++;
        UpdateSyringe();
        return true;
    }

    public bool UseInk()
    {
        if (currentAmount <= 0) return false;
        currentAmount--;
        UpdateSyringe();
        return true;
    }
}