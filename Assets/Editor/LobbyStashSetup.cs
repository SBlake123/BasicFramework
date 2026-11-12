using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class LobbyStashSetup
{
    private const string LobbyPath = "Assets/@Scenes/02_Ingame_Lobby.unity";

    [MenuItem("Tools/BasicFramework/Configure Lobby Stash")]
    public static void Configure()
    {
        var scene = EditorSceneManager.OpenScene(LobbyPath);
        var manager = UnityEngine.Object.FindFirstObjectByType<LobbySceneManager>();
        var inventory = UnityEngine.Object.FindFirstObjectByType<Inventory_000_Base>(FindObjectsInactive.Include);
        var serialized = new SerializedObject(manager);
        var field = scene.GetRootGameObjects().Single(o => o.name == "Field").transform;
        var stashTransform = field.Find("Stash");
        if (stashTransform == null)
        {
            var go = new GameObject("Stash", typeof(SpriteRenderer));
            go.transform.SetParent(field, false);
            go.transform.localPosition = new Vector3(0f, 4.5f, 0f);
            var sprite = go.GetComponent<SpriteRenderer>();
            var template = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/@Resources/GameObject/Box_Common.prefab");
            sprite.sprite = template.GetComponentInChildren<SpriteRenderer>().sprite;
            var npc = (SpriteRenderer)serialized.FindProperty("blacksmith").objectReferenceValue;
            sprite.sortingLayerID = npc.sortingLayerID;
            sprite.sortingOrder = npc.sortingOrder;
            go.transform.localScale = Vector3.one * 1.6f;
            stashTransform = go.transform;
        }
        serialized.FindProperty("stash").objectReferenceValue = stashTransform.GetComponent<SpriteRenderer>();
        serialized.ApplyModifiedPropertiesWithoutUndo();

        var storage = (RectTransform)inventory.lootingStash;
        storage.name = "PermanentStash";
        storage.anchorMin = new Vector2(0f, 0f);
        storage.anchorMax = new Vector2(0f, 1f);
        storage.pivot = new Vector2(0f, 0.5f);
        storage.anchoredPosition = new Vector2(10f, 0f);
        storage.sizeDelta = new Vector2(800f, 0f);
        var layout = inventory.lootingGridList[0].transform.parent.GetComponent<GridLayoutGroup>();
        if (layout == null) layout = inventory.lootingGridList[0].transform.parent.gameObject.AddComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 5;
        layout.cellSize = new Vector2(123f, 123f);
        layout.spacing = new Vector2(10f, 10f);
        layout.childAlignment = TextAnchor.MiddleCenter;
        var gridRect = (RectTransform)layout.transform;
        gridRect.anchorMin = gridRect.anchorMax = new Vector2(0.5f, 0.5f);
        gridRect.anchoredPosition = Vector2.zero;
        gridRect.sizeDelta = new Vector2(680f, 450f);

        while (inventory.lootingGridList.Count < inventory.invenGridList.Count)
        {
            var clone = UnityEngine.Object.Instantiate(inventory.lootingGridList[0].gameObject, layout.transform);
            clone.name = "StorageSlot_" + inventory.lootingGridList.Count;
            inventory.lootingGridList.Add(clone.GetComponent<InventoryGrid_000_Base>());
        }
        storage.gameObject.SetActive(false);
        EditorUtility.SetDirty(inventory);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Validate();
    }

    [MenuItem("Tools/BasicFramework/Validate Stash")]
    public static void Validate()
    {
        EditorSceneManager.OpenScene(LobbyPath);
        var inventory = UnityEngine.Object.FindFirstObjectByType<Inventory_000_Base>(FindObjectsInactive.Include);
        Check(inventory.lootingGridList.Count == inventory.invenGridList.Count, "Storage capacity");
        Check(EditorBuildSettings.scenes.Any(s => s.enabled && s.path == LobbyPath), "Lobby build reference");
        Check(EditorBuildSettings.scenes.Any(s => s.enabled && s.path == "Assets/@Scenes/03_Ingame_Raid_01.unity"), "Raid build reference");
        Check(GSceneName.LOBBY_SCENE == "02_Ingame_Lobby" && GSceneName.INGAME_SCENE == "03_Ingame_Raid_01", "Scene constants");

        // Isolated serialization round-trip; never touches the player's real save.
        var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
        settings.Converters.Add(new SavedItemDataConverter());
        var saved = new PlayerSaveData { playCnt = 7, profileData = new PlayerProfileData() };
        saved.stashItems.Add(new WeaponData { itemId = 1001, equipmentType = (int)EquipmentType.Weapon, gridIdx = 4, attackDamage = 37,
            itemRarity = 2, itemExplanation = "preserved",
            additionalOptionsList = new System.Collections.Generic.List<ItemOptionData> { new ItemOptionData { optionId = 90001, value = 12 } } });
        saved.stashItems.Add(new ShieldData { itemId = 2001, equipmentType = (int)EquipmentType.Shield, gridIdx = 8, defense = 22 });
        saved.stashItems.Add(new AccessoryData { itemId = 5000, equipmentType = (int)EquipmentType.Accessory, gridIdx = 12 });
        saved.profileData.invenItems.Add(new WeaponData { itemId = 1002, isEquip = 1 });
        string temp = Path.Combine(Path.GetTempPath(), "stash-validation-" + Guid.NewGuid() + ".json");
        try
        {
            File.WriteAllText(temp, GSecurity.EncryptString(JsonConvert.SerializeObject(saved, settings)));
            var loaded = JsonConvert.DeserializeObject<PlayerSaveData>(GSecurity.DecryptString(File.ReadAllText(temp)), settings);
            Check(loaded.playCnt == 7 && loaded.stashItems.Count == 3, "Save counts");
            var weapon = (WeaponData)loaded.stashItems[0];
            Check(weapon.attackDamage == 37 && weapon.additionalOptionsList[0].value == 12 && weapon.gridIdx == 4, "Weapon/options/slot round-trip");
            Check(loaded.stashItems[1] is ShieldData shield && shield.defense == 22 && loaded.stashItems[2] is AccessoryData, "Derived item types");
            loaded.profileData = new PlayerProfileData();
            Check(loaded.stashItems.Count == 3, "Profile replacement does not clear storage");
            Check(weapon.DeepCopy().itemExplanation == "preserved", "Deep copy metadata");
            var legacy = JsonConvert.DeserializeObject<PlayerSaveData>("{\"playCnt\":1,\"profileData\":{\"invenItems\":[]}}", settings);
            Check(legacy.stashItems != null && legacy.stashItems.Count == 0, "Old save migration");
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }

        inventory.gameObject.SetActive(true);
        for (Transform t = inventory.transform.parent; t != null; t = t.parent) t.gameObject.SetActive(true);
        inventory.ConfigureLobbyInventory();
        inventory.InventoryInit();
        inventory.lootingStash.gameObject.SetActive(true);
        typeof(Inventory_000_Base).GetProperty("IsStorageOpen").SetValue(inventory, true);
        var source = inventory.weaponGrid;
        var target = inventory.lootingGridList[0];
        for (Transform t = source.transform; t != null; t = t.parent) t.gameObject.SetActive(true);
        for (Transform t = target.transform; t != null; t = t.parent) t.gameObject.SetActive(true);
        source.itemData = new WeaponData { equipmentType = (int)EquipmentType.Weapon };
        target.itemData = null;
        Check(inventory.CanTransfer(source, target), "Equipped weapon directly to storage");
        target.itemData = new WeaponData { equipmentType = (int)EquipmentType.Weapon };
        Check(inventory.CanTransfer(source, target), "Equipped weapon swaps with stored weapon");
        target.itemData = new ShieldData { equipmentType = (int)EquipmentType.Shield };
        Check(!inventory.CanTransfer(source, target), "Incompatible swap rejected");
        source.itemData = null;
        target.itemData = new WeaponData { equipmentType = (int)EquipmentType.Weapon };
        Check(inventory.CanTransfer(target, source), "Storage directly to equipped slot");
        typeof(Inventory_000_Base).GetProperty("IsStorageOpen").SetValue(inventory, false);
        Check(!inventory.CanTransfer(target, source), "Closed storage rejects transfer");
        Check(!inventory.CanTransfer(source, source), "Same-slot drop rejected");
        Debug.Log("STASH_VALIDATION_PASSED: save round-trip, old save migration, capacity, equipment swap rules, scene references.");
        RenderPreview(inventory);
        // Discard test-only objects/data; the configured scene was saved before validation.
        EditorSceneManager.OpenScene(LobbyPath);
    }

    private static void RenderPreview(Inventory_000_Base inventory)
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        var manager = UnityEngine.Object.FindFirstObjectByType<LobbySceneManager>();
        typeof(LobbySceneManager).GetMethod("BuildPanels", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(manager, null);
        var canvas = inventory.DragCanvas;
        var cameraObject = new GameObject("Stash Preview Camera", typeof(Camera));
        var camera = cameraObject.GetComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -20);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.035f, 0.045f, 0.05f);
        camera.cullingMask = 1 << LayerMask.NameToLayer("UI");
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1f;
        foreach (var c in canvas.GetComponentsInChildren<Canvas>(true))
            if (c != canvas && c.overrideSorting) c.sortingOrder = 0;
        var texture = new RenderTexture(1920, 1080, 24);
        camera.targetTexture = texture;
        Canvas.ForceUpdateCanvases();
        camera.Render();
        RenderTexture.active = texture;
        var image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        image.Apply();
        File.WriteAllBytes("Library/StashPreview.png", image.EncodeToPNG());
        RenderTexture.active = null;
        camera.targetTexture = null;
        texture.Release();
        UnityEngine.Object.DestroyImmediate(texture);
        UnityEngine.Object.DestroyImmediate(image);
        UnityEngine.Object.DestroyImmediate(cameraObject);
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Stash validation failed: " + message);
    }
}
