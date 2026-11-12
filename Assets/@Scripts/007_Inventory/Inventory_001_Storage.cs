using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public partial class Inventory_000_Base
{
    public bool IsLobbyInventory { get; private set; }
    public bool IsMovingItem { get; private set; }
    public bool IsStorageOpen { get; private set; }
    public Canvas DragCanvas => hudCanvas != null ? hudCanvas : GetComponentInParent<Canvas>().rootCanvas;
    private bool storageLoaded;

    public void ConfigureLobbyInventory() => IsLobbyInventory = true;

    public async UniTask OpenStorage()
    {
        if (!IsLobbyInventory || IsMovingItem) return;
        IsMovingItem = true;
        try
        {
            LootingItemInit();
            foreach (var grid in lootingGridList)
                grid.inventoryCategory = InventoryCategory.STORAGE;

            foreach (var item in PlayerDataManager.Instance.GetStashItems())
            {
                if (item.gridIdx < 0 || item.gridIdx >= lootingGridList.Count)
                    throw new InvalidOperationException($"Storage slot {item.gridIdx} is unavailable.");
                var grid = lootingGridList[item.gridIdx];
                if (grid.itemData != null)
                    throw new InvalidOperationException($"Duplicate storage slot {item.gridIdx}.");
                grid.itemData = item;
                await RenderSlot(grid);
            }
            storageLoaded = true;
            IsStorageOpen = true;
            lootingStash.gameObject.SetActive(true);
        }
        finally { IsMovingItem = false; }
    }

    public void CloseStorage()
    {
        CancelDrags();
        IsStorageOpen = false;
        lootingStash.gameObject.SetActive(false);
    }

    public void CancelDrags()
    {
        foreach (var handler in GetComponentsInChildren<InvenSpacePointerHandler>(true))
            handler.CancelDrag();
        if (itemConfigWindow != null) itemConfigWindow.gameObject.SetActive(false);
    }

    private bool OwnsSlot(InventoryGrid_000_Base grid)
    {
        if (grid == null || grid.Inventory_000_Base != this) return false;
        return grid == weaponGrid || grid == shieldGrid || accessoryGridList.Contains(grid) ||
               invenGridList.Contains(grid) || lootingGridList.Contains(grid);
    }

    private bool IsEquipmentSlot(InventoryGrid_000_Base grid) =>
        grid == weaponGrid || grid == shieldGrid || accessoryGridList.Contains(grid);

    public bool CanTransfer(InventoryGrid_000_Base source, InventoryGrid_000_Base target)
    {
        if (IsMovingItem || !OwnsSlot(source) || !OwnsSlot(target) || source == target || source.itemData == null)
            return false;
        if (!source.gameObject.activeInHierarchy || !target.gameObject.activeInHierarchy) return false;
        if ((source.inventoryCategory == InventoryCategory.STORAGE || target.inventoryCategory == InventoryCategory.STORAGE)
            && !IsStorageOpen) return false;
        return CanPlace(target, source.itemData) && CanPlace(source, target.itemData);
    }

    private static bool CanPlace(InventoryGrid_000_Base grid, ItemData item) =>
        item == null || grid.gridCategory == GridCategory.STASH || item.equipmentType == (int)grid.gridCategory;

    private void SetSlotMetadata(InventoryGrid_000_Base grid)
    {
        if (grid.itemData == null) return;
        grid.itemData.isEquip = IsEquipmentSlot(grid) ? (int)IsEquip.YES : (int)IsEquip.NO;
        grid.itemData.gridIdx = grid.gridIdx;
    }

    private List<ItemData> ExtractStorageItems()
    {
        var items = new List<ItemData>();
        foreach (var grid in lootingGridList)
        {
            if (grid.itemData == null) continue;
            var item = grid.itemData.DeepCopy();
            item.gridIdx = grid.gridIdx;
            item.isEquip = (int)IsEquip.NO;
            items.Add(item);
        }
        return items;
    }

    private async UniTask RenderSlot(InventoryGrid_000_Base grid)
    {
        while (grid.itemImgParent.childCount > 0)
            ObjectPool.Instance.PushToPool(grid.itemImgParent.GetChild(0).gameObject);
        if (grid.itemData != null)
            await MakeInvenItem(string.Format(GScriptAddress.invenItem, grid.itemData.itemKey), grid.itemImgParent);
    }

    private async UniTask RefreshEquipmentSlot(InventoryGrid_000_Base grid)
    {
        if (!IsEquipmentSlot(grid)) return;
        var session = IngameSessionManager.Instance;
        int accessoryIndex = accessoryGridList.IndexOf(grid);
        if (accessoryIndex >= 0)
            session.currentAccessoryDataArr[accessoryIndex] = grid.itemData?.DeepCopy();
        else
            await session.RefreshCurrentItem(grid, grid.itemData == null ? IsEquip.NO : IsEquip.YES);
    }

    public async UniTask ItemMoveCheck(InventoryGrid_000_Base startGrid, InventoryGrid_000_Base targetGrid)
    {
        if (!CanTransfer(startGrid, targetGrid)) return;
        IsMovingItem = true;
        var originalSource = startGrid.itemData.DeepCopy();
        var originalTarget = targetGrid.itemData?.DeepCopy();
        var session = IngameSessionManager.Instance;
        float originalHp = session.playerStats.currentHp;
        float originalStamina = session.playerStats.currentStamina;
        bool equipmentChanged = IsEquipmentSlot(startGrid) || IsEquipmentSlot(targetGrid);

        try
        {
            startGrid.itemData = originalTarget?.DeepCopy();
            targetGrid.itemData = originalSource.DeepCopy();
            SetSlotMetadata(startGrid);
            SetSlotMetadata(targetGrid);

            await RenderSlot(startGrid);
            await RenderSlot(targetGrid);
            await RefreshEquipmentSlot(startGrid);
            await RefreshEquipmentSlot(targetGrid);
            if (equipmentChanged) await session.RefreshPlayerStatus();

            session.playerIngameData.invenItems = ExtractingInvenItems();
            // One atomic save contains BOTH sides. Never save a half-completed transfer.
            if (IsLobbyInventory && !PlayerDataManager.Instance.SaveLobbyProfile(
                    session.playerIngameData, storageLoaded ? ExtractStorageItems() : null))
                throw new InvalidOperationException("아이템 변경을 저장하지 못했습니다.");
        }
        catch (Exception error)
        {
            startGrid.itemData = originalSource;
            targetGrid.itemData = originalTarget;
            session.playerIngameData.invenItems = ExtractingInvenItems();
            try
            {
                await RenderSlot(startGrid);
                await RenderSlot(targetGrid);
                await RefreshEquipmentSlot(startGrid);
                await RefreshEquipmentSlot(targetGrid);
                if (equipmentChanged) await session.RefreshPlayerStatus();
                session.playerStats.currentHp = originalHp;
                session.playerStats.currentStamina = originalStamina;
                IngameUIManager.Instance.SetHpSlider();
                IngameUIManager.Instance.SetStaminaSlider();
            }
            catch (Exception rollbackError) { Debug.LogException(rollbackError, this); }
            Debug.LogException(error, this);
            PopupManager.Instance.setPopUpCode(true, "아이템을 옮기지 못했습니다. 다시 시도해 주세요.", "확인");
        }
        finally { IsMovingItem = false; }
    }
}
