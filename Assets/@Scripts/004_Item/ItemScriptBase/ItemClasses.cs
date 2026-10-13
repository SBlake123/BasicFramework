using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum EquipmentType
//{
//    None,
//    Weapon,
//    Shield,
//    Accessory
//}

public enum StatType
{
    ADDITIONAL_DAMAGE = 90001,
    TOTAL_DAMAGE = 90002,
    ADDIDTIONAL_DEFENSE = 90004,
    ATTACK_RANGE = 90005,
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
public class ItemOptionData
{
    public int optionId { get; set; }
    public string description { get; set; }
    public ModifierType modifierType { get; set; }
    public DamageElement element { get; set; }
    public int weight { get; set; }
    public int value { get; set; }
    public int minValue { get; set; }
    public int maxValue { get; set; }

    public virtual ItemOptionData DeepCopy()
    {
        return new ItemOptionData
        {
            optionId = optionId,
            modifierType = modifierType,
            weight = weight,
            element = element,
            value = value,
            minValue = minValue,
            maxValue = maxValue
        };
    }

    void SetValue(int minValue, int maxValue)
    {
        if (minValue > maxValue)
            throw new System.ArgumentException("최솟값이 최댓값보다 큽니다.");

        // 1. 가중치로 구간을 선택한다.
        int roll = UnityEngine.Random.Range(0, 90);
        int section;

        if (roll < 60)
            section = 0;
        else if (roll < 76)
            section = 1;
        else if (roll < 85)
            section = 2;
        else if (roll < 89)
            section = 3;
        else
            section = 4;

        // 2. 선택한 구간의 최솟값과 최댓값을 계산한다.
        long count = (long)maxValue - minValue + 1;

        int sectionMin = (int)(minValue + count * section / 5);
        int sectionMax = (int)(minValue + count * (section + 1) / 5 - 1);

        // 가능한 정수가 5개 미만이면 일부 구간은 같은 값을 사용한다.
        sectionMax = Mathf.Max(sectionMin, sectionMax);

        // 3. 구간 안에서 랜덤으로 뽑는다.
        int offset = UnityEngine.Random.Range(
            0,
            (int)((long)sectionMax - sectionMin + 1)
        );

        value = sectionMin + offset;
    }
}