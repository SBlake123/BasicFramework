using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[Serializable]
public class MonsterDropData
{
    public int monsterId { get; set; }
    public string prefabName { get; set; }
    public int itemId { get; set; }
    public bool isWeight { get; set; }
    public float weightRate { get; set; }
    public float dropRate { get; set; }
    public int minCount { get; set; }
    public int maxCount { get; set; }
}

public class ItemLootingManager : MonoSingleton<ItemLootingManager>
{
    public event Func<UniTask> LootingBoxOpenRequested;

    private int commonWeightBonus = 10;
    private int uncommonWeightBonus = 10;
    private int rareWeightBonus  = 115;
    private int uniqueWeightBonus = 0;

    private int uncommonOptionCnt = 1;
    private int rareOptionMinCnt  = 2;
    private int rareOptionMaxCnt  = 3;

    public LootingBox nowLootingBox { get; set; }

    public async UniTask MakingItemLootingBox(string objName, int monsterIdx, Transform spawnTrf)
    {
        GameObject obj = await ObjectPool.Instance.PopFromPool(objName);

        if (!obj.TryGetComponent<LootingBox>(out var lootingBox))
        {
            ObjectPool.Instance.PushToPool(obj);
            return;
        }

        lootingBox.Init();
        SettingLootingItem(monsterIdx, lootingBox);

        if (lootingBox.itemDataList.Count == 0)
        {
            ObjectPool.Instance.PushToPool(obj);
            return;
        }

        obj.transform.position = spawnTrf.position;
        obj.SetActive(true);
    }

    public async UniTask MakingItemLootingBox(string objName, List<ItemData> itemDataList, Transform spawnTrf)
    {
        GameObject obj = await ObjectPool.Instance.PopFromPool(objName);

        if (!obj.TryGetComponent<LootingBox>(out var lootingBox))
        {
            ObjectPool.Instance.PushToPool(obj);
            return;
        }

        lootingBox.Init();

        if (itemDataList != null)
        {
            foreach (var item in itemDataList)
            {
                lootingBox.itemDataList.Add(item);
            }
        }

        obj.transform.position = spawnTrf.position;
        obj.SetActive(true);
    }

    public async UniTask OpenItemLootingBox(LootingBox lootingBox)
    {
        nowLootingBox = lootingBox;

        var handlers = LootingBoxOpenRequested;

        if (handlers == null)
            return;

        foreach (Func<UniTask> handler in handlers.GetInvocationList())
        {
            await handler();
        }
    }

    public void SettingLootingItem(int monsterIdx, LootingBox lootingBox)
    {
        if (!DataManager.Instance.monsterDropDataDic.TryGetValue(monsterIdx, out var drops)) return;

        float totalWeight = 0f;

        foreach (MonsterDropData drop in drops)
        {
            if (drop.isWeight)
            {
                if (drop.weightRate > 0f) totalWeight += drop.weightRate;
            }
            else
            {
                TryDropItem(drop);
            }
        }

        if (totalWeight > 0f)
        {
            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;
            MonsterDropData selectedDrop = null;

            foreach (MonsterDropData drop in drops)
            {
                if (!drop.isWeight || drop.weightRate <= 0f) continue;

                selectedDrop = drop;
                cumulativeWeight += drop.weightRate;
                if (roll < cumulativeWeight) break;
            }

            if (selectedDrop != null) TryDropItem(selectedDrop);
        }

        lootingBox.isSetting = true;

        void TryDropItem(MonsterDropData drop)
        {
            if (drop.dropRate <= 0f) return;
            if (drop.dropRate < 1f && UnityEngine.Random.value >= drop.dropRate) return;

            if (drop.minCount < 0 || drop.maxCount < drop.minCount)
            {
                Debug.LogError($"드롭 수량 설정 오류: monsterId={monsterIdx}, itemId={drop.itemId}");
                return;
            }

            if (!DataManager.Instance.itemDataDic.TryGetValue(drop.itemId, out var template))
            {
                Debug.LogError($"아이템 데이터 없음: {drop.itemId}");
                return;
            }

            int count = UnityEngine.Random.Range(drop.minCount, drop.maxCount + 1);

            for (int i = 0; i < count; i++)
            {
                ItemData item = template.DeepCopy();
                item.isEquip = 0;
                item.gridIdx = lootingBox.itemDataList.Count;
                SetDropItemOption(item);
                lootingBox.itemDataList.Add(item);
            }
        }

    }

