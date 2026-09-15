using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Stores data that must survive scene changes. This intentionally owns a
/// separate file so it does not overwrite the project's existing SaveLoadManager data.
/// </summary>
public class PlayerDataManager : Singleton<PlayerDataManager>
{
    private string SavePath => Path.Combine(Application.persistentDataPath, GScriptAddress.localSaveDataName);

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

    //인게임안에서일어나는 데이터 모두 저장. 

    private PlayerSaveData LoadFromDisk()
    {
        if (!File.Exists(SavePath))
        {
            return null;
        }

        try
        {
            string encryptedJson = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<PlayerSaveData>(GSecurity.DecryptString(encryptedJson));
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[PlayerDataManager] Player data could not be loaded: {exception.Message}");
            return null;
        }
    }

    private void SaveToDisk()
    {
        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SavePath, GSecurity.EncryptString(json));
        }
        catch (Exception exception)
        {
            Debug.LogError($"[PlayerDataManager] Player data could not be saved: {exception.Message}");
        }
    }
}
