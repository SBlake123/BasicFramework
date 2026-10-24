using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class SavedItemDataConverter : JsonConverter
{
    public override bool CanWrite => false;
    public override bool CanConvert(Type type) => type == typeof(ItemData);
    public override object ReadJson(
        JsonReader reader, Type type, object existing, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;

        var json = JObject.Load(reader);

        ItemData item = (EquipmentType)(json.Value<int?>("equipmentType") ?? 0) switch
        {
            EquipmentType.Weapon => new WeaponData(),
            EquipmentType.Shield => new ShieldData(),
            EquipmentType.Accessory => new AccessoryData(),
            _ => new ItemData()
        };

        using var itemReader = json.CreateReader();
        serializer.Populate(itemReader, item);
        return item;
    }

    public override void WriteJson(
        JsonWriter writer, object value, JsonSerializer serializer)
        => throw new NotSupportedException();
}
public class PlayerDataManager : Singleton<PlayerDataManager>
{
    private string SavePath => Path.Combine(Application.persistentDataPath, GScriptAddress.localSaveDataName);

    private readonly JsonSerializerSettings saveSettings = new()
    {
        TypeNameHandling = TypeNameHandling.None,
        Converters = { new SavedItemDataConverter() }
    };

    private PlayerSaveData saveData;
    public PlayerProfileData Profile => saveData.profileData;

    public event Action<PlayerProfileData> OnProfileChanged;
    public event Action<PlayerProfileData> OnRaidChanged;

    public void PlayerLoad()
    {
        if (saveData != null)
        {
            return;
        }

        saveData = LoadFromDisk() ?? new PlayerSaveData();

        if (saveData.profileData == null)
        {
            saveData.profileData = new PlayerProfileData();
        }
    }

    public void DeleteData(bool deleteAll)
    {
        int playCnt = 0;
        if (!deleteAll)
        {
            PlayerLoad();
            playCnt = saveData.playCnt;
        }

        if (File.Exists(SavePath))
            File.Delete(SavePath);

        saveData = new PlayerSaveData
        {
            playCnt = playCnt,
            profileData = new PlayerProfileData()
        };
    }

    //인게임안에서일어나는 데이터 모두 저장. 

    public PlayerSaveData LoadFromDisk()
    {
        if (!File.Exists(SavePath))
        {
            return null;
        }

        try
        {
            string encryptedJson = File.ReadAllText(SavePath);
            Debug.Log(GSecurity.DecryptString(encryptedJson));
            return JsonConvert.DeserializeObject<PlayerSaveData>(GSecurity.DecryptString(encryptedJson), saveSettings);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[PlayerDataManager] Player data could not be loaded: {exception.Message}");
            return null;
        }
    }

    public void SaveToDisk(List<ItemData> itemDataList)
    {
        try
        {
            saveData.playCnt++;
            //saveData.profileData.money = saveData.profileData.money;
            saveData.profileData.invenItems = itemDataList.ConvertAll(item => item.DeepCopy());

            string json = JsonConvert.SerializeObject(saveData, saveSettings);
            File.WriteAllText(SavePath, GSecurity.EncryptString(json));
        }
        catch (Exception exception)
        {
            Debug.LogError($"[PlayerDataManager] Player data could not be saved: {exception.Message}");
        }
    }

   
    //public void MakeSaveData(List<ItemData> itemDataList)
    //{

    //}
}
