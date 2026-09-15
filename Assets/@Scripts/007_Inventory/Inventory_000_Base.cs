using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
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

    public void InventoryItemInit()
    {
        if (weaponGrid.itemData != null)
        {
            weaponGrid.itemData = null;
            Destroy(weaponGrid.itemImgParent.GetChild(0).gameObject);
        }

        if (ArmorGrid.itemData != null)
        {
            ArmorGrid.itemData = null;
            Destroy(ArmorGrid.itemImgParent.GetChild(0).gameObject);
        }

        for (int i = 0; i < invenGridList.Count; i++)
        {
            if (invenGridList[i].itemData != null)
            {
                invenGridList[i].itemData = null;
                Destroy(invenGridList[i].itemImgParent.GetChild(0).gameObject);
            }

        }

        for (int i = 0; i < AccessoryGridList.Count; i++)
        {
            if (AccessoryGridList[i].itemData != null)
            {
                AccessoryGridList[i].itemData = null;
                Destroy(AccessoryGridList[i].itemImgParent.GetChild(0).gameObject);
            }
        }
    }
    public async UniTask MakeInvenItem(string ItemName, RectTransform itemRect)
    {
        Instantiate(await ResourceManager.Instance.LoadAsset<GameObject>(ItemName), itemRect);

        //var prefab = Resources.Load<GameObject>(ItemName);
        //Instantiate(prefab, itemRect, false);
    }

    public async UniTask InventoryClosed()
    {
        IngameSessionManager.Instance.playerIngameData.invenItems.Clear();

        AddGridItem(IngameSessionManager.Instance.playerIngameData.invenItems, weaponGrid);
        AddGridItem(IngameSessionManager.Instance.playerIngameData.invenItems, ArmorGrid);

        foreach (var item in AccessoryGridList)
        {
            AddGridItem(IngameSessionManager.Instance.playerIngameData.invenItems, item);
        }

        foreach (var item in invenGridList)
        {
            AddGridItem(IngameSessionManager.Instance.playerIngameData.invenItems, item);
        }

        void AddGridItem(List<ItemData> itemdataList, InventoryGrid_000_Base inventoryGrid_000_Base)
        {
            if (inventoryGrid_000_Base.itemData == null) return;

            ItemData item = inventoryGrid_000_Base.itemData.DeepCopy();

            itemdataList.Add(item);
        }
    }

    public async UniTask ItemMoveCheck(InventoryGrid_000_Base startGrid, InventoryGrid_000_Base targetGrid)
    {
        switch (targetGrid)
        {
            case var grid when grid == weaponGrid:
                {
                    if (startGrid.itemData.equipmentType == (int)EquipmentType.Weapon)
                    {
                        Debug.Log("StashToWeapon");
                        await ItemMoveOrChange(startGrid, targetGrid);
                        targetGrid.itemData.isEquip = (int)IsEquip.YES;
                        targetGrid.itemData.gridIdx = 0;

                        //장착 무기 갱신
                    }
                }
                break;

            case var grid when grid == ArmorGrid:
                {
                    if (startGrid.itemData.equipmentType == (int)EquipmentType.Shield)
                    {
                        Debug.Log("StashToArmor");

                        await ItemMoveOrChange(startGrid, targetGrid);
                        targetGrid.itemData.isEquip = (int)IsEquip.YES;
                        targetGrid.itemData.gridIdx = 0;
                    }
                }
                break;

            case var grid when AccessoryGridList.Contains(grid):
                {
                    if (startGrid.itemData.equipmentType == (int)EquipmentType.Accessory)
                    {
                        Debug.Log("StashToAccessory");

                        int targetGridIdx = AccessoryGridList.IndexOf(grid);

                        await ItemMoveOrChange(startGrid, targetGrid);
                        targetGrid.itemData.isEquip = (int)IsEquip.YES;
                        targetGrid.itemData.gridIdx = targetGridIdx;
                    }
                }
                break;

            default:
                {
                    await IsStashToStashMoveCheck(startGrid, targetGrid);

                    //int targetGridIdx = invenGridList.IndexOf(targetGrid);

                    //await ItemMoveOrChange(startGrid, targetGrid);



                }
                break;
        }

        //항상 장착 아이템 여부 확인하고 갱신해줘야함.

        async UniTask IsStashToStashMoveCheck(InventoryGrid_000_Base startGrid, InventoryGrid_000_Base targetGrid)
        {
            int targetGridIdx = invenGridList.IndexOf(targetGrid);

            if (targetGrid.itemData != null)
            {
                switch (startGrid)
                {
                    case var grid when grid == weaponGrid:
                        {
                            if (targetGrid.itemData.equipmentType == (int)EquipmentType.Weapon)
                            {
                                Debug.Log("WeaponToStash");                
                                await ItemMoveOrChange(startGrid, targetGrid);

                                //장착하고, 아이템데이타 서로 교환하고, 인스턴시에이트 다시해서 각각에 채워넣기
                                //startgrid에 있는 isEquip = 1, targetGrid에 있는 isEquip = 0;
                                //startgrid.gridIdx = targetGrid.gridIdx, targetGrid.gridIdx = startgrid.gridIdx
                            }
                        }
                        break;

                    case var grid when grid == ArmorGrid:
                        {
                            if (targetGrid.itemData.equipmentType == (int)EquipmentType.Shield)
                            {
                                Debug.Log("ArmorToStash");
                                await ItemMoveOrChange(startGrid, targetGrid);
                            }
                        }
                        break;

                    case var grid when AccessoryGridList.Contains(grid):
                        {
                            if (targetGrid.itemData.equipmentType == (int)EquipmentType.Accessory)
                            {
                                Debug.Log("AccessoryToStash");
                                await ItemMoveOrChange(startGrid, targetGrid);                           
                            }
                        }
                        break;

                    default:
                        {
                            Debug.Log("StashToStash");
                            //stash to stash
                            await ItemMoveOrChange(startGrid, targetGrid);
                        }
                        break;
                }
            }
            else
            {
                Debug.Log("NULL~");

                await ItemMoveOrChange(startGrid, targetGrid);
            }

            targetGrid.itemData.isEquip = (int)IsEquip.NO;
            targetGrid.itemData.gridIdx = targetGridIdx;
        }

        async UniTask ItemMoveOrChange(InventoryGrid_000_Base startGrid, InventoryGrid_000_Base targetGrid)
        {
            if (targetGrid.itemData == null)
            {
                var data = startGrid.itemData.DeepCopy();

                targetGrid.itemData = data;
                Instantiate(startGrid.itemImgParent.GetChild(0).gameObject, targetGrid.itemImgParent);

                startGrid.itemData = null;
                Destroy(startGrid.itemImgParent.GetChild(0).gameObject);
            }
            else
            {
                ItemData startItem = startGrid.itemData.DeepCopy();
                ItemData targetItem = targetGrid.itemData.DeepCopy();

                startItem.isEquip = targetGrid.itemData.isEquip;
                targetItem.isEquip = startGrid.itemData.isEquip;

                startItem.gridIdx = targetGrid.itemData.gridIdx;
                targetItem.gridIdx = startGrid.itemData.gridIdx;

                startGrid.itemData = targetItem;
                targetGrid.itemData = startItem;

                Destroy(startGrid.itemImgParent.GetChild(0).gameObject);
                Destroy(targetGrid.itemImgParent.GetChild(0).gameObject);

                await MakeInvenItem(string.Format(GScriptAddress.invenItem, startGrid.itemData.itemKey), startGrid.itemImgParent);
                await MakeInvenItem(string.Format(GScriptAddress.invenItem, targetGrid.itemData.itemKey), targetGrid.itemImgParent);
            }
        }

    }



}
