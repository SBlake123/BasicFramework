const fs=require('fs'); const p='Assets/@Scripts/007_Inventory/InventoryGrid/InvenSpacePointerHandler.cs';
let b=fs.readFileSync(p),s;try{s=new TextDecoder('utf-8',{fatal:true}).decode(b)}catch{s=new TextDecoder('euc-kr').decode(b)}
s=s.replace('using System.Collections;','using Cysharp.Threading.Tasks;\nusing System;\nusing System.Collections;');
s=s.replace('IPointerClickHandler\r\n','IPointerClickHandler, IInitializePotentialDragHandler\n');
let a=s.indexOf('    public async void OnBeginDrag('),z=s.indexOf('    public void OnPointerClick(',a);
s=s.slice(0,a)+`    private int dragVersion;
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

`+s.slice(z);
s=s.replaceAll('IngameUIManager.Instance.hudCanvas','inventoryGrid_000_Base.Inventory_000_Base.DragCanvas');
// Drop validation belongs to the inventory owner, not the scene or global HUD.
s=s.replace('inventoryGrid_000_Base.Inventory_000_Base.ItemMoveCheck(inventoryGrid_000_Base, targetGrid);','inventoryGrid_000_Base.Inventory_000_Base.ItemMoveCheck(inventoryGrid_000_Base, targetGrid).Forget();');
fs.writeFileSync(p,s.replace(/\r\n/g,'\n'));
const discard='Assets/@Scripts/007_Inventory/InventoryDiscardArea.cs';
b=fs.readFileSync(discard);try{s=new TextDecoder('utf-8',{fatal:true}).decode(b)}catch{s=new TextDecoder('euc-kr').decode(b)}
s=s.replace('if (handler.inventoryGrid_000_Base.itemData == null) return;',`if (handler.inventoryGrid_000_Base.itemData == null) return;
        // Permanent storage transfers use the validated transaction, never the raid discard area.
        if (handler.inventoryGrid_000_Base.Inventory_000_Base.IsLobbyInventory ||
            handler.inventoryGrid_000_Base.Inventory_000_Base.IsMovingItem) return;`);
fs.writeFileSync(discard,s.replace(/\r\n/g,'\n'));
