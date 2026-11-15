using System;

// Temporary raw catalogue. Replace with CSV data without changing item IDs.
public static class ScrollDummyRawData
{
    public static readonly ScrollStatData[] Definitions =
    {
        Define(80001, EquipmentType.Weapon, 60, StatType.ADDITIONAL_DAMAGE, 10),
        Define(80002, EquipmentType.Weapon, 30, StatType.ADDITIONAL_DAMAGE, 20),
        Define(80003, EquipmentType.Weapon, 10, StatType.ADDITIONAL_DAMAGE, 60),
        Define(80101, EquipmentType.Shield, 60, StatType.ADDITIONAL_DEFENSE, 10),
        Define(80102, EquipmentType.Shield, 30, StatType.ADDITIONAL_DEFENSE, 20),
        Define(80103, EquipmentType.Shield, 10, StatType.ADDITIONAL_DEFENSE, 60)
    };

    private static ScrollStatData Define(int id, EquipmentType type, int rate, StatType stat, float percent)
        => new ScrollStatData { itemId = id, equipmentType = type, successRate = rate, statType = stat, increasePercent = percent };

    public static ScrollStatData Find(int id) => Array.Find(Definitions, x => x.itemId == id);
    public static ItemData CreateItem(int id)
    {
        var raw = Find(id) ?? throw new ArgumentException("등록되지 않은 주문서 ID입니다.", nameof(id));
        string type = raw.equipmentType == EquipmentType.Weapon ? "무기" : "방패";
        return new ItemData
        {
            itemId = id,
            itemName = $"{type} {raw.successRate}% 주문서",
            itemKey = $"DummyScroll_{id}",
            category = (int)ItemCategory.Consumable,
            equipmentType = (int)EquipmentType.None,
            itemRarity = (int)ItemRarity.COMMON,
            canStack = 0,
            maxStackAmount = 1,
            isEquip = (int)IsEquip.NO,
            itemExplanation = $"생존 시 성공률 {raw.successRate}%\n기본 {type} 수치의 +{raw.increasePercent}%\n사용 시 주문서 1개 소모. 장비 파괴 확률 별도 적용."
        };
    }

    public static bool TryViewKey(string key, out int id)
    {
        id = 0;
        const string prefix = "DummyScroll_";
        const string suffix = "_Inven";
        return key != null && key.StartsWith(prefix) && key.EndsWith(suffix) &&
            int.TryParse(key.Substring(prefix.Length, key.Length - prefix.Length - suffix.Length), out id) && Find(id) != null;
    }
}
