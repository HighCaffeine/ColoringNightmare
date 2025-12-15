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

        if (setCoroutine != null) StopCoroutine(setCoroutine);
        setCoroutine = StartCoroutine(SetItemCoroutine());
    }

    private Coroutine setCoroutine;

    private IEnumerator SetItemCoroutine()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.5f));
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

    private Coroutine moveCoroutine;

    public void MoveToTarget(Vector3 targetPos)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveToTargetCoroutine(targetPos));
    }

    private IEnumerator MoveToTargetCoroutine(Vector3 targetPos)
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.5f));

        while (targetPos != Vector3.zero)
        {
            float distance = Vector3.Distance(transform.position, targetPos);

            if (distance < 0.1f)
            {
                break;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                Time.deltaTime * DropManager.Instance.GetItemMoveSpeed()
            );

            yield return null;
        }

        OnArrivalTargetPosEvent?.Invoke(currentItem);
        InitEvent();
    }
}