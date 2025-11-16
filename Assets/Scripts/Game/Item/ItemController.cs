using System;
using System.Collections;
using UnityEngine;

public class ItemController : MonoBehaviour, OnReturnPool<ItemController>
{
    OnReturnPoolEvent<ItemController> OnReturnPoolEvent;
    Action<ItemData> OnArrivalTargetPosEvent;

    private ItemData currentItem;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TrailRenderer trailRenderer;

    public void Init(OnReturnPoolEvent<ItemController> onReturnPoolEvent)
    {
        this.OnReturnPoolEvent = onReturnPoolEvent;
    }

    public void SetItem(ItemData itemData, Action<ItemData> OnArrivalTargetPosEvent)
    {
        this.OnArrivalTargetPosEvent = OnArrivalTargetPosEvent;
        spriteRenderer.sprite = itemData.itemImage;

        currentItem = itemData;

        // 풀에서 나올 때 렌더러 활성화
        spriteRenderer.enabled = true;
        trailRenderer.enabled = true;
    }

    public void InitEvent()
    {
        currentItem = null;
        spriteRenderer.sprite = null;
        OnArrivalTargetPosEvent = null;

        trailRenderer.Clear();
        spriteRenderer.enabled = false;
        trailRenderer.enabled = false;

        OnReturnPoolEvent?.Invoke(this);
    }

    private Coroutine coroutine;

    public void MoveToTarget(Transform targetTransform)
    {
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(MoveToTargetCoroutine(targetTransform));
    }

    private IEnumerator MoveToTargetCoroutine(Transform targetTransform)
    {
        while (targetTransform != null)
        {
            Vector3 targetPos = targetTransform.position;

            float distance = Vector3.Distance(transform.position, targetPos);

            if (distance < 0.1f)
            {
                break;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                Time.deltaTime * DropManager.ItemMoveSpeed
            );

            yield return null;
        }

        OnArrivalTargetPosEvent?.Invoke(currentItem);
        InitEvent();
    }
}