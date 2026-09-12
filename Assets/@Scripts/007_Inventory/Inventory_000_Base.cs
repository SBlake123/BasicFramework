using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Inventory_000_Base : MonoBehaviour
{
    public Canvas hudCanvas;

    public InventoryGrid_000_Base[] invenGridArr;
    public InventoryGrid_000_Base weaponGrid;
    public InventoryGrid_000_Base ArmorGrid;
    public InventoryGrid_000_Base[] AccessoryGrid;

    public InventoryGrid_000_Base selectedGrid { get; set; }

    public void Start()
    {
        invenGridArr[0].itemData = itemData;
        InventoryInit();
    }

    ItemData itemData = new ItemData()
    {
        itemId = 1,
        itemName = "Sword",
        category = ItemCategory.Equipment,
        equipmentType = EquipmentType.Weapon


        //ItemCategory category;
        //private EquipmentSlot equipmentSlot = EquipmentSlot.None;

        //private bool canStack = true;
        //private int maxStackAmount = 99;

        //private int healAmount;
    };

    public async UniTask InventoryInit()
    {
        //원래는 아이템데이터 리스트 받아서 쫙 깔아야댐

        weaponGrid.Inventory_000_Base = this;
        ArmorGrid.Inventory_000_Base = this;
        for (int i = 0; i < AccessoryGrid.Length; i++)
        {
            AccessoryGrid[i].Inventory_000_Base = this;
        }

        for (int i = 0; i < invenGridArr.Length; i++)
        {
            invenGridArr[i].Inventory_000_Base = this;

            if (invenGridArr[i].itemData != null)
            {
                await MakeInvenItem(invenGridArr[i].itemData.itemName, invenGridArr[i].itemImgParent);
            }
        }
    }

    public async UniTask MakeInvenItem(string ItemName, RectTransform itemRect)
    {
        Instantiate(await ResourceManager.Instance.LoadAsset<GameObject>(ItemName), itemRect);

        //var prefab = Resources.Load<GameObject>(ItemName);
        //Instantiate(prefab, itemRect, false);
    }
}
