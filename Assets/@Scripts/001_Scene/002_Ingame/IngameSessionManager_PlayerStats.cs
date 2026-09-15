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
        playerIngameData.invenItems.Add(DataManager.Instance.GetItemData(1001).DeepCopy());
        playerIngameData.invenItems.Add(DataManager.Instance.GetItemData(1002).DeepCopy());

        ItemData itemData_1 = DataManager.Instance.GetItemData(1001).DeepCopy();
        itemData_1.gridIdx = 10;

        ItemData itemData_3 = DataManager.Instance.GetItemData(1002).DeepCopy();
        itemData_3.gridIdx = 11;

        playerIngameData.invenItems.Add(itemData_3);
        playerIngameData.invenItems.Add(itemData_1);

    }

    //일단 인벤토리부터 해주자..
    //플레이어 현재 아이템, 상태, 스탯값 다 계산하기.
    //
}
