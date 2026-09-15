using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public partial class IngameSessionManager : MonoSingleton<IngameSessionManager>
{
    public int playerHp;
    public int playerStamina;


    public PlayerProfileData playerIngameData;

    public void PlayerDataSetting()
    {
        var profile = PlayerDataManager.Instance.Profile;

        playerIngameData = new PlayerProfileData
        {
            playerId = profile.playerId,
            money = profile.money,
            playerHp = profile.playerHp,
            playerStamina = profile.playerStamina,
            invenItems = profile.invenItems.ConvertAll(item => item.DeepCopy()),
        };
    }


    public void DummyDataSetting()
    { 
        playerIngameData.invenItems.Add(DataManager.Instance.GetItemData(1001));
        playerIngameData.invenItems.Add(DataManager.Instance.GetItemData(1002));
    }

    //일단 인벤토리부터 해주자..
    //플레이어 현재 아이템, 상태, 스탯값 다 계산하기.
    //
}
