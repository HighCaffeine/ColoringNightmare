using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailController : GenericSingleton<RailController>
{
    [SerializeField] private float railSpeed;
    [SerializeField] private ColorDuck color1Duck;   // c1 오리
    [SerializeField] private ColorDuck color2Duck;  // c2 오리

    [Tooltip("오리가 순서대로 이동할 지점(Transform) 목록")]
    [Header("(black, red, yellow, blue, white)")]
    [SerializeField] private List<Transform> railPoints;

    [SerializeField] private List<Transform> spawnPoint;

    private int ducksFinished = 0;

    public void StartDuckSequence(ColorMixer.ColorType c1, ColorMixer.ColorType c2)
    {
        StartCoroutine(StartDuckCoroutine(c1, c2));
    }

    private IEnumerator StartDuckCoroutine(ColorMixer.ColorType c1, ColorMixer.ColorType c2)
    {
        ducksFinished = 0;

        // 오리 활성화 및 색상 설정
        color1Duck.gameObject.SetActive(true);
        color1Duck.transform.position = spawnPoint[Devcat.ValueCastTo<int>.From(c1)].position;
        color1Duck.SetColor(ColorMixer.Instance.GetColor(c1));

        color1Duck.StartMoving(railPoints, c1);
        if (c1 == c2) yield return new WaitForSeconds(1f);    //조금 텀

        color2Duck.gameObject.SetActive(true);
        color2Duck.transform.position = spawnPoint[Devcat.ValueCastTo<int>.From(c2)].position;
        color2Duck.SetColor(ColorMixer.Instance.GetColor(c2));

        // 두 오리 이동 시작
        color2Duck.StartMoving(railPoints, c2);

        yield return null;
    }

    public void OnDuckFinished(ColorMixer.ColorType colorType)
    {
        ducksFinished++;

        if (ducksFinished == 1)
        {
            MixerButtonController.Instance.SetFirstColor(colorType);
        }

        if (ducksFinished >= 2)
        {
            WolfWorkStation.Instance.SetSkechBookLock(false);
            Debug.Log("모든 오리 도착 스케치북 잠금이 해제");

            MixerButtonController.Instance.SetSecondColor();
        }
    }

    public float RailSpeed() => railSpeed;
}