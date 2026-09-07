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
    private string SavePath => Path.Combine(Application.persistentDataPath, "PlayerRaidData.dat");

    private PlayerSaveData saveData;
    private bool saveRequested;
    private bool saveWorkerRunning;

    public PlayerProfileData Profile => saveData.profile;
    public RaidSessionData ActiveRaid => saveData.activeRaid;
    public bool HasActiveRaid => ActiveRaid != null;

    public event Action<PlayerProfileData> OnProfileChanged;
    public event Action<RaidSessionData> OnRaidChanged;

    public void LoadOrCreate()
    {
        if (saveData != null)
        {
            return;
        }

        saveData = LoadFromDisk() ?? new PlayerSaveData();
        if (saveData.profile == null)
        {
            saveData.profile = new PlayerProfileData();
        }
    }

    public void BeginRaid(string mapId)
    {
        LoadOrCreate();

        if (saveData.activeRaid != null)
        {
            return;
        }

        saveData.activeRaid = new RaidSessionData
        {
            mapId = mapId,
            startedAtUtc = DateTime.UtcNow.ToString("O")
        };

        NotifyRaidChanged();
        SaveCheckpointNow();
    }

    public void UpdateRaidPosition(Vector3 position)
    {
        if (ActiveRaid == null)
        {
            return;
        }

        ActiveRaid.playerPosition = position;
        NotifyRaidChanged();
    }

    public void UpdateRaidVitals(float health, float stamina)
    {
        if (ActiveRaid == null)
        {
            return;
        }

        ActiveRaid.health = health;
        ActiveRaid.stamina = stamina;
        NotifyRaidChanged();
    }

    public void ReplaceRaidInventory(IReadOnlyList<ItemStackData> items)
    {
        if (ActiveRaid == null)
        {
            return;
        }

        ActiveRaid.carriedItems.Clear();
        foreach (ItemStackData item in items)
        {
            if (item != null)
            {
                ActiveRaid.carriedItems.Add(item.Clone());
            }
        }

        NotifyRaidChanged();
    }

    public void RequestCheckpoint()
    {
        if (ActiveRaid == null)
        {
            return;
        }

        saveRequested = true;

        if (!saveWorkerRunning)
        {
            SaveWhenQuiet().Forget();
        }
    }

    public void SaveCheckpointNow()
    {
        if (ActiveRaid == null)
        {
            return;
        }

        SaveToDisk();
    }

    public void CommitExtraction()
    {
        if (ActiveRaid == null)
        {
            return;
        }

        foreach (ItemStackData item in ActiveRaid.carriedItems)
        {
            AddToStash(item);
        }

        saveData.activeRaid = null;
        OnProfileChanged?.Invoke(Profile);
        OnRaidChanged?.Invoke(null);
        SaveToDisk();
    }

    public void CommitDeath()
    {
        if (ActiveRaid == null)
        {
            return;
        }

        // Raid inventory is intentionally discarded. Later, insured gear or a
        // death-recovery rule can be implemented here without touching Player.
        saveData.activeRaid = null;
        OnRaidChanged?.Invoke(null);
        SaveToDisk();
    }

    private async UniTask SaveWhenQuiet()
    {
        saveWorkerRunning = true;

        try
        {
            while (saveRequested)
            {
                saveRequested = false;
                await UniTask.Delay(500);
                SaveCheckpointNow();
            }
        }
        finally
        {
            saveWorkerRunning = false;
        }
    }

    private void AddToStash(ItemStackData incomingItem)
    {
        if (incomingItem == null || incomingItem.amount <= 0)
        {
            return;
        }

        ItemStackData stack = Profile.stashItems.Find(item => item.itemId == incomingItem.itemId);

        if (stack == null)
        {
            Profile.stashItems.Add(incomingItem.Clone());
            return;
        }

        stack.amount += incomingItem.amount;
    }

    private void NotifyRaidChanged()
    {
        OnRaidChanged?.Invoke(ActiveRaid);
    }

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
