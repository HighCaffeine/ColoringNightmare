using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Ink/InkData")]
public class InkData : ScriptableObject
{
    [Header("Ink data")]
    public ColorMixer.ColorType color;
    public Color GetColor() => ColorMixer.Instance.GetColor(color);
}