    public ItemData CreateStashItem(int itemId, int gridIdx, ItemRarity itemRarity)
    {
        if (itemRarity == ItemRarity.UNIQUE) return CreateUniqueItem(itemId, gridIdx);

        if (!DataManager.Instance.itemDataDic.TryGetValue(itemId, out var template))
        {
            return null;
        }

        ItemData item = template.DeepCopy();
        item.itemRarity = (int)itemRarity;
        item.isEquip = (int)IsEquip.NO;
        item.gridIdx = gridIdx; // 넣을 슬롯이 정해지면 지정

        AddRandomOptions(item, GetOptionCount(itemRarity));

        return item;

        ItemData CreateUniqueItem(int itemId, int gridIdx)
        {
            if (!DataManager.Instance.itemDataDic.TryGetValue(itemId, out var template))
            {
                return null;
            }

            if (!DataManager.Instance.uniqueItemOptionDataDic.TryGetValue(itemId, out var uniqueOptions))
            {
                return null;
            }

            ItemData item = template.DeepCopy();
            item.itemRarity = (int)ItemRarity.UNIQUE;
            item.isEquip = (int)IsEquip.NO;
            item.gridIdx = gridIdx;

            List<ItemOptionData> itemOptions;

            if (item is WeaponData weapon) itemOptions = weapon.additionalOptionsList;
            else if (item is ShieldData shield) itemOptions = shield.additionalOptionsList;
            else if (item is AccessoryData accessory) itemOptions = accessory.additionalOptionsList;
            else return null;

            var uniqueItemOptions = new List<UniqueItemOptionData>(DataManager.Instance.uniqueItemOptionDataDic[itemId]);

            foreach (var data in uniqueOptions)
            {
                if (!DataManager.Instance.itemOptionDataDic.TryGetValue(data.optionId, out var optionTemplate))
                {
                    return null;
                }

                ItemOptionData option = optionTemplate.DeepCopy();

                if (data.maxValue > 0) option.SetValue(data.minValue, data.maxValue);
                else option.value = data.value0;

                itemOptions.Add(option);
            }

            itemOptions.Sort((a, b) => a.optionId.CompareTo(b.optionId));

            return item;
        }
    }
    void AddRandomOptions(ItemData item, int count)
    {
        if (count <= 0) return;

        if (!DataManager.Instance.optionsByEquipmentTypeDic.TryGetValue(item.equipmentType, out var options)) return;

        var itemOptions = new List<ItemOptionData>(options);

        itemOptions.RemoveAll(option => option.weight <= 0);

        for (int i = 0; i < count; i++)
        {
            if (itemOptions.Count == 0) break;

            ItemOptionData selected = PickOptionByWeight(itemOptions);
            ItemOptionData option = selected.DeepCopy();
            option.SetValue(option.minValue, option.maxValue);

            if (item is WeaponData weapon) weapon.additionalOptionsList.Add(option);
            else if (item is ShieldData shield) shield.additionalOptionsList.Add(option);
            else if (item is AccessoryData accessory) accessory.additionalOptionsList.Add(option);

            itemOptions.RemoveAll(candidate => candidate.optionId == selected.optionId);
        }
    }

    ItemOptionData PickOptionByWeight(List<ItemOptionData> optionList)
    {
        int totalWeight = 0;
        foreach (var option in optionList) totalWeight += option.weight;

        int roll = UnityEngine.Random.Range(0, totalWeight);

        foreach (var option in optionList)
        {
            if (roll < option.weight) return option;
            roll -= option.weight;
        }

        return null;
    }

    int GetOptionCount(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.UNCOMMON:
                return uncommonOptionCnt;

            case ItemRarity.RARE:
                return UnityEngine.Random.Range(rareOptionMinCnt, rareOptionMaxCnt + 1); 

            default:
                return 0;
        }
    }


    void SetDropItemOption(ItemData item)
    {
        int common = commonWeightBonus;
        int uncommon = common + uncommonWeightBonus;
        int rare = uncommon + rareWeightBonus;
        int totalWeight = rare + uniqueWeightBonus;

        if (totalWeight <= 0) return;

        int roll = UnityEngine.Random.Range(0, totalWeight);

        if (roll < common) item.itemRarity = (int)ItemRarity.COMMON;
        else if (roll < uncommon) item.itemRarity = (int)ItemRarity.UNCOMMON;
        else if (roll < rare) item.itemRarity = (int)ItemRarity.RARE;
        else item.itemRarity = (int)ItemRarity.UNIQUE;

        AddRandomOptions(item, GetOptionCount((ItemRarity)item.itemRarity));
    }
}
