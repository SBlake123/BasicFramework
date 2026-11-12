using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public partial class Inventory_000_Base : MonoBehaviour
{
    public Canvas hudCanvas;

    public List<InventoryGrid_000_Base> invenGridList = new List<InventoryGrid_000_Base>();
    public InventoryGrid_000_Base weaponGrid;
    public InventoryGrid_000_Base shieldGrid;
    public List<InventoryGrid_000_Base> accessoryGridList = new List<InventoryGrid_000_Base>();
    public List<InventoryGrid_000_Base> lootingGridList = new List<InventoryGrid_000_Base>();

    public Inventory_ItemConfigWindow itemConfigWindow;
    public Transform lootingStash;

    public TextMeshProUGUI damageMainTxt;
    public TextMeshProUGUI defenseMainTxt;
    public TextMeshProUGUI hpMainTxt;
    public TextMeshProUGUI staminaMainTxt;
    public TextMeshProUGUI damageValTxt;
    public TextMeshProUGUI defenseValTxt;
    public TextMeshProUGUI hpValTxt;
    public TextMeshProUGUI staminaValTxt;


    private bool isFirstSetting = true;

    public async UniTask Init()
    {
        if (isFirstSetting)
        {
            InventoryInit();
        }

        lootingStash.gameObject.SetActive(false);
        await InventoryItemSetting();
        SetInventoryStatusVal();
        isFirstSetting = false;
    }
    public void InventoryInit()
    {
        weaponGrid.Inventory_000_Base = this;
        weaponGrid.inventoryCategory = InventoryCategory.PLAYER;
        weaponGrid.gridCategory = GridCategory.WEAPON;
        weaponGrid.gridIdx = 0;

        shieldGrid.Inventory_000_Base = this;
        shieldGrid.inventoryCategory = InventoryCategory.PLAYER;
        shieldGrid.gridCategory = GridCategory.SHIELD;

        shieldGrid.gridIdx = 0;


        foreach (var grid in accessoryGridList)
        {
            grid.inventoryCategory = InventoryCategory.PLAYER;
            grid.gridCategory = GridCategory.ACC;

            grid.gridIdx = accessoryGridList.IndexOf(grid);
            grid.Inventory_000_Base = this;
        }

        foreach (var grid in invenGridList)
        {
            grid.inventoryCategory = InventoryCategory.PLAYER;
            grid.gridIdx = invenGridList.IndexOf(grid);
            grid.gridCategory = GridCategory.STASH;
            grid.Inventory_000_Base = this;
        }

        foreach (var grid in lootingGridList)
        {
            grid.inventoryCategory = IsLobbyInventory ? InventoryCategory.STORAGE : InventoryCategory.LOOTING;
            grid.gridIdx = lootingGridList.IndexOf(grid);
            grid.gridCategory = GridCategory.STASH;
            grid.Inventory_000_Base = this;
        }
    }
    public async UniTask InventoryItemSetting()
    {

        Debug.Log("ItemSetting");
        InventoryItemInit();

        var items = IngameSessionManager.Instance.playerIngameData.invenItems;

        Debug.Log(JsonConvert.SerializeObject(items, Formatting.Indented));

        foreach (var item in items)
        {
            if (item == null) continue;

            InventoryGrid_000_Base grid = null;

            // 가방 아이템은 gridIdx번째 일반 슬롯에 놓는다.
            if (item.isEquip == 0)
            {
                if (item.gridIdx >= 0 && item.gridIdx < invenGridList.Count)
                    grid = invenGridList[item.gridIdx];
            }
            // 장착 아이템은 장비 종류에 맞는 슬롯에 놓는다.
            else if (item.isEquip == 1)
            {
                if (item.equipmentType == (int)EquipmentType.Weapon && item.gridIdx == 0)
                    grid = weaponGrid;
                else if (item.equipmentType == (int)EquipmentType.Shield && item.gridIdx == 0)
                    grid = shieldGrid;
                else if (item.equipmentType == (int)EquipmentType.Accessory &&
                         item.gridIdx >= 0 && item.gridIdx < accessoryGridList.Count)
                    grid = accessoryGridList[item.gridIdx];
            }

            if (grid == null)
            {
                Debug.LogWarning($"아이템 {item.itemId}의 슬롯을 찾을 수 없음: isEquip={item.isEquip}, gridIdx={item.gridIdx}");
                continue;
            }

            if (grid.itemData != null)
            {
                Debug.LogWarning($"아이템 {item.itemId}의 대상 슬롯에 이미 아이템이 있음");
                continue;
            }

            grid.itemData = item;
            string prefabKey = string.Format(GScriptAddress.invenItem, item.itemKey);
            await MakeInvenItem(prefabKey, grid.itemImgParent);
        }
    }
    public async UniTask LootingBoxItemSetting()
    {
        LootingItemInit();

        foreach (var item in ItemLootingManager.Instance.nowLootingBox.itemDataList)
        {
            if (item == null) continue;
            if (item.gridIdx < 0 || item.gridIdx >= lootingGridList.Count) continue;

            InventoryGrid_000_Base grid = lootingGridList[item.gridIdx];
            grid.itemData = item;

            string prefabKey = string.Format(GScriptAddress.invenItem, item.itemKey);
            await MakeInvenItem(prefabKey, grid.itemImgParent);
        }

        lootingStash.gameObject.SetActive(true);
    }
    public void SetInventoryStatusVal()
    {
        LanguageManager.Instance.GetLangScript(10020, LanguageManager.Instance.languageScriptDic, damageMainTxt);
        LanguageManager.Instance.GetLangScript(10021, LanguageManager.Instance.languageScriptDic, defenseMainTxt);
        LanguageManager.Instance.GetLangScript(10022, LanguageManager.Instance.languageScriptDic, hpMainTxt);
        LanguageManager.Instance.GetLangScript(10023, LanguageManager.Instance.languageScriptDic, staminaMainTxt);

        damageValTxt.text = $": {(int)IngameSessionManager.Instance.playerStats.attackDamage}";
        defenseValTxt.text = $": {(int)IngameSessionManager.Instance.playerStats.defense}";

        Debug.Log($"IngameSessionManager.Instance.playerStats.maxHp {IngameSessionManager.Instance.playerStats.maxHp} ");
        hpValTxt.text = $": {IngameSessionManager.Instance.playerStats.maxHp}";
        staminaValTxt.text = $": {IngameSessionManager.Instance.playerStats.maxStamina}";
    }
    public void InventoryItemInit()
    {
        if (weaponGrid.itemData != null)
        {
            weaponGrid.itemData = null;

            ObjectPool.Instance.PushToPool(weaponGrid.itemImgParent.GetChild(0).gameObject);
            //Destroy(weaponGrid.itemImgParent.GetChild(0).gameObject);
        }

        if (shieldGrid.itemData != null)
        {
            shieldGrid.itemData = null;
            ObjectPool.Instance.PushToPool(shieldGrid.itemImgParent.GetChild(0).gameObject);

            //Destroy(ArmorGrid.itemImgParent.GetChild(0).gameObject);
        }

        for (int i = 0; i < invenGridList.Count; i++)
        {
            if (invenGridList[i].itemData != null)
            {
                invenGridList[i].itemData = null;
                ObjectPool.Instance.PushToPool(invenGridList[i].itemImgParent.GetChild(0).gameObject);

                //Destroy(invenGridList[i].itemImgParent.GetChild(0).gameObject);
            }

        }

        for (int i = 0; i < accessoryGridList.Count; i++)
        {
            if (accessoryGridList[i].itemData != null)
            {
                accessoryGridList[i].itemData = null;
                ObjectPool.Instance.PushToPool(accessoryGridList[i].itemImgParent.GetChild(0).gameObject);

                //Destroy(AccessoryGridList[i].itemImgParent.GetChild(0).gameObject);
            }
        }
    }
    public void LootingItemInit()
    {
        for (int i = 0; i < lootingGridList.Count; i++)
        {
            if (lootingGridList[i].itemData != null)
            {
                lootingGridList[i].itemData = null;
                ObjectPool.Instance.PushToPool(lootingGridList[i].itemImgParent.GetChild(0).gameObject);

                //Destroy(invenGridList[i].itemImgParent.GetChild(0).gameObject);
            }

        }
    }
    public void SaveLootingBoxItems()
    {
        if (IsLobbyInventory) return;
        LootingBox box = ItemLootingManager.Instance.nowLootingBox;
        if (box == null) return;

        box.itemDataList.Clear();

        foreach (var grid in lootingGridList)
        {
            if (grid.itemData == null) continue;

            ItemData item = grid.itemData.DeepCopy();
            item.gridIdx = grid.gridIdx;
            item.isEquip = 0;
            box.itemDataList.Add(item);
        }

        ItemLootingManager.Instance.nowLootingBox = null;
    }
    public async UniTask MakeInvenItem(string ItemName, RectTransform itemRect)
    {
        GameObject obj = await ObjectPool.Instance.PopFromPool(ItemName, itemRect);
        var group = obj.GetComponent<CanvasGroup>();
        if (group != null) { group.blocksRaycasts = true; group.alpha = 1f; }
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
    }
    public UniTask InventoryClosed()
    {
        CancelDrags();
        IngameSessionManager.Instance.playerIngameData.invenItems = ExtractingInvenItems();
        SaveLootingBoxItems();
        return UniTask.CompletedTask;
    }

    public void SettingItemConfigWindow(ItemData itemData)
    {
        SettingItemSpecText(itemData);
        SettingItemAdditionalOptionText(itemData);
        LanguageManager.Instance.GetLangScript(itemData.itemId + 100000, LanguageManager.Instance.languageScriptDic, itemConfigWindow.itemExplanation);
    }
    public void SettingItemSpecText(ItemData itemData)
    {
        StringBuilder builder = new();

        switch (itemData.equipmentType)
        {
            case (int)EquipmentType.Weapon:
                {
                    itemConfigWindow.itemSpec.gameObject.SetActive(true);
                    WeaponData weaponData = (WeaponData)itemData;
                    AddStat(LanguageManager.Instance.GetLangScript(10020), weaponData.attackDamage);
                    AddStat(LanguageManager.Instance.GetLangScript(10024), weaponData.attackSpeed);
                    AddStat(LanguageManager.Instance.GetLangScript(10025), weaponData.attackRange);

                    itemConfigWindow.itemSpec.text = builder.ToString();
                }
                break;

            case (int)EquipmentType.Shield:
                {
                    itemConfigWindow.itemSpec.gameObject.SetActive(true);
                    ShieldData shieldData = (ShieldData)itemData;
                    AddStat(LanguageManager.Instance.GetLangScript(10021), shieldData.defense);
                    itemConfigWindow.itemSpec.text = builder.ToString();
                }
                break;

            case (int)EquipmentType.Accessory:
                {
                    itemConfigWindow.itemSpec.gameObject.SetActive(false);
                }
                break;

            case (int)EquipmentType.None:
                {

                }
                break;

            default:
                {

                }
                break;
        }

        void AddStat(string label, float value, string suffix = "")
        {
            if (value == 0f) return;

            if (builder.Length > 0) builder.Append('\n');
            builder.Append($"{label} : {value:0.##}{suffix}");
        }
    }
    public void SettingItemAdditionalOptionText(ItemData itemData)
    {
        List<ItemOptionData> options = null;

        if (itemData is WeaponData weapon) options = weapon.additionalOptionsList;
        else if (itemData is ShieldData shield) options = shield.additionalOptionsList;
        else if (itemData is AccessoryData accessory) options = accessory.additionalOptionsList;

        switch (itemData.itemRarity)
        {
            case (int)ItemRarity.COMMON:
                {
                    itemConfigWindow.itemName.text = LanguageManager.Instance.GetColorLangScript(itemData.itemId, GColorScript.COLOR_WHITE, LanguageManager.Instance.languageScriptDic);
                    break;
                }
            case (int)ItemRarity.UNCOMMON:
                {
                    itemConfigWindow.itemName.text = LanguageManager.Instance.GetColorLangScript(itemData.itemId, GColorScript.COLOR_UNCOMMON_BLUE, LanguageManager.Instance.languageScriptDic);
                    break;
                }
            case (int)ItemRarity.RARE:
                {
                    itemConfigWindow.itemName.text = LanguageManager.Instance.GetColorLangScript(itemData.itemId, GColorScript.COLOR_RARE_YELLOW, LanguageManager.Instance.languageScriptDic);
                    break;
                }
            case (int)ItemRarity.UNIQUE:
                {
                    itemConfigWindow.itemName.text = LanguageManager.Instance.GetColorLangScript(itemData.itemId, GColorScript.COLOR_UNIQUE_PINK, LanguageManager.Instance.languageScriptDic);
                    break;
                }
        }

        options?.Sort((a, b) => a.optionId.CompareTo(b.optionId));

        StringBuilder builder = new();

        if (options != null)
        {
            foreach (var option in options)
            {
                if (option == null) continue;

                string format = LanguageManager.Instance.GetLangScript(option.optionId);

                if (builder.Length > 0) builder.Append('\n');
                builder.AppendFormat(format, option.value);
            }
        }

        itemConfigWindow.itemAdditionalOption.text = builder.ToString();
        itemConfigWindow.itemAdditionalOption.gameObject.SetActive(builder.Length > 0);
    }
    public List<ItemData> ExtractingInvenItems()
    {
        if (isFirstSetting)
        {
            return IngameSessionManager.Instance.playerIngameData.invenItems
                .ConvertAll(item => item.DeepCopy());
        }

        List<ItemData> itemDataList = new List<ItemData>();

        AddItem(weaponGrid);
        AddItem(shieldGrid);

        foreach (var grid in accessoryGridList)
            AddItem(grid);

        foreach (var grid in invenGridList)
            AddItem(grid);

        return itemDataList;

        void AddItem(InventoryGrid_000_Base grid)
        {
            if (grid == null || grid.itemData == null) return;

            itemDataList.Add(grid.itemData.DeepCopy());
        }
    }

}
