using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
public class ShieldData : ItemData
{
    public float defense;

    public List<ItemStatModifierData> additionalOptionsList = new List<ItemStatModifierData>();

    public override ItemData DeepCopy()
    {
        return new ShieldData
        {
            itemId = itemId,
            itemName = itemName,
            itemNameId = itemNameId,
            icon = icon,
            itemKey = itemKey,

            category = category,
            equipmentType = equipmentType,
            isEquip = isEquip,
            gridIdx = gridIdx,
            canStack = canStack,
            maxStackAmount = maxStackAmount,
            healAmount = healAmount,

            defense = defense,

            additionalOptionsList =
                additionalOptionsList.ConvertAll(option => option.DeepCopy())
        };
    }

}

public abstract class ShieldBase : MonoBehaviour
{
    public ShieldData shieldData { get; set; }

    public GameObject guardEffect;

    public float Defense => shieldData.defense;

    public abstract UniTask GuardAsync(Player player, CancellationToken token);
}
