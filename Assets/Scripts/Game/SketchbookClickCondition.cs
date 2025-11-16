using UnityEngine;

public class SketchbookClickCondition : MonoBehaviour, IClickCondition
{
    public bool IsClickAllowed()
    {
        if (WolfWorkStation.Instance != null)
        {
            return !WolfWorkStation.Instance.IsSketchbookLocked();
        }

        return false;
    }
}
