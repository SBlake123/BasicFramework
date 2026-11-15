using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class CraftingValidation
{
    [MenuItem("Tools/Validation/Equipment Crafting")]
    public static void Run()
    {
        float[] risks = { 0, 0, 0, .12f, .24f, .33f, .8f, .8f };
        for (int i = 0; i < risks.Length; i++) Check(Mathf.Approximately(LobbyCraftingUI.DestructionProbability(i), risks[i]), "Attempt risk " + i);
        Check(LobbyCraftingUI.DestructionProbability(100) == .8f, "Unlimited attempts, capped risk");
        var stock = CraftingDummyData.Grant(new CraftingStock());
        Check(CraftingDummyData.Grant(stock).Count(80001) == 30, "Grant once");
        var item = new WeaponData { itemId = 1001, itemName = "검증용 무기", equipmentType = (int)EquipmentType.Weapon, attackDamage = 20 };
        var first = EquipmentCrafting.Execute(item, 80001, stock, 0, () => 0);
        Check(first.item.scrollAttemptCount == 1 && first.item.scrollSuccessCount == 1 && first.stock.Count(80001) == 29, "Successful paid attempt");
        Check(item.scrollAttemptCount == 0 && stock.Count(80001) == 30, "Immutable input");
        var second = EquipmentCrafting.Execute(first.item, 80001, first.stock, 0, () => 0);
        Check(second.item.scrollBonuses[0].value == 4 && ((WeaponData)second.item).attackDamage == 20, "Base percent does not compound");
        var fail = EquipmentCrafting.Execute(second.item, 80001, second.stock, 0, () => .9);
        Check(fail.outcome == CraftOutcome.Failed && fail.item.scrollAttemptCount == 3 && fail.item.scrollSuccessCount == 2 && fail.item.scrollBonuses[0].value == 4, "Failed attempt retains bonuses and consumes attempt");
        var destroy = EquipmentCrafting.Execute(fail.item, 80001, fail.stock, LobbyCraftingUI.DestructionProbability(3), () => .1);
        Check(destroy.outcome == CraftOutcome.Destroyed && destroy.item == null && destroy.stock.Count(80001) == 26, "Fourth attempt destruction debit");
        var safe = EquipmentCrafting.Execute(fail.item, 80001, fail.stock, .12f, () => .13);
        Check(safe.item != null, "Destruction boundary survival");
        var shield = new ShieldData { equipmentType = (int)EquipmentType.Shield, defense = 10 };
        Check(EquipmentCrafting.Execute(shield, 80103, stock, 0, () => 0).item.scrollBonuses[0].value == 6, "Shield percent increase");
        Check(EquipmentCrafting.Validate(shield, 80001, stock) != null, "Wrong scroll rejected");
        item.itemRarity = (int)ItemRarity.UNIQUE;
        foreach (int id in CraftingDummyData.MaterialIds()) Check(EquipmentCrafting.Validate(item, id, stock) != null, "Unique rejected " + id);
        Check(EquipmentCrafting.Validate(shield, 80101, new CraftingStock()) != null, "Missing materials");
        var crafted = fail;
        for (int i = 0; i < 4; i++) crafted = EquipmentCrafting.Execute(crafted.item, CraftingDummyData.AddStone, crafted.stock, .8f, () => .1);
        Check(EquipmentCrafting.Options(crafted.item).Count == 4 && crafted.stock.Count(CraftingDummyData.AddStone) == 18, "Four options, cost based on successes");
        Check(EquipmentCrafting.Validate(crafted.item, CraftingDummyData.AddStone, crafted.stock) != null, "Fifth option rejected");
        var reroll = EquipmentCrafting.Execute(crafted.item, CraftingDummyData.RerollStone, crafted.stock, .8f, () => .8);
        Check(reroll.item.scrollAttemptCount == 3 && reroll.item.scrollSuccessCount == 2 && reroll.item.scrollBonuses[0].value == 4, "Option crafting preserves all scroll progress");
        Check(EquipmentCrafting.Options(reroll.item).Select(x => x.optionId).Distinct().Count() == 4, "No duplicate option group");
        var replace = EquipmentCrafting.Execute(reroll.item, CraftingDummyData.ReplaceStone, reroll.stock, .8f, () => .5);
        Check(EquipmentCrafting.Options(replace.item).Count == 4, "Replace keeps count");
        var acc = new AccessoryData { equipmentType = (int)EquipmentType.Accessory };
        Check(EquipmentCrafting.Execute(acc, CraftingDummyData.AddStone, stock, .8f, () => 0).item is AccessoryData, "Accessory options supported");

        var settings = new JsonSerializerSettings { Converters = { new SavedItemDataConverter() } };
        var save = new PlayerSaveData { profileData = new PlayerProfileData(), craftingStock = replace.stock };
        save.stashItems.Add(replace.item);
        save.profileData.invenItems.Add(shield);
        var loaded = JsonConvert.DeserializeObject<PlayerSaveData>(JsonConvert.SerializeObject(save, settings), settings);
        Check(loaded.stashItems[0] is WeaponData && loaded.stashItems[0].scrollBonuses[0].value == 4 && loaded.craftingStock.Count(CraftingDummyData.AddStone) == 18, "Polymorphic save and materials roundtrip");
        var legacy = JsonConvert.DeserializeObject<PlayerSaveData>("{\"profileData\":{\"invenItems\":[]}}", settings);
        Check(legacy.craftingStock != null && legacy.craftingStock.Count(80001) == 0, "Legacy save compatibility");
        var copy = replace.item.DeepCopy();
        copy.scrollBonuses[0].value = 999;
        Check(replace.item.scrollBonuses[0].value == 4, "Independent bonus deep copy");

        Debug.Log("CRAFTING_VALIDATION_PASSED: probability boundaries, scroll outcomes, percent baseline, stock, options, save migration.");
    }

    public static void Preview()
    {
        Run();
        Transaction();
        var scene = EditorSceneManager.OpenScene("Assets/@Scenes/02_Ingame_Lobby.unity");
        var inventory = UnityEngine.Object.FindFirstObjectByType<Inventory_000_Base>(FindObjectsInactive.Include);
        inventory.ConfigureLobbyInventory();
        inventory.InventoryInit();
        inventory.IsCraftingOpen = true;
        typeof(Inventory_000_Base).GetField("isFirstSetting", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(inventory, false);
        inventory.weaponGrid.itemData = new WeaponData { itemName = "테스트 소드", itemId = 1001, equipmentType = 1, isEquip = 1, attackDamage = 20,
            scrollAttemptCount = 6, scrollSuccessCount = 3, scrollBonuses = new List<ScrollStatBonus> { new ScrollStatBonus { statType = StatType.ADDITIONAL_DAMAGE, value = 6 } },
            additionalOptionsList = new List<ItemOptionData> { new ItemOptionData { optionId = 92001, value = 10 } } };
        // In-memory fixture only; never read or write the user's save file.
        var manager = PlayerDataManager.Instance;
        var field = typeof(PlayerDataManager).GetField("saveData", BindingFlags.NonPublic | BindingFlags.Instance);
        var original = field.GetValue(manager);
        field.SetValue(manager, new PlayerSaveData { profileData = new PlayerProfileData(), craftingStock = CraftingDummyData.Grant(new CraftingStock()) });
        try
        {
            var lobby = UnityEngine.Object.FindFirstObjectByType<LobbySceneManager>();
            typeof(LobbySceneManager).GetMethod("BuildPanels", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(lobby, null);
            var page = (Ingame_002_Inventory)typeof(LobbySceneManager).GetField("inventoryPage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(lobby);
            for (Transform t = page.pageMain.transform; t != null; t = t.parent) t.gameObject.SetActive(true);
            page.pageMain.SetActive(true);
            inventory.lootingStash.gameObject.SetActive(false);
            ((GameObject)typeof(LobbySceneManager).GetField("screenGuard", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(lobby)).SetActive(false);
            ((Image)typeof(LobbySceneManager).GetField("transitionOverlay", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(lobby)).gameObject.SetActive(false);
            ((GameObject)typeof(LobbySceneManager).GetField("upgradePanel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(lobby)).SetActive(true);
            var canvas = inventory.DragCanvas;
            var camera = new GameObject("Preview Camera", typeof(Camera)).GetComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -20);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.04f, .05f, .06f);
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1;
            Debug.Log("CRAFTING_PREVIEW_CANVAS_SORT=" + canvas.sortingOrder);
            var rt = new RenderTexture(1920, 1080, 24);
            camera.targetTexture = rt;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = rt;
            var image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            image.Apply();
            File.WriteAllBytes("Library/CraftingPreview.png", image.EncodeToPNG());
            RenderTexture.active = null;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(image);
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);
        }
        finally
        {
            field.SetValue(manager, original);
            EditorSceneManager.OpenScene(scene.path);
        }
    }

    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception("Crafting validation failed: " + name);
    }

    public static void Transaction()
    {
        EditorSceneManager.OpenScene("Assets/@Scenes/02_Ingame_Lobby.unity");
        var inventory = UnityEngine.Object.FindFirstObjectByType<Inventory_000_Base>(FindObjectsInactive.Include);
        inventory.ConfigureLobbyInventory();
        inventory.InventoryInit();
        inventory.IsCraftingOpen = true;
        typeof(Inventory_000_Base).GetField("isFirstSetting", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(inventory, false);
        var manager = PlayerDataManager.Instance;
        var session = IngameSessionManager.Instance;
        var dataField = typeof(PlayerDataManager).GetField("saveData", BindingFlags.Instance | BindingFlags.NonPublic);
        var nameField = typeof(PlayerDataManager).GetField("validationSavePath", BindingFlags.Instance | BindingFlags.NonPublic);
        var pathProperty = typeof(PlayerDataManager).GetProperty("SavePath", BindingFlags.Instance | BindingFlags.NonPublic);
        var originalData = dataField.GetValue(manager);
        var originalName = nameField.GetValue(manager);
        string path = Path.Combine(Path.GetTempPath(), "craft-validation-" + Guid.NewGuid() + ".dat");
        try
        {
            // Verify the isolated path before ANY save call. The production path is never written.
            nameField.SetValue(manager, path);
            Check((string)pathProperty.GetValue(manager) == path, "Isolated transaction path");
            dataField.SetValue(manager, new PlayerSaveData { profileData = new PlayerProfileData(), craftingStock = CraftingDummyData.Grant(new CraftingStock()) });
            session.playerIngameData = new PlayerProfileData();
            var grid = inventory.invenGridList[0];
            grid.itemData = new WeaponData { itemId = 1001, equipmentType = 1, attackDamage = 20 };
            var scrollGrid = inventory.invenGridList[1];
            for (Transform t = scrollGrid.transform; t != null; t = t.parent) t.gameObject.SetActive(true);
            scrollGrid.itemData = ScrollDummyRawData.CreateItem(80001);
            Check(scrollGrid.itemData.canStack == 0 && scrollGrid.itemData.maxStackAmount == 1, "Non-stacking raw scroll");
            inventory.CraftItem(grid, grid.itemData, 80001, 0, scrollGrid, scrollGrid.itemData).GetAwaiter().GetResult();
            var settings = new JsonSerializerSettings { Converters = { new SavedItemDataConverter() } };
            var persisted = JsonConvert.DeserializeObject<PlayerSaveData>(GSecurity.DecryptString(File.ReadAllText(path)), settings);
            Check(persisted.profileData.invenItems[0].scrollAttemptCount == 1 && persisted.craftingStock.Count(80001) == 30 && scrollGrid.itemData == null && persisted.profileData.invenItems.Count == 1, "Item and material atomically persisted");
            int successes = grid.itemData.scrollSuccessCount;
            scrollGrid.itemData = ScrollDummyRawData.CreateItem(80001);
            string invalidPath = Path.Combine(Path.GetTempPath(), "missing-" + Guid.NewGuid(), "save.dat");
            nameField.SetValue(manager, invalidPath);
            Check((string)pathProperty.GetValue(manager) == invalidPath, "Isolated failing path");
            bool failed = false;
            try { inventory.CraftItem(grid, grid.itemData, 80001, 0, scrollGrid, scrollGrid.itemData).GetAwaiter().GetResult(); }
            catch (InvalidOperationException) { failed = true; }
            Check(failed && grid.itemData.scrollAttemptCount == 1 && grid.itemData.scrollSuccessCount == successes && manager.GetCraftingStock().Count(80001) == 30 && scrollGrid.itemData != null && !inventory.IsMovingItem, "Save failure refunds and unlocks");
            Debug.Log("CRAFTING_TRANSACTION_PASSED: atomic item/material save and failed-save rollback; temporary files only.");
        }
        finally
        {
            nameField.SetValue(manager, originalName);
            dataField.SetValue(manager, originalData);
            if (File.Exists(path)) File.Delete(path);
            if (File.Exists(path + ".tmp")) File.Delete(path + ".tmp");
            EditorSceneManager.OpenScene("Assets/@Scenes/02_Ingame_Lobby.unity");
        }
    }
}
