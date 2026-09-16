using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public int version = 1;
    public PlayerProfileData profileData;
    public PlayerProfileData raidData;
}

[Serializable]
public class PlayerProfileData
{
    public string playerId = "local-player";
    public int money;
    public int playerHp;
    public int playerStamina;
    public int attackDamage;
    public int defense;
    public List<ItemData> invenItems = new List<ItemData>();
}

