using System;
using System.Collections;
using UnityEngine;

public class ItemController : MonoBehaviour, OnReturnPool<ItemController>
{
    OnReturnPoolEvent<ItemController> OnReturnPoolEvent;
    Action OnArrivalTargetPosEvent;

    [SerializeField] private SpriteRenderer spriteRenderer;

    public void Init(OnReturnPoolEvent<ItemController> onReturnPoolEvent)
    {
        this.OnReturnPoolEvent = onReturnPoolEvent;
    }

    public void SetItem(Sprite sprite, Action OnArrivalTargetPosEvent)
    {
        this.OnArrivalTargetPosEvent = OnArrivalTargetPosEvent;
        spriteRenderer.sprite = sprite;
    }

    public void InitEvent()
    {
        spriteRenderer.sprite = null;
        OnArrivalTargetPosEvent = null;

        OnReturnPoolEvent?.Invoke(this);
    }

    private Coroutine coroutine;
    public void MoveToTargetPos(Vector3 targetPos)
    {
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(MoveToTargetPosCoroutine(targetPos));
    }

    private IEnumerator MoveToTargetPosCoroutine(Vector3 targetPos)
    {
        float distance = float.MaxValue;
        float beforeDis = distance;

        while (distance >= 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                Time.deltaTime * DropManager.ItemMoveSpeed
            );

            distance = Vector3.Distance(transform.position, targetPos);

            if (beforeDis > distance)
            {
                break;
            }

            beforeDis = distance;

            yield return null;
        }

        OnArrivalTargetPosEvent?.Invoke();
        InitEvent();
    }
}
