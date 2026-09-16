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

    public async UniTask OnInitialize()
    {
        await Initialize();
    }

    protected override async UniTask Initialize()
    {
        Debug.Log("DataMAnager Initialize");

        //메소드 내용은 프로젝트마다 다르게 작성.
        await DataDicLoad("gameData", itemDataDic);
        Debug.Log("gameData");
        await ApplyWeaponData("weaponData");
        Debug.Log("weaponData");
        await ApplyShieldData("shieldData");
        Debug.Log("shieldData");

    }

    private async UniTask DataDicLoad(string jsonName, Dictionary<int, ItemData> saveDic)
    {
        TextAsset _textAsset = await ResourceManager.Instance.LoadAsset<TextAsset>(jsonName);

        Debug.Log(_textAsset.text);
        var items = JsonConvert.DeserializeObject<List<ItemData>>(_textAsset.text, settings);

        foreach (var item in items)
        {
            if (saveDic.ContainsKey(item.itemId)) continue;

            saveDic.Add(item.itemId, item);
        }
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
