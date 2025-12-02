using UnityEngine;

public class DreamCottonUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject enhancePanel; // 강화
    [SerializeField] private GameObject skillPanel;   // 스킬
    [SerializeField] private GameObject gachaPanel;   // 뽑기

    private void Start()
    {
        // 시작 시 모두 끄기
        CloseAll();
    }

    public void OpenEnhance()
    {
        CloseAll();
        enhancePanel.SetActive(true);
    }

    public void OpenSkill()
    {
        CloseAll();
        skillPanel.SetActive(true);
    }

    public void OpenGacha()
    {
        CloseAll();
        gachaPanel.SetActive(true);
    }

    public void CloseAll()
    {
        if(enhancePanel) enhancePanel.SetActive(false);
        if(skillPanel) skillPanel.SetActive(false);
        if(gachaPanel) gachaPanel.SetActive(false);
    }
}