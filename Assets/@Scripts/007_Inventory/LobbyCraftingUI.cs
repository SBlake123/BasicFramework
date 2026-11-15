using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCraftingUI : MonoBehaviour
{
    private static readonly float[] DestructionRates = { 0, 0, 0, .12f, .24f, .33f, .8f };
    public static float DestructionProbability(int attempts) => DestructionRates[Mathf.Clamp(attempts, 0, 6)];

    private Inventory_000_Base inventory;
    private TMP_FontAsset font;
    private Func<UniTask> close;
    private InventoryGrid_000_Base equipmentSource, scrollSource;
    private ItemData equipment, scroll;
    private TextMeshProUGUI equipmentName, scrollName, preview, result, spawnName;
    private RectTransform equipmentIcon, scrollIcon;
    private Button execute, spawn;
    private ScrollDummySpawner spawner;
    private int spawnIndex;
    private bool running, lastBusy;

    public void Initialize(Inventory_000_Base owner, TMP_FontAsset uiFont, Func<UniTask> closeRequested)
    {
        inventory = owner;
        font = uiFont;
        close = closeRequested;
        spawner = gameObject.AddComponent<ScrollDummySpawner>();
        spawner.inventory = owner;
        Label("주문서 강화", 36, 0, 360, 620, 60);
        Label("인벤토리에서 장비와 주문서를 끌어다 놓으세요.", 24, 0, 305, 620, 45);
        equipmentIcon = Slot(false, -160, out equipmentName);
        scrollIcon = Slot(true, 160, out scrollName);
        Label("등록한 칸을 누르면 해제됩니다.", 20, 0, 25, 620, 35);
        preview = Label("", 25, 0, -75, 610, 155);
        preview.enableAutoSizing = true;
        preview.fontSizeMin = 19;
        preview.fontSizeMax = 25;
        execute = Button("강화 실행", -160, -205, () => Apply().Forget());
        Button("닫기", 160, -205, () => { if (!running) close().Forget(); });
        result = Label("", 23, 0, -265, 620, 60);
        Label("테스트 주문서 생성 / 중첩 불가", 21, 0, -320, 620, 35);
        Button("<", -265, -365, () => ChangeSpawn(-1), 65);
        spawnName = Label("", 23, 0, -365, 440, 45);
        Button(">", 265, -365, () => ChangeSpawn(1), 65);
        spawn = Button("인벤토리에 1개 생성", 0, -420, () => Spawn().Forget(), 330);
        Refresh();
    }

    private void OnEnable() { if (inventory != null) { ClearSelections(); Refresh(); } }
    private void OnDisable() { if (inventory != null) ClearSelections(); }
    private void Update()
    {
        if (inventory == null || running) return;
        bool changed = false;
        if (equipmentSource != null && (!inventory.CanSelectCraftingSource(equipmentSource) || !ReferenceEquals(equipmentSource.itemData, equipment)))
        { equipmentSource = null; equipment = null; DrawIcon(equipmentIcon, null); changed = true; }
        if (scrollSource != null && (!inventory.CanSelectCraftingSource(scrollSource) || !ReferenceEquals(scrollSource.itemData, scroll)))
        { scrollSource = null; scroll = null; DrawIcon(scrollIcon, null); changed = true; }
        if (changed || lastBusy != inventory.IsMovingItem) Refresh();
    }

    // These are references to original slots, not extra inventory containers.
    // Closing the panel never removes or duplicates the underlying items.
    public void AssignSlot(InventoryGrid_000_Base source, bool isScroll)
    {
        if (running || inventory.IsMovingItem || !inventory.IsCraftingOpen) return;
        if (source != null)
        {
            if (source.Inventory_000_Base != inventory || !inventory.CanSelectCraftingSource(source)) return;
            if (isScroll)
            {
                if (ScrollDummyRawData.Find(source.itemData.itemId) == null || source.itemData.equipmentType != (int)EquipmentType.None)
                { result.text = "주문서만 등록할 수 있습니다."; return; }
            }
            else if (EquipmentCrafting.Options(source.itemData) == null || source.itemData.itemRarity == (int)ItemRarity.UNIQUE)
            { result.text = "유니크를 제외한 장비만 등록할 수 있습니다."; return; }
        }
        if (isScroll) { scrollSource = source; scroll = source?.itemData; DrawIcon(scrollIcon, source); }
        else { equipmentSource = source; equipment = source?.itemData; DrawIcon(equipmentIcon, source); }
        result.text = "";
        Refresh();
    }

    private void ClearSelections()
    {
        equipmentSource = scrollSource = null;
        equipment = scroll = null;
        DrawIcon(equipmentIcon, null);
        DrawIcon(scrollIcon, null);
    }

    private void Refresh()
    {
        lastBusy = inventory.IsMovingItem;
        equipmentName.text = equipment?.itemName ?? "대상 장비";
        scrollName.text = scroll?.itemName ?? "주문서 1개";
        spawner.itemId = ScrollDummyRawData.Definitions[spawnIndex].itemId;
        spawnName.text = ScrollDummyRawData.CreateItem(spawner.itemId).itemName;
        spawn.interactable = !running && !inventory.IsMovingItem;
        execute.interactable = false;
        if (equipment == null || scroll == null)
        { preview.text = "장비 슬롯과 주문서 슬롯을 모두 채워 주세요.\n등록만으로 아이템이 소비되지는 않습니다."; return; }
        var raw = ScrollDummyRawData.Find(scroll.itemId);
        var stock = new CraftingStock { counts = new Dictionary<int, int> { { scroll.itemId, 1 } } };
        string error = EquipmentCrafting.Validate(equipment, scroll.itemId, stock);
        preview.text = $"주문서 시도 {equipment.scrollAttemptCount}회 / 성공 {equipment.scrollSuccessCount}회\n" +
            $"이번 파괴 확률 <color=#FF9585>{DestructionProbability(equipment.scrollAttemptCount):P0}</color> / 생존 시 성공 {raw.successRate}%\n" +
            $"성공 시 기본 수치의 +{raw.increasePercent}% (+{EquipmentCrafting.ScrollIncrease(equipment, raw):0.##})\n" +
            "성공/실패/파괴 모두 주문서 1개 소비";
        if (error != null) preview.text += "\n" + error;
        execute.interactable = error == null && !running && !inventory.IsMovingItem && inventory.IsCraftingOpen;
    }

    private async UniTask Apply()
    {
        if (running || equipment == null || scroll == null || inventory.IsMovingItem) return;
        running = true;
        Refresh();
        try
        {
            var target = equipmentSource;
            var outcome = await inventory.CraftItem(target, equipment, scroll.itemId,
                DestructionProbability(equipment.scrollAttemptCount), scrollSource, scroll);
            scrollSource = null; scroll = null; DrawIcon(scrollIcon, null);
            equipment = target.itemData;
            if (equipment == null) equipmentSource = null;
            DrawIcon(equipmentIcon, equipmentSource);
            result.text = outcome == CraftOutcome.Destroyed ? "장비가 파괴되었습니다. 주문서 1개 소비." :
                outcome == CraftOutcome.Failed ? "강화 실패. 시도 횟수 +1 / 주문서 1개 소비." : "강화 성공! 주문서 1개 소비.";
        }
        catch (Exception error)
        {
            Debug.LogException(error, this);
            ClearSelections();
            result.text = error.Message;
        }
        finally { running = false; if (this != null) Refresh(); }
    }

    private void ChangeSpawn(int direction)
    {
        if (running) return;
        spawnIndex = (spawnIndex + direction + ScrollDummyRawData.Definitions.Length) % ScrollDummyRawData.Definitions.Length;
        Refresh();
    }

    private async UniTask Spawn()
    {
        if (running || inventory.IsMovingItem) return;
        running = true;
        Refresh();
        try { await spawner.Spawn(spawner.itemId); result.text = "선택한 주문서 1개를 인벤토리에 생성했습니다."; }
        catch (Exception error) { result.text = error.Message; Debug.LogException(error, this); }
        finally { running = false; if (this != null) Refresh(); }
    }

    private RectTransform Slot(bool isScroll, float x, out TextMeshProUGUI nameLabel)
    {
        var image = new GameObject(isScroll ? "Scroll Drop Slot" : "Equipment Drop Slot", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        image.transform.SetParent(transform, false);
        Place(image.rectTransform, x, 190, 185, 185);
        image.color = new Color(.23f, .30f, .32f);
        var drop = image.gameObject.AddComponent<CraftingDropSlot>();
        drop.owner = this; drop.isScroll = isScroll;
        var icon = new GameObject("Icon", typeof(RectTransform)).GetComponent<RectTransform>();
        icon.SetParent(image.transform, false);
        Place(icon, 0, 0, 120, 120);
        nameLabel = Label("", 24, x, 70, 290, 55);
        return icon;
    }

    private static void DrawIcon(RectTransform holder, InventoryGrid_000_Base source)
    {
        if (holder == null) return;
        while (holder.childCount > 0)
        {
            var child = holder.GetChild(0);
            child.SetParent(null); child.gameObject.SetActive(false); Destroy(child.gameObject);
        }
        if (source == null || source.itemImgParent.childCount == 0) return;
        var clone = Instantiate(source.itemImgParent.GetChild(0).gameObject, holder, false);
        var rect = clone.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;
        clone.SetActive(true);
        foreach (var graphic in clone.GetComponentsInChildren<Graphic>()) graphic.raycastTarget = false;
        if (!clone.TryGetComponent<CanvasGroup>(out var group)) group = clone.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
    }

    private TextMeshProUGUI Label(string text, float size, float x, float y, float width, float height)
    {
        var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        label.transform.SetParent(transform, false);
        Place(label.rectTransform, x, y, width, height);
        label.font = font; label.fontSize = size; label.text = text;
        label.color = new Color(.96f, .93f, .82f);
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }

    private Button Button(string text, float x, float y, UnityEngine.Events.UnityAction action, float width = 240)
    {
        var image = new GameObject(text, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        image.transform.SetParent(transform, false);
        Place(image.rectTransform, x, y, width, 52);
        image.color = new Color(.19f, .33f, .31f);
        var button = image.gameObject.AddComponent<Button>();
        button.onClick.AddListener(action);
        var label = Label(text, 24, 0, 0, width - 10, 52);
        label.transform.SetParent(image.transform, false);
        Place(label.rectTransform, 0, 0, width - 10, 52);
        return button;
    }

    private static void Place(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
    }
}
