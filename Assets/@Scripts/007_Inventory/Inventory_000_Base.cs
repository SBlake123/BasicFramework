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
        invenGridArr[0].itemData = DataManager.Instance.GetItemData(1001);
        invenGridArr[1].itemData = DataManager.Instance.GetItemData(1002);
        InventoryInit();
    }

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
                await MakeInvenItem(string.Format(GScriptAddress.invenItem, invenGridArr[i].itemData.itemKey), invenGridArr[i].itemImgParent);
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
