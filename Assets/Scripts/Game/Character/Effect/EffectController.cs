using UnityEngine;


public enum EffectType
{
    Attack,
}

public class EffectController : MonoBehaviour
{
    [SerializeField] private GameObject effectPrefab;

    [SerializeField] private Transform effectPivot;


    [Header("TEST")]
    [SerializeField] private EffectData test_data;

    public void PlayEffect()
    {
        GameObject effectInstance = Instantiate(effectPrefab, transform.position, Quaternion.identity);

        effectInstance.transform.position = effectPivot.position;

        effectInstance.GetComponent<EffectPlayer>().Play(test_data);
    }
}

