using System;
using System.Collections.Generic;

public enum CraftOutcome { Success, Failed, Destroyed }

public sealed class CraftResult
{
    public ItemData item;
    public CraftingStock stock;
    public CraftOutcome outcome;
}

// Computes on copies; inventory commits the result and material debit in one save.
public static class EquipmentCrafting
{
    public const int MaxOptions = 4;
    public static List<ItemOptionData> Options(ItemData item)
    {
        if (item is WeaponData weapon) return weapon.additionalOptionsList;
        if (item is ShieldData shield) return shield.additionalOptionsList;
        if (item is AccessoryData accessory) return accessory.additionalOptionsList;
        return null;
    }

    public static float ScrollIncrease(ItemData item, ScrollStatData scroll)
    {
        if (item is WeaponData weapon && scroll.equipmentType == EquipmentType.Weapon)
            return weapon.attackDamage * scroll.increasePercent / 100f;
        if (item is ShieldData shield && scroll.equipmentType == EquipmentType.Shield)
            return shield.defense * scroll.increasePercent / 100f;
        return 0;
    }

    public static string Validate(ItemData item, int materialId, CraftingStock stock)
    {
        if (item == null) return "대상 장비를 선택해 주세요.";
        if (item.itemRarity == (int)ItemRarity.UNIQUE) return "유니크는 강화·제작할 수 없습니다.";
        var options = Options(item);
        if (options == null) return "제작 가능한 장비가 아닙니다.";
        var scroll = CraftingDummyData.FindScroll(materialId);
        int cost;
        if (scroll != null)
        {
            if (item.equipmentType != (int)scroll.equipmentType || ScrollIncrease(item, scroll) <= 0)
                return "장비 종류에 맞는 주문서를 선택해 주세요.";
            cost = 1;
        }
        else
        {
            if (materialId != CraftingDummyData.RerollStone && materialId != CraftingDummyData.AddStone && materialId != CraftingDummyData.ReplaceStone)
                return "재료를 선택해 주세요.";
            if (options.Count > MaxOptions) return "기존 옵션이 4줄을 초과하여 제작할 수 없습니다.";
            if (materialId == CraftingDummyData.AddStone && options.Count >= MaxOptions) return "옵션은 최대 4줄입니다.";
            if (materialId == CraftingDummyData.ReplaceStone && options.Count == 0) return "교체할 옵션이 없습니다.";
            cost = CraftingDummyData.OptionCost(item);
        }
        return stock.Count(materialId) < cost ? $"재료가 부족합니다. 필요: {cost}개" : null;
    }

    public static CraftResult Execute(ItemData source, int materialId, CraftingStock stock,
        float destructionProbability, Func<double> random)
    {
        string error = Validate(source, materialId, stock);
        if (error != null) throw new InvalidOperationException(error);
        if (destructionProbability < 0 || destructionProbability > 1) throw new ArgumentOutOfRangeException(nameof(destructionProbability));
        var result = new CraftResult { item = source.DeepCopy(), stock = stock.DeepCopy(), outcome = CraftOutcome.Success };
        var scroll = CraftingDummyData.FindScroll(materialId);
        result.stock.counts[materialId] -= scroll != null ? 1 : CraftingDummyData.OptionCost(source);
        if (scroll != null)
        {
            result.item.scrollAttemptCount = checked(source.scrollAttemptCount + 1);
            if (random() < destructionProbability)
            {
                result.item = null;
                result.outcome = CraftOutcome.Destroyed;
                return result;
            }
            if (random() >= scroll.successRate / 100d)
            {
                result.outcome = CraftOutcome.Failed;
                return result;
            }
            result.item.scrollSuccessCount = checked(source.scrollSuccessCount + 1);
            result.item.scrollBonuses ??= new List<ScrollStatBonus>();
            var bonus = result.item.scrollBonuses.Find(x => x != null && x.statType == scroll.statType);
            if (bonus == null)
            {
                bonus = new ScrollStatBonus { statType = scroll.statType };
                result.item.scrollBonuses.Add(bonus);
            }
            bonus.value += ScrollIncrease(source, scroll);
            return result;
        }

        var options = Options(result.item);
        if (materialId == CraftingDummyData.RerollStone)
        {
            int count = Math.Max(1, options.Count); // Reroll preserves line count; empty equipment starts with one.
            options.Clear();
            for (int i = 0; i < count; i++) AddOption(options, result.item.equipmentType, random);
        }
        else if (materialId == CraftingDummyData.AddStone)
            AddOption(options, result.item.equipmentType, random);
        else
        {
            options.RemoveAt(Index(options.Count, random));
            AddOption(options, result.item.equipmentType, random);
        }
        return result;
    }

    private static int Index(int count, Func<double> random) => Math.Min(count - 1, (int)(random() * count));
    private static void AddOption(List<ItemOptionData> options, int equipmentType, Func<double> random)
    {
        var pool = CraftingDummyData.OptionPool();
        pool.RemoveAll(candidate => !candidate.allowedType.Contains(equipmentType) || options.Exists(x => x.optionId == candidate.optionId));
        if (pool.Count == 0) throw new InvalidOperationException("추가 가능한 옵션이 없습니다.");
        var option = pool[Index(pool.Count, random)];
        option.value = option.minValue + Index(option.maxValue - option.minValue + 1, random);
        options.Add(option);
    }
}
