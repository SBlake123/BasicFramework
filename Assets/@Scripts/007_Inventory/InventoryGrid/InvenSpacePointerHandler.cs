using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum InventoryEmpty
{
    Empty,
    NotEmpty
}


public class InvenSpacePointerHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IEndDragHandler, IBeginDragHandler, IPointerClickHandler, IInitializePotentialDragHandler
{
    public RectTransform dragObject { get; set; }

    public InventoryGrid_000_Base inventoryGrid_000_Base; //데이터 불러오기

    private int dragVersion;
    private bool dragging;

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = true;
        EventSystem.current.pixelDragThreshold = Mathf.Max(25, EventSystem.current.pixelDragThreshold);
    }

    public async void OnBeginDrag(PointerEventData eventData)
    {
        var owner = inventoryGrid_000_Base.Inventory_000_Base;
        var item = inventoryGrid_000_Base.itemData;
        if (item == null || owner == null || owner.IsMovingItem) return;
        CancelDrag();
        dragging = true;
        int version = dragVersion;
        try
        {
            var instance = await ObjectPool.Instance.PopFromPool(
                string.Format(GScriptAddress.invenItem, item.itemKey), owner.DragCanvas.transform);
            if (this == null || !isActiveAndEnabled || !dragging || version != dragVersion)
            {
                ObjectPool.Instance.PushToPool(instance);
                return;
            }
            dragObject = instance.GetComponent<RectTransform>();
            dragObject.SetAsLastSibling();
            if (!instance.TryGetComponent<CanvasGroup>(out var group))
                group = instance.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            MoveDragObject(eventData);
        }
        catch (Exception error)
        {
            if (this != null && version == dragVersion) CancelDrag();
            Debug.LogException(error);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        inventoryGrid_000_Base.Inventory_000_Base.itemConfigWindow.gameObject.SetActive(false);
        MoveDragObject(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        bool ready = dragObject != null && dragging;
        var hit = eventData.pointerCurrentRaycast.gameObject;
        var target = hit != null ? hit.GetComponentInParent<InventoryGrid_000_Base>() : null;
        CancelDrag();
        if (ready && target != null)
            inventoryGrid_000_Base.Inventory_000_Base.ItemMoveCheck(inventoryGrid_000_Base, target).Forget();
    }

    public void CancelDrag()
    {
        dragging = false;
        dragVersion++;
        if (dragObject == null) return;
        var instance = dragObject.gameObject;
        dragObject = null;
        ObjectPool.Instance.PushToPool(instance);
    }

    private void OnDisable() => CancelDrag();

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (inventoryGrid_000_Base.itemData)
        {
            case not null:
                {
                    //무기나 아머류면 더블클릭하면 장착
                    //악세서리면 안 됩니다.
                    break;
                }
            default:
                {

                    break;
                }
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        switch (inventoryGrid_000_Base.itemData)
        {
            case not null:
                {
                    inventoryGrid_000_Base.Inventory_000_Base.SettingItemConfigWindow(inventoryGrid_000_Base.itemData);

                    var window = inventoryGrid_000_Base.Inventory_000_Base.itemConfigWindow;


                    RectTransform gridRect =
                        (RectTransform)inventoryGrid_000_Base.transform;

                    RectTransform windowRect =
                        (RectTransform)window.transform;

                    Canvas canvas = gridRect.GetComponentInParent<Canvas>().rootCanvas;

                    Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                        ? null
                        : canvas.worldCamera;

                    // Grid 중심이 화면 위쪽인지 확인
                    Vector3 gridCenter = gridRect.TransformPoint(gridRect.rect.center);
                    Vector2 screenPosition =
                        RectTransformUtility.WorldToScreenPoint(uiCamera, gridCenter);

                    bool isUpperHalf = screenPosition.y >= Screen.height * 0.5f;

                    Vector3[] corners = new Vector3[4];
                    gridRect.GetWorldCorners(corners);

                    // 위쪽: 창의 오른쪽 위를 기준으로 아래로 펼침
                    // 아래쪽: 창의 오른쪽 아래를 기준으로 위로 펼침
                    windowRect.pivot = new Vector2(1f, isUpperHalf ? 1f : 0f);

                    window.gameObject.SetActive(true);

                    // corners[1]: Grid 왼쪽 위 / corners[0]: Grid 왼쪽 아래
                    windowRect.position = isUpperHalf ? corners[1] : corners[0];

                    // 왼쪽으로 간격, 위/아래로 약간 이동
                    windowRect.anchoredPosition += new Vector2(
                        -10f,
                        isUpperHalf ? -10f : 10f
                    );

                    inventoryGrid_000_Base.Inventory_000_Base.itemConfigWindow.gameObject.SetActive(true);
                    break;
                }
            default:
                {

                    break;
                }
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        switch (inventoryGrid_000_Base.itemData)
        {
            case not null:
                {
                    inventoryGrid_000_Base.Inventory_000_Base.itemConfigWindow.gameObject.SetActive(false);

                    break;
                }
            default:
                {

                    break;
                }
        }
    }
    private void MoveDragObject(PointerEventData eventData)
    {
        if (dragObject == null) return;

        var canvasRect = (RectTransform)inventoryGrid_000_Base.Inventory_000_Base.DragCanvas.transform;

        var camera = inventoryGrid_000_Base.Inventory_000_Base.DragCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : inventoryGrid_000_Base.Inventory_000_Base.DragCanvas.worldCamera;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRect,
            eventData.position,
            camera,
            out var position
        );

        dragObject.position = position;
    }
    private void MoveItem(InventoryGrid_000_Base targetGrid)
    {
        inventoryGrid_000_Base.Inventory_000_Base.ItemMoveCheck(inventoryGrid_000_Base, targetGrid).Forget();
    }
}
