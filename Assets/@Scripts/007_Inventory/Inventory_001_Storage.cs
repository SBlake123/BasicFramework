using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public partial class Inventory_000_Base
{
    public bool IsLobbyInventory { get; private set; }
    public bool IsMovingItem { get; private set; }
    public bool IsStorageOpen { get; private set; }
    public bool IsCraftingOpen { get; set; }
    public event Action<InventoryGrid_000_Base> CraftingSelectionRequested;
    public void SelectForCrafting(InventoryGrid_000_Base grid)
    {
        if (IsCraftingOpen && !IsMovingItem && OwnsSlot(grid)) CraftingSelectionRequested?.Invoke(grid);
    }
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

    public List<InventoryGrid_000_Base> CraftingSlots()
    {
        var slots = new List<InventoryGrid_000_Base> { weaponGrid, shieldGrid };
        slots.AddRange(accessoryGridList);
        slots.AddRange(invenGridList);
        if (storageLoaded) slots.AddRange(lootingGridList);
        slots.RemoveAll(grid => grid == null || grid.itemData == null ||
            !(grid.itemData is WeaponData || grid.itemData is ShieldData || grid.itemData is AccessoryData));
        return slots;
    }

    public async UniTask<CraftOutcome> CraftItem(InventoryGrid_000_Base grid, ItemData expected,
        int materialId, float destructionProbability, InventoryGrid_000_Base scrollGrid = null, ItemData expectedScroll = null)
    {
        if (!IsLobbyInventory || !IsCraftingOpen || IsMovingItem || !OwnsSlot(grid) ||
            !ReferenceEquals(grid.itemData, expected) || expected == null)
            throw new InvalidOperationException("장비가 변경되었습니다. 다시 선택해 주세요.");

        bool usesScroll = ScrollDummyRawData.Find(materialId) != null;
        if (usesScroll && (!CanSelectCraftingSource(scrollGrid) || scrollGrid == grid || expectedScroll == null ||
            !ReferenceEquals(scrollGrid.itemData, expectedScroll) || expectedScroll.itemId != materialId ||
            expectedScroll.equipmentType != (int)EquipmentType.None))
            throw new InvalidOperationException("인벤토리의 주문서 1개를 등록해 주세요.");

        IsMovingItem = true;
        CancelDrags();
        var original = grid.itemData.DeepCopy();
        var originalScroll = usesScroll ? scrollGrid.itemData.DeepCopy() : null;
        var session = IngameSessionManager.Instance;
        float hp = session.playerStats.currentHp;
        float stamina = session.playerStats.currentStamina;
        bool committed = false;
        try
        {
            var stock = PlayerDataManager.Instance.GetCraftingStock();
            if (usesScroll) stock = new CraftingStock { counts = new Dictionary<int, int> { { materialId, 1 } } };
            var result = EquipmentCrafting.Execute(original, materialId, stock, destructionProbability, () => UnityEngine.Random.value);
            grid.itemData = result.item;
            if (usesScroll) scrollGrid.itemData = null;
            SetSlotMetadata(grid);
            // Persist the roll before rendering. A view failure must not refund or reroll a paid attempt.
            session.playerIngameData.invenItems = ExtractingInvenItems();
            if (!PlayerDataManager.Instance.SaveLobbyProfile(session.playerIngameData,
                    storageLoaded ? ExtractStorageItems() : null, usesScroll ? null : result.stock))
                throw new InvalidOperationException("저장하지 못해 재료와 장비 변경을 취소했습니다.");
            committed = true;
            SyncCraftedEquipmentData(grid);
            if (usesScroll) await RenderSlot(scrollGrid);
            if (result.item == null) await RenderSlot(grid);
            if (IsEquipmentSlot(grid)) await session.RefreshPlayerStatus();
            return result.outcome;
        }
        catch
        {
            if (!committed)
            {
                grid.itemData = original;
                if (usesScroll) scrollGrid.itemData = originalScroll;
                session.playerIngameData.invenItems = ExtractingInvenItems();
                session.playerStats.currentHp = hp;
                session.playerStats.currentStamina = stamina;
            }
            else
            {
                // Keep equipped data correct even if an asynchronous icon/prefab load fails.
                SyncCraftedEquipmentData(grid);
                try { if (IsEquipmentSlot(grid)) await session.RefreshPlayerStatus(); }
                catch (Exception error) { Debug.LogException(error, this); }
                throw new InvalidOperationException("결과는 저장됐지만 화면 갱신에 실패했습니다. 로비에 다시 입장해 주세요.");
            }
            throw;
        }
        finally { IsMovingItem = false; }
    }

    public bool CanSelectCraftingSource(InventoryGrid_000_Base grid) => IsLobbyInventory && IsCraftingOpen &&
        !IsMovingItem && OwnsSlot(grid) && grid.itemData != null && grid.gameObject.activeInHierarchy;

    public async UniTask AddDummyScroll(int id)
    {
        if (!IsLobbyInventory || !IsCraftingOpen || IsMovingItem)
            throw new InvalidOperationException("로비 강화창에서 생성해 주세요.");
        var grid = invenGridList.Find(slot => slot.itemData == null);
        if (grid == null) throw new InvalidOperationException("인벤토리에 빈 칸이 없습니다.");
        var item = ScrollDummyRawData.CreateItem(id);
        IsMovingItem = true;
        bool saved = false;
        var session = IngameSessionManager.Instance;
        try
        {
            grid.itemData = item;
            SetSlotMetadata(grid);
            session.playerIngameData.invenItems = ExtractingInvenItems();
            if (!PlayerDataManager.Instance.SaveLobbyProfile(session.playerIngameData))
                throw new InvalidOperationException("주문서 생성을 저장하지 못했습니다.");
            saved = true;
            await RenderSlot(grid);
        }
        catch
        {
            if (!saved) { grid.itemData = null; session.playerIngameData.invenItems = ExtractingInvenItems(); }
            throw;
        }
        finally { IsMovingItem = false; }
    }

    private void SyncCraftedEquipmentData(InventoryGrid_000_Base grid)
    {
        var session = IngameSessionManager.Instance;
        if (grid == weaponGrid)
        {
            session.currentWeaponData = grid.itemData?.DeepCopy() as WeaponData;
            if (session.player.currentWeapon != null)
            {
                if (grid.itemData == null) { ObjectPool.Instance.PushToPool(session.player.currentWeapon.gameObject); session.player.currentWeapon = null; }
                else session.player.currentWeapon.weaponData = session.currentWeaponData;
            }
        }
        else if (grid == shieldGrid)
        {
            session.currentShieldData = grid.itemData?.DeepCopy() as ShieldData;
            if (session.player.currentShield != null)
            {
                if (grid.itemData == null) { ObjectPool.Instance.PushToPool(session.player.currentShield.gameObject); session.player.currentShield = null; }
                else session.player.currentShield.shieldData = session.currentShieldData;
            }
        }
        else
        {
            int index = accessoryGridList.IndexOf(grid);
            if (index >= 0) session.currentAccessoryDataArr[index] = grid.itemData?.DeepCopy();
        }
    }

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
            ScrollDummyItemView.Release(grid.itemImgParent.GetChild(0).gameObject);
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
