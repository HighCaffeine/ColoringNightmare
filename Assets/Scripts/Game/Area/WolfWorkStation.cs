using UnityEngine;

[ExecuteAlways]
public class WolfWorkStation : GenericSingleton<WolfWorkStation>
{
    [Header("Work Station Area")]
    [SerializeField] private AreaData paletteArea;
    [SerializeField] private AreaData sketchArea;
    [SerializeField] private AreaData enhanceArea;

    [Space(5f)]
    [Header("Palette Event")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnPaletteEnter;
    [SerializeField] private UnityEngine.Events.UnityEvent OnPaletteExit;

    [Space(5f)]
    [Header("Sketch Event")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnSketchEnter;
    [SerializeField] private UnityEngine.Events.UnityEvent OnSketchExit;

    [Space(5f)]
    [Header("Enhance Event")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnEnhanceEnter;
    [SerializeField] private UnityEngine.Events.UnityEvent OnEnhanceExit;

    // 영역별 캐릭터 상태
    private bool isInPalette = false;
    private bool isInSketch = false;
    private bool isInEnhance = false;

    void OnDrawGizmos()
    {
        if (paletteArea != null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(paletteArea.pos + paletteArea.offset, paletteArea.size);
        }
        if (sketchArea != null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(sketchArea.pos + sketchArea.offset, sketchArea.size);
        }
        if (enhanceArea != null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(enhanceArea.pos + enhanceArea.offset, enhanceArea.size);
        }
    }

    public void CheckAreaEnter(Vector2 pos)
    {
        // Palette Area
        if (paletteArea != null)
        {
            bool nowIn = IsInsideArea(pos, paletteArea);

            if (nowIn && !isInPalette)
            {
                isInPalette = true;
                OnPaletteEnter.Invoke();
            }
            else if (!nowIn && isInPalette)
            {
                isInPalette = false;
                OnPaletteExit.Invoke();
            }
        }

        // Sketch Area
        if (sketchArea != null)
        {
            bool nowIn = IsInsideArea(pos, sketchArea);

            if (nowIn && !isInSketch)
            {
                isInSketch = true;
                OnSketchEnter.Invoke();
            }
            else if (!nowIn && isInSketch)
            {
                isInSketch = false;
                OnSketchExit.Invoke();
            }
        }

        // Enhance Area
        if (enhanceArea != null)
        {
            bool nowIn = IsInsideArea(pos, enhanceArea);

            if (nowIn && !isInEnhance)
            {
                isInEnhance = true;
                OnEnhanceEnter.Invoke();
            }
            else if (!nowIn && isInEnhance)
            {
                isInEnhance = false;
                OnEnhanceExit.Invoke();
            }
        }
    }

    private bool IsInsideArea(Vector2 pos, AreaData area)
    {
        Vector2 center = area.pos + area.offset;
        Vector2 halfSize = area.size * 0.5f;

        return pos.x >= center.x - halfSize.x && pos.x <= center.x + halfSize.x && pos.y >= center.y - halfSize.y && pos.y <= center.y + halfSize.y;
    }
}
