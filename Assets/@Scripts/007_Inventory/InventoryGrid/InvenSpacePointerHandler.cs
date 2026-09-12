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

    public void OnBeginDrag(PointerEventData eventData)
    {
        switch (inventoryGrid_000_Base.itemData)
        {
            case not null:
                {
                    //var prefab = Resources.Load<GameObject>("Sword");
                    var instance = Instantiate(inventoryGrid_000_Base.itemImgParent.GetChild(0).gameObject, IngameUIManager.Instance.hudCanvas.transform, false);

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
                    Destroy(dragObject.gameObject);
                    dragObject = null;
                    break;
                }
            default:
                {
                    Destroy(dragObject.gameObject);
                    dragObject = null;
                    break;
                }
        }
    }

    private void MoveDragObject(PointerEventData eventData)
    {
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
        switch (targetGrid.itemData)
        {
            case null:
                {
                    targetGrid.itemData = inventoryGrid_000_Base.itemData;
                    Instantiate(inventoryGrid_000_Base.itemImgParent.GetChild(0).gameObject, targetGrid.itemImgParent);

                    Destroy(inventoryGrid_000_Base.itemImgParent.GetChild(0).gameObject);
                    inventoryGrid_000_Base.itemData = null;



                    break;
                }
        }
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
        //switch (inventoryGrid_000_Base.itemData)
        //{
        //    case not null:
        //        {
        //            //아이템 설명
        //            break;
        //        }
        //    default:
        //        {

        //            break;
        //        }
        //}
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //switch (inventoryGrid_000_Base.itemData)
        //{
        //    case not null:
        //        {
        //            //아이템 이미지 생성
        //            break;
        //        }
        //    default:
        //        {

        //            break;
        //        }
        //}
    }
}
