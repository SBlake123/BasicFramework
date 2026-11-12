using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public int playCnt = 0;
    // Permanent storage is independent of the profile taken into a raid.
    public List<ItemData> stashItems = new List<ItemData>();
    public PlayerProfileData profileData;
}

[Serializable]
public class PlayerProfileData
{
    public string playerId = "local-player";
    public int money;
    public List<ItemData> invenItems = new List<ItemData>();
}

