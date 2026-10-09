using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum InventoryEmpty
{
    Empty,
    NotEmpty
}


public class InvenSpacePointerHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IEndDragHandler, IBeginDragHandler, IPointerClickHandler
{
    public RectTransform dragObject { get; set; }

    public InventoryGrid_000_Base inventoryGrid_000_Base; //데이터 불러오기

    public async void OnBeginDrag(PointerEventData eventData)
    {
        switch (inventoryGrid_000_Base.itemData)
        {
            case not null:
                {
                    //var prefab = Resources.Load<GameObject>("Sword");
                    //GameObject instance = ObjectPool.Instance.PopFromPool(string.Format(GScriptAddress.invenItem, inventoryGrid_000_Base.itemData.itemKey), IngameUIManager.Instance.hudCanvas.transform);
                    var instance = await ObjectPool.Instance.PopFromPool(string.Format(GScriptAddress.invenItem, inventoryGrid_000_Base.itemData.itemKey), IngameUIManager.Instance.hudCanvas.transform);
                   // var instance = Instantiate(inventoryGrid_000_Base.itemImgParent.GetChild(0).gameObject, IngameUIManager.Instance.hudCanvas.transform, false);

                    //var instance = Instantiate(prefab, IngameUIManager.Instance.hudCanvas.transform, false);
                    dragObject = instance.GetComponent<RectTransform>();
                    dragObject.SetAsLastSibling();

                    // 드래그 이미지가 아래 슬롯의 포인터 이벤트를 막지 않도록 설정

                    CanvasGroup group;

                    switch (instance.TryGetComponent<CanvasGroup>(out group))
                    {
                        case false:
                            {
                                group = instance.AddComponent<CanvasGroup>();
                                break;
                            }
                    }

                    group.blocksRaycasts = false;

                    MoveDragObject(eventData);
                    break;
                }
            default:
                {

                    break;
                }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        //설명 끄기
        inventoryGrid_000_Base.Inventory_000_Base.itemConfigWindow.gameObject.SetActive(false);

        switch (inventoryGrid_000_Base.itemData)
        {
            case not null:
                {
                    
                    MoveDragObject(eventData);
                    break;
                }
            default:
                {

                    break;
                }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        switch (dragObject)
        {
            case null:
                return;
        }

        var hitObject = eventData.pointerCurrentRaycast.gameObject;

        var targetGrid = hitObject != null
            ? hitObject.GetComponentInParent<InventoryGrid_000_Base>()
            : null;

        switch (targetGrid)
        {
            case not null:
                {

                    switch (targetGrid == inventoryGrid_000_Base)
                    {
                        case false:
                            MoveItem(targetGrid);
                            break;
                    }
                    ObjectPool.Instance.PushToPool(dragObject.gameObject);
                    dragObject = null;
                    break;
                }
            default:
                {
                    ObjectPool.Instance.PushToPool(dragObject.gameObject);
                    dragObject = null;
                    break;
                }
        }
    }

    private void MoveDragObject(PointerEventData eventData)
    {
        if (dragObject == null) return;

        var canvasRect = (RectTransform)IngameUIManager.Instance.hudCanvas.transform;

        var camera = IngameUIManager.Instance.hudCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : IngameUIManager.Instance.hudCanvas.worldCamera;

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
        inventoryGrid_000_Base.Inventory_000_Base.ItemMoveCheck(inventoryGrid_000_Base, targetGrid);       
    }

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
}
