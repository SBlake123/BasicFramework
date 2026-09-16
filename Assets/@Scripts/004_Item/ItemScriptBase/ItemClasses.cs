using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    ADDIDTIONAL_DAMAGE,
    TOTAL_DAMAGE,
    ADDIDTIONAL_DEFENSE,
    ATTACK_RANGE
}
public enum DamageElement
{
    NORMAL,
    FIRE,
    ICE,
    POISON
}

public enum ModifierType
{
    FLAT,
    PERCENT
}

[Serializable]
public class ItemStatModifierData
{
    public StatType statType;
    public ModifierType modifierType;
    public DamageElement element;
    public float value;

    public virtual ItemStatModifierData DeepCopy()
    {
        return new ItemStatModifierData
        {
            statType = statType,
            modifierType = modifierType,
            element = element,
            value = value
        };
    }
}