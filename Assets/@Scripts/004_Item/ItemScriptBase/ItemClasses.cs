using Newtonsoft.Json;
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
    ADDIDTIONAL_DEFENSE = 91001,
    TOTAL_DEFENSE = 91002,
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

    [JsonIgnore]
    public List<int> allowedType { get; set; } = new();

    [JsonProperty("allowedType")]
    private string AllowedTypeJson
    {
        get => JsonConvert.SerializeObject(allowedType);
        set => allowedType = string.IsNullOrWhiteSpace(value) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(value) ?? new List<int>();
    }

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
            maxValue = maxValue,
            description = description,
            allowedType = allowedType == null ? null : new List<int>(allowedType)
        };
    }

    public void SetValue(int minValue, int maxValue)
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