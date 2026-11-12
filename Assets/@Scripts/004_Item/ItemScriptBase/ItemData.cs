using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemRarity
{
    COMMON,
    UNCOMMON,
    RARE,
    UNIQUE
}

[CreateAssetMenu(fileName = "Item_", menuName = "BasicFramework/Item Data")]
[Serializable]
public class ItemData
{
    public int itemId = 0;
    public string itemName = "";
    public int itemNameId = 0;
    public int icon = 0;
    public int itemRarity = 0;
    public string itemKey;

    public int category = 0;
    public int equipmentType = 0;
    public int isEquip = 0;
    public int gridIdx = 0;
    public int canStack = 0;
    public int maxStackAmount = 0;
    public int healAmount = 0;

    public string itemExplanation = "";

    // Per-instance progress. Raw item definitions can omit these fields.
    public int scrollAttemptCount = 0;
    public int scrollSuccessCount = 0;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Ignore)]
    public List<ScrollStatBonus> scrollBonuses = new List<ScrollStatBonus>();

    protected List<ScrollStatBonus> CopyScrollBonuses()
    {
        return scrollBonuses == null
            ? new List<ScrollStatBonus>()
            : scrollBonuses.ConvertAll(bonus => bonus == null ? null : new ScrollStatBonus
            {
                statType = bonus.statType,
                value = bonus.value
            });
    }

    public virtual ItemData DeepCopy()
    {
        return new ItemData
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
            scrollAttemptCount = scrollAttemptCount,
            scrollSuccessCount = scrollSuccessCount,
            scrollBonuses = CopyScrollBonuses()
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


[Serializable]
public class ScrollStatBonus
{
    public StatType statType;
    public float value;
}