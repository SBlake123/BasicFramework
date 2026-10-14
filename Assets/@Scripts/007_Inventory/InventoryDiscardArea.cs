using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDiscardArea : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        if (!eventData.pointerDrag.TryGetComponent<InvenSpacePointerHandler>(out var handler)) return;
        if (handler.inventoryGrid_000_Base.itemData == null) return;
        //handler.inventoryGrid_000_Base.


        PopupManager.Instance.setPopUpCode(true, "아이템을 버리시겠습니까?", "네", "아니오");
        PopupManager.Instance.AddMethodToBtn(() => {
            ObjectPool.Instance.PushToPool(handler.inventoryGrid_000_Base.itemImgParent.GetChild(0).gameObject);
            handler.inventoryGrid_000_Base.itemData = null;
        }, () => { }, () => { });

        
    }
}
