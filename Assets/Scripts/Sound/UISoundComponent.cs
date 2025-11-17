using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI에 부착하여 마우스 이벤트 시 사운드를 재생하는 컴포넌트
/// </summary>
public class UISoundComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Sound Settings")]
    [SerializeField] private bool useEnterSound = true;
    [SerializeField] private bool useExitSound = false;
    [SerializeField] private bool useClickSound = true;

    [Header("Sound Names")]
    [Tooltip("마우스 Enter 시 재생할 사운드")]
    [SerializeField] private string enterSoundName = "UI_Default";

    [Tooltip("마우스 Exit 시 재생할 사운드")]
    [SerializeField] private string exitSoundName = "UI_Default";

    [Tooltip("클릭 시 재생할 사운드")]
    [SerializeField] private string clickSoundName = "UI_Default";

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (useEnterSound && !string.IsNullOrEmpty(enterSoundName))
        {
            PlaySound(enterSoundName);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (useExitSound && !string.IsNullOrEmpty(exitSoundName))
        {
            PlaySound(exitSoundName);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (useClickSound && !string.IsNullOrEmpty(clickSoundName))
        {
            PlaySound(clickSoundName);
        }
    }

    private void PlaySound(string soundName)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(soundName, false);
        }
        else
        {
            Debug.LogWarning($"[UISoundComponent] SoundManager가 없습니다. {soundName}을 재생할 수 없습니다.");
        }
    }

    // 코드에서 직접 사운드 재생이 필요한 경우
    public void PlayEnterSound()
    {
        if (useEnterSound) PlaySound(enterSoundName);
    }

    public void PlayExitSound()
    {
        if (useExitSound) PlaySound(exitSoundName);
    }

    public void PlayClickSound()
    {
        if (useClickSound) PlaySound(clickSoundName);
    }

    // 런타임에 사운드 이름 변경
    public void SetEnterSound(string soundName)
    {
        enterSoundName = soundName;
    }

    public void SetExitSound(string soundName)
    {
        exitSoundName = soundName;
    }

    public void SetClickSound(string soundName)
    {
        clickSoundName = soundName;
    }
}