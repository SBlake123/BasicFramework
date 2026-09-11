using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Item_", menuName = "BasicFramework/Item Data")]
[Serializable]
public class ItemData
{
    public int itemId;
    public string itemName;
    public Sprite icon;
    public ItemCategory category;
    public EquipmentType equipmentType = EquipmentType.None;
    public bool canStack = true;
    public int maxStackAmount = 99;
    public int healAmount;

    public void OnValidate()
    {
        if (category != ItemCategory.Equipment)
        {
            equipmentType = EquipmentType.None;
        }

        if (category == ItemCategory.Equipment)
        {
            canStack = false;
            maxStackAmount = 1;
        }

        if (!canStack)
        {
            maxStackAmount = 1;
        }
    }
}
