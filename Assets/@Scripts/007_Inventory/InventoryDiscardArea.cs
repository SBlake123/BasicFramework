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
        // Permanent storage transfers use the validated transaction, never the raid discard area.
        if (handler.inventoryGrid_000_Base.Inventory_000_Base.IsLobbyInventory ||
            handler.inventoryGrid_000_Base.Inventory_000_Base.IsMovingItem) return;
        //handler.inventoryGrid_000_Base.


        PopupManager.Instance.setPopUpCode(true, LanguageManager.Instance.GetLangScript(10011), LanguageManager.Instance.GetLangScript(10012),
                                                    LanguageManager.Instance.GetLangScript(10013));
        PopupManager.Instance.AddMethodToBtn(() => {
            ObjectPool.Instance.PushToPool(handler.inventoryGrid_000_Base.itemImgParent.GetChild(0).gameObject);
            handler.inventoryGrid_000_Base.itemData = null;
        }, () => { }, () => { });

        
    }
}
