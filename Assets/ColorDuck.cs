using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorDuck : MonoBehaviour
{
    [SerializeField] private SpriteRenderer duckSpriteRenderer;

    private ColorMixer.ColorType colorType;
    public void StartMoving(List<Transform> points, ColorMixer.ColorType colorType)
    {
        this.colorType = colorType;
        StartCoroutine(MoveAllPoints(points));
    }

    private IEnumerator MoveAllPoints(List<Transform> points)
    {
        foreach (Transform point in points)
        {
            yield return StartCoroutine(MoveToRailPointCoroutine(point.position));
        }
        OnRailFinished();
    }

    private void OnRailFinished()
    {
        Debug.Log(gameObject.name + " 도착 완료.");

        RailController.Instance.OnDuckFinished(colorType);

        gameObject.SetActive(false);
    }

    private IEnumerator MoveToRailPointCoroutine(Vector3 point)
    {
        float distance = Vector3.Distance(transform.position, point);
        float beforeDistance = distance;

        while (distance > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                point,
                RailController.Instance.RailSpeed() * Time.deltaTime
            );

            distance = Vector3.Distance(transform.position, point);
            if (beforeDistance < distance)
            {
                break;
            }
            beforeDistance = distance;
            yield return null;
        }
    }

    public void SetColor(Color color)
    {
        if (duckSpriteRenderer != null)
        {
            duckSpriteRenderer.color = color;
        }
    }
}