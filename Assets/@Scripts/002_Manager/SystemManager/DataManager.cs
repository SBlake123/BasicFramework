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

public class DataManager : Singleton<DataManager>
{
    JsonSerializerSettings settings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Populate
    };

    public Dictionary<int, ItemData> itemDataDic { get; set; } = new Dictionary<int, ItemData>();
    public Dictionary<int, ItemOptionData> itemOptionDataDic { get; set; } = new Dictionary<int, ItemOptionData>();

    public Dictionary<int, MonsterData> monsterDataDic { get; set; } = new Dictionary<int, MonsterData>();
    public Dictionary<int, List<MonsterDropData>> monsterDropDataDic { get; set; } = new Dictionary<int, List<MonsterDropData>>();
    public Dictionary<int, List<MonsterAttackData>> monsterAttackDataDic { get; set; } = new Dictionary<int, List<MonsterAttackData>>();


    public async UniTask OnInitialize()
    {
        await Initialize();
    }

    protected override async UniTask Initialize()
    {
        Debug.Log("DataMAnager Initialize");

        //메소드 내용은 프로젝트마다 다르게 작성.
        await DataDicLoad("gameData", itemDataDic, item => item.itemId);
        await DataDicLoad("monsterData", monsterDataDic, item => item.monsterId);
        await DataDicLoad("monsterDropData", monsterDropDataDic, item => item.monsterId);
        await DataDicLoad("monsterAttackData", monsterAttackDataDic, item => item.monsterId);
        Debug.Log("gameData");
        await ApplyWeaponData("weaponData");
        Debug.Log("weaponData");
        await ApplyShieldData("shieldData");
        Debug.Log("shieldData");

    }

    //private async UniTask DataDicLoad(string jsonName, Dictionary<int, ItemData> saveDic)
    //{
    //    TextAsset _textAsset = await ResourceManager.Instance.LoadAsset<TextAsset>(jsonName);

    //    Debug.Log(_textAsset.text);
    //    var items = JsonConvert.DeserializeObject<List<ItemData>>(_textAsset.text, settings);

    //    foreach (var item in items)
    //    {
    //        if (saveDic.ContainsKey(item.itemId)) continue;

    //        saveDic.Add(item.itemId, item);
    //    }
    //}

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

        Debug.Log(_textAsset.text);
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
        //switch (typeof(T))
        //{
        //    case var type when type == typeof(ItemData):
        //        {
        //            foreach (var item in items)
        //            {
        //                if (saveDic.ContainsKey(getId(item))) continue;

        //                saveDic.TryAdd(getId(item), item);
        //            }
        //        }
        //        break;

        //    case var type when type == typeof(MonsterData)
        //                    || type == typeof(MonsterAttackData)
        //                    || type == typeof(MonsterDropData):
        //        {

        //        }
        //        break;
        //}
    }


    private async UniTask ApplyWeaponData(string jsonName)
    {
        TextAsset textAsset = await ResourceManager.Instance.LoadAsset<TextAsset>(jsonName);

        var weaponStats = JsonConvert.DeserializeObject<List<WeaponStatData>>(textAsset.text, settings);

        foreach (var stat in weaponStats)
        {
            if (!itemDataDic.TryGetValue(stat.itemId, out ItemData item))
            {
                continue;
            }

            var weapon = new WeaponData
            {
                itemId = item.itemId,
                itemName = item.itemName,
                itemNameId = item.itemNameId,
                icon = item.icon,
                itemKey = item.itemKey,

                category = item.category,
                equipmentType = item.equipmentType,
                isEquip = item.isEquip,
                gridIdx = item.gridIdx,
                canStack = item.canStack,
                maxStackAmount = item.maxStackAmount,
                healAmount = item.healAmount,

                attackDamage = stat.attackDamage,
                attackSpeed = stat.attackSpeed,
                attackRange = stat.attackRange
            };

            itemDataDic[stat.itemId] = weapon;
        }
    }

    private async UniTask ApplyShieldData(string jsonName)
    {
        TextAsset textAsset = await ResourceManager.Instance.LoadAsset<TextAsset>(jsonName);

        var weaponStats = JsonConvert.DeserializeObject<List<ShieldStatData>>(textAsset.text, settings);

        foreach (var stat in weaponStats)
        {
            if (!itemDataDic.TryGetValue(stat.itemId, out ItemData item))
            {
                continue;
            }

            var weapon = new ShieldData
            {
                itemId = item.itemId,
                itemName = item.itemName,
                itemNameId = item.itemNameId,
                icon = item.icon,
                itemKey = item.itemKey,

                category = item.category,
                equipmentType = item.equipmentType,
                isEquip = item.isEquip,
                gridIdx = item.gridIdx,
                canStack = item.canStack,
                maxStackAmount = item.maxStackAmount,
                healAmount = item.healAmount,

                defense = stat.defense
            };

            itemDataDic[stat.itemId] = weapon;
        }
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

        Debug.LogError($"아이템 데이터 없음: {itemId}");
        return null;

    }
}
