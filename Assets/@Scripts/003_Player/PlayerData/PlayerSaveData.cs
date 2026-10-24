using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public int playCnt = 0;
    public PlayerProfileData profileData;
}

[Serializable]
public class PlayerProfileData
{
    public string playerId = "local-player";
    public int money;
    public List<ItemData> invenItems = new List<ItemData>();
}

