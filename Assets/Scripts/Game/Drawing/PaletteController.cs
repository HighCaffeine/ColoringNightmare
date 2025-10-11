using UnityEngine;

public class PaletteController : MonoBehaviour
{
    [SerializeField] private UnityEngine.Events.UnityEvent OnPaletteOnEvent;
    [SerializeField] private UnityEngine.Events.UnityEvent OnPaletteOffEvent;

    public void OnPaletteOn() { OnPaletteOnEvent?.Invoke(); }
    public void OnPaletteOff() { OnPaletteOffEvent?.Invoke(); }
}
