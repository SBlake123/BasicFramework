using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public int version = 1;
    public PlayerProfileData profile = new PlayerProfileData();
    public RaidSessionData activeRaid;
}

[Serializable]
public class PlayerProfileData
{
    public string playerId = "local-player";
    public int money;
    public List<ItemStackData> stashItems = new List<ItemStackData>();
}

[Serializable]
public class RaidSessionData
{
    public string mapId;
    public string startedAtUtc;
    public Vector3 playerPosition;
    public float health = 100f;
    public float stamina = 100f;
    public List<ItemStackData> carriedItems = new List<ItemStackData>();
}

[Serializable]
public class ItemStackData
{
    public string itemId;
    public int amount;

    public ItemStackData Clone()
    {
        return new ItemStackData
        {
            itemId = itemId,
            amount = amount
        };
    }
}
