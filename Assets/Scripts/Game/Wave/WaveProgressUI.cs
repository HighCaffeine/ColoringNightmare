using UnityEngine;
using UnityEngine.UI;

public class WaveProgressUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Image bossIcon;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color bossReadyColor = Color.red;

    private WaveManager waveManager;
    private int totalWaves;

    private void Start()
    {
        waveManager = WaveManager.Instance;

        if (waveManager != null)
        {
            totalWaves = waveManager.GetWaveCount();

            if (progressSlider != null)
            {
                progressSlider.maxValue = totalWaves;
                progressSlider.value = 0;
            }
        }
    }

    private void Update()
    {
        if (waveManager == null) return;

        int currentWaveIndex = waveManager.GetCurrentWaveIndex();

        if (progressSlider != null)
        {
            progressSlider.value = Mathf.Lerp(progressSlider.value, currentWaveIndex, Time.deltaTime * 5f);
        }

        if (currentWaveIndex >= totalWaves)
        {
            if (bossIcon != null) bossIcon.color = bossReadyColor;

            if (progressSlider != null) progressSlider.value = totalWaves;
        }
        else
        {
            if (bossIcon != null) bossIcon.color = normalColor;
        }
    }
}