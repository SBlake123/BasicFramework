using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public partial class Inventory_000_Base : MonoBehaviour
{
    public Canvas hudCanvas;

    public List<InventoryGrid_000_Base> invenGridList = new List<InventoryGrid_000_Base>();
    public InventoryGrid_000_Base weaponGrid;
    public InventoryGrid_000_Base ArmorGrid;
    public List<InventoryGrid_000_Base> AccessoryGridList = new List<InventoryGrid_000_Base>();

    public InventoryGrid_000_Base selectedGrid { get; set; }

    private bool isFirstSetting = true;

    public async UniTask Init()
    {
        if (isFirstSetting)
        {
            InventoryInit();
        }
   
        await InventoryItemSetting();
        isFirstSetting = false;
    }

    public void InventoryInit()
    {
        weaponGrid.Inventory_000_Base = this;
        ArmorGrid.Inventory_000_Base = this;

        foreach (var grid in AccessoryGridList)
            grid.Inventory_000_Base = this;

        foreach (var grid in invenGridList)
            grid.Inventory_000_Base = this;
    }

    public async UniTask InventoryItemSetting()
    {
        var items = IngameSessionManager.Instance.playerIngameData.invenItems;

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
                else if (item.equipmentType == (int)EquipmentType.Armor && item.gridIdx == 0)
                    grid = ArmorGrid;
                else if (item.equipmentType == (int)EquipmentType.Accessory &&
                         item.gridIdx >= 0 && item.gridIdx < AccessoryGridList.Count)
                    grid = AccessoryGridList[item.gridIdx];
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
    public async UniTask MakeInvenItem(string ItemName, RectTransform itemRect)
    {
        Instantiate(await ResourceManager.Instance.LoadAsset<GameObject>(ItemName), itemRect);

        //var prefab = Resources.Load<GameObject>(ItemName);
        //Instantiate(prefab, itemRect, false);
    }
}
