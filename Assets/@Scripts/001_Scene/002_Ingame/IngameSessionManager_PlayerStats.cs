using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public partial class IngameSessionManager : MonoSingleton<IngameSessionManager>
{
    public int playerHp;
    public int playerStamina;

    public ItemData currentWeaponData;
    public ItemData currentShieldData;
    public ItemData[] currentAccessoryDataArr = new ItemData[3];

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

    public async UniTask PlayerEquipmentSetting()
    {
        foreach (var item in playerIngameData.invenItems)
        {
            if (item.isEquip == (int)IsEquip.YES)
            {
                switch ((EquipmentType)item.equipmentType)
                {
                    case EquipmentType.Weapon:
                        {
                            ItemData currentWeaponData = item.DeepCopy();
                            this.currentWeaponData = currentWeaponData;

                            Instantiate(await ResourceManager.Instance.LoadAsset<GameObject>(string.Format(GScriptAddress.equipItem, currentWeaponData.itemKey)), player.playerSkinBase.weaponTrf);

                            player.currentWeapon = player.playerSkinBase.shieldTrf.GetChild(0).GetComponent<WeaponBase>();
                        }
                        break;

                    case EquipmentType.Shield:
                        {
                            ItemData currentShieldData = item.DeepCopy();
                            this.currentShieldData = currentShieldData;
                            Instantiate(await ResourceManager.Instance.LoadAsset<GameObject>(string.Format(GScriptAddress.equipItem, currentShieldData.itemKey)), player.playerSkinBase.shieldTrf);

                            player.currentShield = player.playerSkinBase.shieldTrf.GetChild(0).GetComponent<ShieldBase>();
                        }
                        break;
                }
            }
        }
    }

    public async UniTask CurrentItemRefresh(ItemData itemData)
    {
        if (itemData == null) return;
        if (itemData.isEquip == (int)IsEquip.NO) return;

        switch ((EquipmentType)itemData.equipmentType)
        {
            case EquipmentType.Weapon:
                {
                    this.currentWeaponData = null;
                    player.currentWeapon = null;
                    if (player.playerSkinBase.weaponTrf.childCount > 0)
                        Destroy(player.playerSkinBase.weaponTrf.GetChild(0).gameObject);


                    ItemData currentWeaponData = itemData.DeepCopy();
                    this.currentWeaponData = currentWeaponData;

                    Instantiate(await ResourceManager.Instance.LoadAsset<GameObject>(string.Format(GScriptAddress.equipItem, currentWeaponData.itemKey)), player.playerSkinBase.weaponTrf);

                    player.currentWeapon = player.playerSkinBase.weaponTrf.GetChild(0).GetComponent<WeaponBase>();
                    //데이터 기반으로 만들어야됨 player 위치에 
                }
                break;

            case EquipmentType.Shield:
                {
                    this.currentShieldData = null;
                    player.currentShield = null;

                    if (player.playerSkinBase.shieldTrf.childCount > 0)
                        Destroy(player.playerSkinBase.shieldTrf.GetChild(0).gameObject);

                    ItemData currentShieldData = itemData.DeepCopy();
                    this.currentShieldData = currentShieldData;
                    Instantiate(await ResourceManager.Instance.LoadAsset<GameObject>(string.Format(GScriptAddress.equipItem, currentShieldData.itemKey)), player.playerSkinBase.shieldTrf);

                    player.currentShield = player.playerSkinBase.shieldTrf.GetChild(0).GetComponent<ShieldBase>();
                }
                break;

            case EquipmentType.Accessory:
                {
                    ItemData currentAccessoryData = itemData.DeepCopy();
                    currentAccessoryDataArr[currentAccessoryData.gridIdx] = null;



                    currentAccessoryDataArr[currentAccessoryData.gridIdx] = currentAccessoryData;
                }
                break;
        }
    }


    //일단 인벤토리부터 해주자..
    //플레이어 현재 아이템, 상태, 스탯값 다 계산하기.

    //아이템 장착 시 알림 받고 스탯, 장착한 값 다 계산해서 플레이어한테 무기 다시 생성
    //
}
