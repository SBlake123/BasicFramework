using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
[Serializable]
public class WeaponData : ItemData
{
    public float attackDamage = 1f;
    public float attackSpeed = 0.3f;
    public float attackRange = 1f;

    public List<ItemStatModifierData> additionalOptionsList = new List<ItemStatModifierData>();

    public override ItemData DeepCopy()
    {
        return new WeaponData
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

            attackDamage = attackDamage,
            attackSpeed = attackSpeed,
            attackRange = attackRange,

            additionalOptionsList =
                additionalOptionsList.ConvertAll(option => option.DeepCopy())
        };
    }
}

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponData weaponData { get; set; } = new WeaponData();

    public GameObject attackEffect;

    public GameObject weaponHitBox;
    public float AttackDamage => weaponData.attackDamage;
    public float AttackSpeed => weaponData.attackSpeed;
    public float AttackRange => weaponData.attackRange;

    //async
    public abstract UniTask AttackAsync(Player player, CancellationToken token);

    public abstract void UpdateEffectDirection(GameObject gameObject, Player player);
}
