using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public partial class IngameSessionManager : MonoSingleton<IngameSessionManager>
{
    public int playerHp;
    public int playerStamina;


    public PlayerProfileData ingamePlayerData;

    public void PlayerDataSetting()
    {
        var profile = PlayerDataManager.Instance.Profile;

        ingamePlayerData = new PlayerProfileData
        {
            playerId = profile.playerId,
            money = profile.money,
            playerHp = profile.playerHp,
            playerStamina = profile.playerStamina,
            stashItems = profile.stashItems.ConvertAll(item => item.DeepCopy()),
            equipItems = profile.equipItems.ConvertAll(item => item.DeepCopy())
        };
    }


    public void DummyDataSetting()
    {
        ingamePlayerData.stashItems[0] = DataManager.Instance.GetItemData(1001);
        ingamePlayerData.stashItems[1] = DataManager.Instance.GetItemData(1002);
    }

    //일단 인벤토리부터 해주자..
    //플레이어 현재 아이템, 상태, 스탯값 다 계산하기.
    //
}
