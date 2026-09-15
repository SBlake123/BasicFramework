using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }

    private async UniTask DataDicLoad(string jsonName, Dictionary<int, ItemData> saveDic)
    {
        Debug.Log("Data Dic Load Front");
        TextAsset _textAsset = await ResourceManager.Instance.LoadAsset<TextAsset>(jsonName);
        Debug.Log("Data Dic Load Last");

        Debug.Log(_textAsset.text);
        var items = JsonConvert.DeserializeObject<List<ItemData>>(_textAsset.text, settings);


        foreach (var item in items)
        {
            if (saveDic.ContainsKey(item.itemId)) continue;

            saveDic.Add(item.itemId, item);
        }


    }

    public ItemData GetItemData(int itemId)
    {
        if (itemDataDic.TryGetValue(itemId, out ItemData data))
            return data;

        Debug.LogError($"아이템 데이터 없음: {itemId}");
        return null;
    }  
}
