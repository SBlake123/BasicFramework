using System;
using System.Collections.Generic;

[Serializable]
public class ScrollStatData
{
    public int itemId;
    public EquipmentType equipmentType;
    public int successRate;
    public StatType statType;
    public float increasePercent;
}

[Serializable]
public class CraftingStock
{
    public bool dummyGranted;
    public Dictionary<int, int> counts = new Dictionary<int, int>();

    public int Count(int id) => counts != null && counts.TryGetValue(id, out int count) ? count : 0;
    public CraftingStock DeepCopy() => new CraftingStock
    {
        dummyGranted = dummyGranted,
        counts = counts == null ? new Dictionary<int, int>() : new Dictionary<int, int>(counts)
    };
}

// Replace this catalogue with CSV-loaded definitions when the final item data is ready.
// No automatic inventory injection: the UI grants this test stock once, explicitly.
public static class CraftingDummyData
{
    public const int RerollStone = 80901;
    public const int AddStone = 80902;
    public const int ReplaceStone = 80903;
    public static ScrollStatData[] Scrolls => ScrollDummyRawData.Definitions;

    public static ScrollStatData FindScroll(int id) => Array.Find(Scrolls, x => x.itemId == id);
    public static string MaterialName(int id)
    {
        var scroll = FindScroll(id);
        if (scroll != null)
            return $"{(scroll.equipmentType == EquipmentType.Weapon ? "무기" : "방패")} {scroll.successRate}% 주문서";
        switch (id)
        {
            case RerollStone: return "재설정석";
            case AddStone: return "부여석";
            case ReplaceStone: return "교체석";
            default: return "알 수 없는 재료";
        }
    }

    public static IEnumerable<int> MaterialIds()
    {
        foreach (var scroll in Scrolls) yield return scroll.itemId;
        yield return RerollStone;
        yield return AddStone;
        yield return ReplaceStone;
    }

    public static CraftingStock Grant(CraftingStock current)
    {
        var stock = current.DeepCopy();
        if (stock.dummyGranted) return stock;
        foreach (int id in MaterialIds()) stock.counts[id] = stock.Count(id) + 30;
        stock.dummyGranted = true;
        return stock;
    }

    // Temporary balance: all option operations cost 1 + successful scroll count.
    public static int OptionCost(ItemData item) => checked(1 + Math.Max(0, item.scrollSuccessCount));

    public static List<ItemOptionData> OptionPool() => new List<ItemOptionData>
    {
        Option(StatType.ADDITIONAL_DAMAGE, 1, 5),
        Option(StatType.TOTAL_DAMAGE, 2, 10),
        Option(StatType.ADDITIONAL_DEFENSE, 1, 5),
        Option(StatType.TOTAL_DEFENSE, 2, 10),
        Option(StatType.ADDITIONAL_HEALTH, 5, 20),
        Option(StatType.TOTAL_HEALTH, 2, 10)
    };

    private static ItemOptionData Option(StatType stat, int min, int max) => new ItemOptionData
    {
        optionId = (int)stat, minValue = min, maxValue = max, weight = 1,
        modifierType = stat == StatType.TOTAL_DAMAGE || stat == StatType.TOTAL_DEFENSE || stat == StatType.TOTAL_HEALTH
            ? ModifierType.PERCENT : ModifierType.FLAT,
        allowedType = new List<int> { (int)EquipmentType.Weapon, (int)EquipmentType.Shield, (int)EquipmentType.Accessory }
    };

    public static string StatName(StatType stat)
    {
        switch (stat)
        {
            case StatType.ADDITIONAL_DAMAGE: return "공격력";
            case StatType.TOTAL_DAMAGE: return "공격력 증가(%)";
            case StatType.ADDITIONAL_DEFENSE: return "방어력";
            case StatType.TOTAL_DEFENSE: return "방어력 증가(%)";
            case StatType.ADDITIONAL_HEALTH: return "체력";
            case StatType.TOTAL_HEALTH: return "체력 증가(%)";
            default: return stat.ToString();
        }
    }
}
