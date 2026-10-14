using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;


public class AccessoryData : ItemData
{
    public List<ItemOptionData> additionalOptionsList = new List<ItemOptionData>();

    public override ItemData DeepCopy()
    {
        return new AccessoryData
        {
            itemId = itemId,
            itemName = itemName,
            itemNameId = itemNameId,
            icon = icon,
            itemKey = itemKey,
            itemRarity = itemRarity,

            category = category,
            equipmentType = equipmentType,
            isEquip = isEquip,
            gridIdx = gridIdx,
            canStack = canStack,
            maxStackAmount = maxStackAmount,
            healAmount = healAmount,
            itemExplanation = itemExplanation,

            additionalOptionsList = additionalOptionsList.ConvertAll(option => option.DeepCopy())
        };
    }

    //public void OnValidate()
    //{
    //    if (category != ItemCategory.Equipment)
    //    {
    //        equipmentType = EquipmentType.None;
    //    }

    //    if (category == ItemCategory.Equipment)
    //    {
    //        canStack = false;
    //        maxStackAmount = 1;
    //    }

    //    if (!canStack)
    //    {
    //        maxStackAmount = 1;
    //    }
    //}
}

public class AccessoryBase : MonoBehaviour
{

}
