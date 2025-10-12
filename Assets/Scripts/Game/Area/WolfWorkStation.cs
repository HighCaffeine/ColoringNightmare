using UnityEngine;

[ExecuteAlways]
public class WolfWorkStation : GenericSingleton<WolfWorkStation>
{
    private enum WorkStationType
    {
        None,
        Palette, Sketch, Enhance,
    }

    [Header("Work Station Area")]
    [SerializeField] private AreaData paletteArea;
    [SerializeField] private AreaData sketchArea;
    [SerializeField] private AreaData enhanceArea;

    [Space(5f)]
    [Header("Palette Event")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnPaletteEnter;
    [SerializeField] private UnityEngine.Events.UnityEvent OnPaletteExit;
    [SerializeField] private UnityEngine.Events.UnityEvent OnPalettePanelOn;
    [SerializeField] private UnityEngine.Events.UnityEvent OnPalettePanelOff;

    [Space(5f)]
    [Header("Sketch Event")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnSketchEnter;
    [SerializeField] private UnityEngine.Events.UnityEvent OnSketchExit;
    [SerializeField] private UnityEngine.Events.UnityEvent OnSketchOn;
    [SerializeField] private UnityEngine.Events.UnityEvent OnSketchOff;

    [Space(5f)]
    [Header("Enhance Event")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnEnhanceEnter;
    [SerializeField] private UnityEngine.Events.UnityEvent OnEnhanceExit;

    [SerializeField] private UnityEngine.Events.UnityEvent OnEnhancePanelOn;
    [SerializeField] private UnityEngine.Events.UnityEvent OnEnhancePanelOff;

    // 영역별 캐릭터 상태
    private bool isInPalette = false;
    private bool isInSketch = false;
    private bool isInEnhance = false;

    private WorkStationType workStationType = WorkStationType.None;
    private bool isOnInteractive = false;

    public bool IsOnInteractive => isOnInteractive;

    public void SetIsOnInteractive(bool isOnInteractive) { this.isOnInteractive = isOnInteractive; }

    public void SetPaletteType() { workStationType = WorkStationType.Palette; }
    public void SetEnhanceType() { workStationType = WorkStationType.Enhance; }
    public void SetSketchType() { workStationType = WorkStationType.Sketch; }

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

    public void AllWorkStationOff()
    {
        OnPalettePanelOff?.Invoke();
        OnSketchOff?.Invoke();
        OnEnhancePanelOff?.Invoke();
    }

    public void TEST_DisableDrawing()
    {
        OnSketchOff?.Invoke();
    }

    public void Interactive()
    {
        switch (workStationType)
        {
            case WorkStationType.None:
                break;
            case WorkStationType.Palette:
                if (isOnInteractive) OnPalettePanelOn?.Invoke();
                else OnPalettePanelOff?.Invoke();
                break;
            case WorkStationType.Sketch:
                if (isOnInteractive) OnSketchOn?.Invoke();
                else OnSketchOff?.Invoke();
                break;
            case WorkStationType.Enhance:
                if (isOnInteractive) OnEnhancePanelOn?.Invoke();
                else OnEnhancePanelOff?.Invoke();
                break;
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
                workStationType = WorkStationType.Palette;
                isInPalette = true;
                OnPaletteEnter.Invoke();
            }
            else if (!nowIn && isInPalette)
            {
                workStationType = WorkStationType.Palette;
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
                workStationType = WorkStationType.Sketch;
                isInSketch = true;
                OnSketchEnter.Invoke();
            }
            else if (!nowIn && isInSketch)
            {
                workStationType = WorkStationType.None;
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
                workStationType = WorkStationType.Enhance;
                isInEnhance = true;
                OnEnhanceEnter.Invoke();
            }
            else if (!nowIn && isInEnhance)
            {
                workStationType = WorkStationType.None;
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
