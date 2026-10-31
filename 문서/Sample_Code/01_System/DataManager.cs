using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponStatData
{
    public int itemId;
    public string itemName;
    public float attackDamage;
    public float attackSpeed;
    public float attackRange;
}

[Serializable]
public class ShieldStatData
{
    public int itemId;
    public string itemName;
    public float defense;
}

[Serializable]
public class AccessoryStatData
{
    public int itemId;
    public string itemName;
}

public class DataManager : Singleton<DataManager>
{
    JsonSerializerSettings settings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Populate
    };

    public Dictionary<int, ItemData> itemDataDic { get; set; } = new Dictionary<int, ItemData>();
    public Dictionary<int, ItemOptionData> itemOptionDataDic { get; set; } = new Dictionary<int, ItemOptionData>();
    public Dictionary<int, List<ItemOptionData>> optionsByEquipmentTypeDic { get; set; } = new Dictionary<int, List<ItemOptionData>>();
    public Dictionary<int, List<UniqueItemOptionData>> uniqueItemOptionDataDic { get; set; } = new Dictionary<int, List<UniqueItemOptionData>>();
    public Dictionary<int, MonsterData> monsterDataDic { get; set; } = new Dictionary<int, MonsterData>();
    public Dictionary<int, List<MonsterDropData>> monsterDropDataDic { get; set; } = new Dictionary<int, List<MonsterDropData>>();
    public Dictionary<int, List<MonsterAttackData>> monsterAttackDataDic { get; set; } = new Dictionary<int, List<MonsterAttackData>>();

    public async UniTask OnInitialize()
    {
        await Initialize();
    }

    protected override async UniTask Initialize()
    {
        //메소드 내용은 프로젝트마다 다르게 작성.
        await DataDicLoad("gameData", itemDataDic, item => item.itemId);
        await DataDicLoad("itemOptionData", itemOptionDataDic, item => item.optionId);
        await DataDicLoad("uniqueItemOptionData", uniqueItemOptionDataDic, item => item.itemId);
        GroupOptionsByEquipmentType();
        await DataDicLoad("monsterData", monsterDataDic, item => item.monsterId);
        await DataDicLoad("monsterDropData", monsterDropDataDic, item => item.monsterId);
        await DataDicLoad("monsterAttackData", monsterAttackDataDic, item => item.monsterId);
        await ApplyWeaponData("weaponData");
        await ApplyShieldData("shieldData");
        await ApplyAccessoryData("accessoryData");
    }

    void GroupOptionsByEquipmentType()
    {
        optionsByEquipmentTypeDic.Clear();

        foreach (var item in itemOptionDataDic.Values)
        {
            if (item.allowedType == null) continue;

            foreach (int type in item.allowedType)
            {
                if (!optionsByEquipmentTypeDic.TryGetValue(type, out var options))
                {
                    options = new List<ItemOptionData>();
                    optionsByEquipmentTypeDic.Add(type, options);
                }

                if (!options.Contains(item))
                {
                    options.Add(item);
                    Debug.Log(item.optionId);
                }

            }

        }
    }

    private async UniTask DataListLoad<T>(string jsonName, List<T> saveList)
    {
        TextAsset textAsset = await ResourceManager.Instance.LoadAsset<TextAsset>(jsonName);

        var items = JsonConvert.DeserializeObject<List<T>>(textAsset.text, settings);
        if (items == null) return;

        saveList.AddRange(items);
    }

    private async UniTask DataDicLoad<T>(string jsonName, Dictionary<int, T> saveDic, Func<T, int> getId)
    {
        TextAsset _textAsset = await ResourceManager.Instance.LoadAsset<TextAsset>(jsonName);

        Debug.Log(_textAsset.text);
        var items = JsonConvert.DeserializeObject<List<T>>(_textAsset.text, settings);

        if (items == null) return;

        foreach (T item in items)
            saveDic.TryAdd(getId(item), item);
    }

    private async UniTask DataDicLoad<T>(string jsonName, Dictionary<int, List<T>> saveDic, Func<T, int> getId)
    {
        TextAsset _textAsset = await ResourceManager.Instance.LoadAsset<TextAsset>(jsonName);

        var items = JsonConvert.DeserializeObject<List<T>>(_textAsset.text, settings);

        if (items == null) return;

        foreach (T item in items)
        {
            int id = getId(item);

            if (!saveDic.TryGetValue(id, out List<T> list))
            {
                list = new List<T>();
                saveDic.Add(id, list);
            }

            list.Add(item);
        }
    }

    private async UniTask ApplyItemData<TStat, TItem>(string jsonName, Func<TStat, int> getItemId, Action<TItem, TStat> applyStats = null) where TItem : ItemData, new()
    {
        TextAsset textAsset = await ResourceManager.Instance.LoadAsset<TextAsset>(jsonName);

        var stats = JsonConvert.DeserializeObject<List<TStat>>(textAsset.text, settings);

        if (stats == null) return;

        foreach (var stat in stats)
        {
            int itemId = getItemId(stat);

            if (!itemDataDic.TryGetValue(itemId, out ItemData item))
                continue;

            var equipment = new TItem
            {
                itemId = item.itemId,
                itemName = item.itemName,
                itemNameId = item.itemNameId,
                icon = item.icon,
                itemKey = item.itemKey,
                itemRarity = item.itemRarity,

                category = item.category,
                equipmentType = item.equipmentType,
                isEquip = item.isEquip,
                gridIdx = item.gridIdx,
                canStack = item.canStack,
                maxStackAmount = item.maxStackAmount,
                healAmount = item.healAmount,
                itemExplanation = item.itemExplanation
            };

            applyStats?.Invoke(equipment, stat);
            itemDataDic[itemId] = equipment;
        }
    }

    private UniTask ApplyWeaponData(string jsonName)
    {
        return ApplyItemData<WeaponStatData, WeaponData>(jsonName, stat => stat.itemId, (weapon, stat) =>
        {
            weapon.attackDamage = stat.attackDamage;
            weapon.attackSpeed = stat.attackSpeed;
            weapon.attackRange = stat.attackRange;
        });
    }

    private UniTask ApplyShieldData(string jsonName)
    {
        return ApplyItemData<ShieldStatData, ShieldData>(jsonName, stat => stat.itemId, (shield, stat) =>
        {
            shield.defense = stat.defense;
        });
    }

    private UniTask ApplyAccessoryData(string jsonName)
    {
        return ApplyItemData<AccessoryStatData, AccessoryData>(jsonName, stat => stat.itemId);
    }

    private T CopyData<T>(T data) where T : class
    {
        if (data == null) return null;

        string json = JsonConvert.SerializeObject(data, settings);
        return (T)JsonConvert.DeserializeObject(json, data.GetType(), settings);
    }

    public T GetData<T>(int idx, Dictionary<int, T> dataDic) where T : class
    {
        return dataDic.TryGetValue(idx, out T data) ? CopyData(data) : null;
    }

    public List<T> GetDataList<T>(int idx, Dictionary<int, List<T>> dataDic) where T : class
    {
        if (!dataDic.TryGetValue(idx, out List<T> list)) return new List<T>();

        return list.ConvertAll(data => CopyData(data));
    }

    public T GetItemData<T>(int itemId) where T : ItemData
    {
        if (!itemDataDic.TryGetValue(itemId, out ItemData data))
            return null;

        if (data is T typedData)
            return typedData;

        return null;
    }
}
