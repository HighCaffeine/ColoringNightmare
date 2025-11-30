using UnityEngine;
using System.Collections.Generic;

public enum Design { Sword, Spear, Axe, Count }

[System.Serializable]
public struct DesignPatternGroup
{
    public Design type;
    [Tooltip("이 타입에 해당하는 3개의 도안을 넣으세요")]
    public List<SpriteRenderer> patterns;
}

public class WeaponPatternSelector : GenericSingleton<WeaponPatternSelector>
{
    [Header("Pattern Configuration")]
    [SerializeField] private List<DesignPatternGroup> designPatterns;

    [SerializeField] private GameObject patternSelectionPanel;
    [SerializeField] private List<GameObject> checkDesigns;

    private Design selectedDesign = Design.Count;
    private SpriteRenderer currentActivePattern; // 현재 활성화된 랜덤 도안

    private new void Awake()
    {
        base.Awake();
        Init();
        ClosePatternSelector();
    }

    public void OpenPatternSelector() { patternSelectionPanel.SetActive(true); Init(); }
    public void ClosePatternSelector() { patternSelectionPanel.SetActive(false); }

    public void Init()
    {
        selectedDesign = Design.Count;
        currentActivePattern = null;

        foreach (var group in designPatterns)
        {
            if (group.patterns != null)
            {
                foreach (var p in group.patterns)
                {
                    if (p != null) p.gameObject.SetActive(false);
                }
            }
        }

        foreach (var c in checkDesigns) c?.gameObject.SetActive(false);
        DrawWeapon.Instance.SetRefSprite(null);
    }

    public void SetSword() { SelectWeaponDesign(Design.Sword); }
    public void SetSpear() { SelectWeaponDesign(Design.Spear); }
    public void SetAxe() { SelectWeaponDesign(Design.Axe); }

    private void SelectWeaponDesign(Design design)
    {
        if (selectedDesign == design) return;

        if (currentActivePattern != null)
        {
            currentActivePattern.gameObject.SetActive(false);
            currentActivePattern = null;
        }

        if ((int)selectedDesign >= 0 && (int)selectedDesign < checkDesigns.Count)
        {
            checkDesigns[(int)selectedDesign].gameObject.SetActive(false);
        }

        selectedDesign = design;

        DesignPatternGroup group = designPatterns.Find(x => x.type == design);
        if (group.patterns != null && group.patterns.Count > 0)
        {
            int randomIndex = Random.Range(0, group.patterns.Count);
            currentActivePattern = group.patterns[randomIndex];

            if (currentActivePattern != null)
            {
                currentActivePattern.gameObject.SetActive(true);
                DrawWeapon.Instance.SetRefSprite(currentActivePattern);
            }
        }

        int index = (int)selectedDesign;
        if (index >= 0 && index < checkDesigns.Count)
        {
            checkDesigns[index].gameObject.SetActive(true);
        }
    }

    public WeaponManager.WeaponType GetSelectedWeaponType()
    {
        switch (selectedDesign)
        {
            case Design.Spear: return WeaponManager.WeaponType.Spear;
            case Design.Axe: return WeaponManager.WeaponType.Axe;
            default: return WeaponManager.WeaponType.Sword;
        }
    }
}