using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public enum PatternGrade { Normal, Rare, Epic, Legendary };

public enum Design { Sword, Spear, Axe, Count }

public class WeaponPatternSelector : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] patterns;

    [SerializeField] private GameObject patternSelectionPanel;
    [SerializeField] private List<GameObject> checkDesigns;

    private Design selectedDesign = Design.Count;

    void Awake()
    {
        Init();
        ClosePatternSelector();
    }

    public void OpenPatternSelector()
    {
        patternSelectionPanel.SetActive(true);

        Init();
    }

    public void ClosePatternSelector()
    {
        patternSelectionPanel.SetActive(false);
    }

    public void Init()
    {
        selectedDesign = Design.Count;

        foreach (var patternRenderer in patterns)
        {
            patternRenderer?.gameObject.SetActive(false);
        }

        foreach (var checkDesign in checkDesigns)
        {
            checkDesign?.gameObject.SetActive(false);
        }

        DrawWeaponGPU.Instance.SetRefSprite(null);
    }

    public void SetSword() { SelectWeaponDesign(Design.Sword); }
    public void SetSpear() { SelectWeaponDesign(Design.Spear); }
    public void SetAxe() { SelectWeaponDesign(Design.Axe); }

    private void SelectWeaponDesign(Design design)
    {
        if (selectedDesign == design) return;

        selectedDesign = design;
        DrawWeaponGPU.Instance.SetRefSprite(patterns[Devcat.ValueCastTo<int>.From(selectedDesign)]);

        switch (design)
        {
            case Design.Sword:
                DrawWeaponGPU.Instance.SetWeaponType(WeaponManager.WeaponType.Sword);
                break;
            case Design.Spear:
                DrawWeaponGPU.Instance.SetWeaponType(WeaponManager.WeaponType.Spear);
                break;
            case Design.Axe:
                DrawWeaponGPU.Instance.SetWeaponType(WeaponManager.WeaponType.Axe);
                break;
        }

        patterns[Devcat.ValueCastTo<int>.From(selectedDesign)].gameObject.SetActive(true);

        WolfWorkStation.Instance.SetSketchType();
        WolfWorkStation.Instance.Interactive();



        Invoke(nameof(ClosePatternSelector), 0.5f);
    }
}
