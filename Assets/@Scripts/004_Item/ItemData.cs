using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Item_", menuName = "BasicFramework/Item Data")]
[Serializable]
public class ItemData
{
    public int itemId = 0;
    public string itemName = "";
    public int itemNameId = 0;
    public int icon = 0;
    public string itemKey;

    public int category = 0;
    public int equipmentType = 0;
    public int canStack = 0;
    public int maxStackAmount = 0;
    public int healAmount = 0;

    public float attackDamage;
    public float attackSpeed;
    public float attackRange;
    public ItemData DeepCopy()
    {
        return new ItemData
        {
            itemId = itemId,
            itemName = itemName,
            itemNameId = itemNameId,
            icon = icon,
            itemKey = itemKey,

            category = category,
            equipmentType = equipmentType,
            canStack = canStack,
            maxStackAmount = maxStackAmount,
            healAmount = healAmount,

            attackDamage = attackDamage,
            attackSpeed = attackSpeed,
            attackRange = attackRange
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
