using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BossUIController : GenericSingleton<BossUIController>
{
    [SerializeField] private Transform hpImageTarget;

    [Header("HP Bar")]
    [SerializeField] private Image hpBarImage;

    [Header("Groggy Coins")]
    [SerializeField] private List<Image> coinImages;

    [SerializeField] private Sprite coinNormalSprite; // 정상 코인
    [SerializeField] private Sprite coinBrokenSprite; // 깨진 코인

    private new void Awake()
    {
        SetActiveBossHP(false);
    }

    public void SetActiveBossHP(bool isActive)
    {
        hpImageTarget.gameObject.SetActive(isActive);
    }

    public void Init(int maxHp, int maxCoins)
    {
        UpdateHP(maxHp, maxHp);
        UpdateCoins(maxCoins);
    }

    public void UpdateHP(float currentHp, float maxHp)
    {
        if (hpBarImage != null)
        {
            hpBarImage.fillAmount = currentHp / maxHp;
        }
    }

    public void UpdateCoins(int currentCoinCount)
    {
        if (coinImages == null || coinNormalSprite == null || coinBrokenSprite == null) return;

        for (int i = 0; i < coinImages.Count; i++)
        {
            if (i < currentCoinCount)
            {
                coinImages[i].sprite = coinNormalSprite;
            }
            else
            {
                coinImages[i].sprite = coinBrokenSprite;
            }
        }
    }
}