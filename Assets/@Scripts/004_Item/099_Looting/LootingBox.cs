using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class LootingBox : MonoBehaviour, IPointerClickHandler
{
    public List<ItemData> itemDataList { get; set; } = new List<ItemData>();

    public bool isSetting = false;

    public async UniTask Init()
    {
        itemDataList.Clear();
        isSetting = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ItemLootingManager.Instance.OpenItemLootingBox(this);        
    }
}
