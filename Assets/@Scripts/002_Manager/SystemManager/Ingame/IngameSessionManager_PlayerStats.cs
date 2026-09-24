using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerRuntimeStats
{
    public int currentHp { get; set; }
    public int maxHp { get; set; } = 60;
    public int currentStamina { get; set; }
    public int maxStamina { get; set; } = 100;
    public float attackDamage { get; set; }
    public float noWeaponDamage { get; set; } = 1;
    public float defense { get; set; }
    public float noShieldDefense { get; set; } = 1;
    public float attackFlatBonus { get; set; }
    public float attackPercentBonus { get; set; }
    public float defenseFlatBonus { get; set; }
    public float defensePercentBonus { get; set; }

    public void ResetEquipmentBonus()
    {
        attackFlatBonus = 0f;
        attackPercentBonus = 0f;

        defenseFlatBonus = 0f;
        defensePercentBonus = 0f;
    }

    public void CalculateAttackDamage(float baseAttackDamage)
    {
        attackDamage = (baseAttackDamage + attackFlatBonus) * (1f + attackPercentBonus);
    }

    public void CalculateDefense(float baseDefense)
    {
        defense = (baseDefense + defenseFlatBonus) * (1f + defensePercentBonus);
    }

    public void CalculateMaxHpAndStanima(float baseHp, float baseStamina)
    {
        maxHp = (int)baseHp;
        maxHp = (int)baseStamina;
    }
}

public partial class IngameSessionManager : MonoSingleton<IngameSessionManager>
{
    public PlayerRuntimeStats playerStats = new PlayerRuntimeStats();
    public WeaponData currentWeaponData { get; set; }
    public ShieldData currentShieldData { get; set; }
    public ItemData[] currentAccessoryDataArr { get; set; } = new ItemData[3];
    public PlayerProfileData playerIngameData { get; set; }

    public event Action playerStatsChanged;

    public void SetPlayerData()
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

    public void SetDummyData()
    {
        playerIngameData.invenItems.Add(DataManager.Instance.GetItemData<WeaponData>(1001).DeepCopy());
        playerIngameData.invenItems.Add(DataManager.Instance.GetItemData<ShieldData>(2001).DeepCopy());

        ItemData itemData_1 = DataManager.Instance.GetItemData<WeaponData>(1001).DeepCopy();
        itemData_1.gridIdx = 10;

        ItemData itemData_3 = DataManager.Instance.GetItemData<ShieldData>(2001).DeepCopy();
        itemData_3.gridIdx = 11;

        playerIngameData.invenItems.Add(itemData_3);
        playerIngameData.invenItems.Add(itemData_1);

    }

    public async UniTask SetPlayerEquipment()
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
                            this.currentWeaponData = (WeaponData)currentWeaponData;


                            await ObjectPool.Instance.PopFromPool(string.Format(GScriptAddress.equipItem, currentWeaponData.itemKey), player.playerSkinBase.weaponTrf);

                            //Instantiate(await ResourceManager.Instance.LoadAsset<GameObject>(string.Format(GScriptAddress.equipItem, currentWeaponData.itemKey)), player.playerSkinBase.weaponTrf);

                            player.currentWeapon = player.playerSkinBase.shieldTrf.GetChild(0).GetComponent<WeaponBase>();
                        }
                        break;

                    case EquipmentType.Shield:
                        {
                            ItemData currentShieldData = item.DeepCopy();
                            this.currentShieldData = (ShieldData)currentShieldData;

                            await ObjectPool.Instance.PopFromPool(string.Format(GScriptAddress.equipItem, currentShieldData.itemKey), player.playerSkinBase.shieldTrf);

                            //Instantiate(await ResourceManager.Instance.LoadAsset<GameObject>(string.Format(GScriptAddress.equipItem, currentShieldData.itemKey)), player.playerSkinBase.shieldTrf);

                            player.currentShield = player.playerSkinBase.shieldTrf.GetChild(0).GetComponent<ShieldBase>();
                        }
                        break;
                }
            }
        }
    }

    public async UniTask SetPlayerStatus()
    {
        CalculateStatCoefficients();

        float baseAttackDamage = currentWeaponData != null
        ? currentWeaponData.attackDamage
        : playerStats.noWeaponDamage;

        float baseDefense = currentShieldData != null
        ? currentShieldData.defense
        : playerStats.noShieldDefense;

        float baseHp = playerStats.maxHp;

        float baseStamina = playerStats.maxStamina;

        Debug.Log($"baseAttackDamage : {baseAttackDamage}");

        playerStats.CalculateAttackDamage(baseAttackDamage);
        playerStats.CalculateDefense(baseDefense);
        playerStats.CalculateMaxHpAndStanima(baseHp, baseStamina);
        //SetDefense();
        SetHpAndStamina();
        IngameUIManager.Instance.SetHpSliderText();

    }

    public void CalculateStatCoefficients()
    {
        playerStats.ResetEquipmentBonus();

        ApplyOptions(currentWeaponData?.additionalOptionsList);
        ApplyOptions(currentShieldData?.additionalOptionsList);

        //악세서리 추가

        void ApplyOptions(List<ItemStatModifierData> options)
        {
            if (options == null)
                return;

            foreach (ItemStatModifierData option in options)
            {
                switch (option.statType)
                {
                    case StatType.ADDIDTIONAL_DAMAGE:
                        {
                            if (option.modifierType == ModifierType.FLAT)
                                playerStats.attackFlatBonus += option.value;
                            else
                                playerStats.attackPercentBonus += option.value;

                            break;
                        }

                    case StatType.TOTAL_DAMAGE:
                        {
                            if (option.modifierType == ModifierType.PERCENT)
                                playerStats.attackPercentBonus += option.value;
                            else
                                playerStats.attackFlatBonus += option.value;

                            break;
                        }

                    case StatType.ADDIDTIONAL_DEFENSE:
                        {
                            if (option.modifierType == ModifierType.FLAT)
                                playerStats.defenseFlatBonus += option.value;
                            else
                                playerStats.defensePercentBonus += option.value;

                            break;
                        }

                    case StatType.ATTACK_RANGE:
                        {
                            // PlayerRuntimeStats에 공격 범위 필드를 만든 뒤 계산
                            break;
                        }
                }
            }
        }
    }

    //public void SetAttackDamage()
    //{
    //    //무기 공격력 -> 배수 계산까지 할 것
    //    //없으면 맨손
    //}

    //public void SetDefense()
    //{
    //    //
    //}

    public void SetHpAndStamina()
    {
        playerStats.currentHp = playerStats.maxHp;
    }

    public async UniTask RefreshPlayerStatus(ItemData itemData, EquipmentType equipmentType, IsEquip isEquip)
    {
        await RefreshCurrentItem(itemData, equipmentType, isEquip);
        await SetPlayerStatus();
        playerStatsChanged?.Invoke();
    }

    public async UniTask RefreshPlayerStatus(EquipmentType equipmentType, IsEquip isEquip)
    {
        await RefreshCurrentItem(equipmentType, isEquip);
        await SetPlayerStatus();
        playerStatsChanged?.Invoke();
    }

    public async UniTask RefreshCurrentItem(ItemData itemData, EquipmentType equipmentType, IsEquip isEquip)
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
                        ObjectPool.Instance.PushToPool(player.playerSkinBase.weaponTrf.GetChild(0).gameObject);
                    //Destroy(player.playerSkinBase.weaponTrf.GetChild(0).gameObject);


                    WeaponData currentWeaponData = (WeaponData)itemData.DeepCopy();
                    this.currentWeaponData = currentWeaponData;
                    Debug.Log($"this.currentWeaponData.attackDamage : {this.currentWeaponData.attackDamage}");

                    await ObjectPool.Instance.PopFromPool(string.Format(GScriptAddress.equipItem, currentWeaponData.itemKey), player.playerSkinBase.weaponTrf);


                    //Instantiate(await ResourceManager.Instance.LoadAsset<GameObject>(string.Format(GScriptAddress.equipItem, currentWeaponData.itemKey)), player.playerSkinBase.weaponTrf);

                    player.currentWeapon = player.playerSkinBase.weaponTrf.GetChild(0).GetComponent<WeaponBase>();
                    //데이터 기반으로 만들어야됨 player 위치에 
                }
                break;

            case EquipmentType.Shield:
                {
                    this.currentShieldData = null;
                    player.currentShield = null;

                    if (player.playerSkinBase.shieldTrf.childCount > 0)
                        ObjectPool.Instance.PushToPool(player.playerSkinBase.shieldTrf.GetChild(0).gameObject);

                    //Destroy(player.playerSkinBase.shieldTrf.GetChild(0).gameObject);

                    ShieldData currentShieldData = (ShieldData)itemData.DeepCopy();
                    this.currentShieldData = currentShieldData;

                    await ObjectPool.Instance.PopFromPool(string.Format(GScriptAddress.equipItem, currentShieldData.itemKey), player.playerSkinBase.shieldTrf);

                    //Instantiate(await ResourceManager.Instance.LoadAsset<GameObject>(string.Format(GScriptAddress.equipItem, currentShieldData.itemKey)), player.playerSkinBase.shieldTrf);

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

    public async UniTask RefreshCurrentItem(EquipmentType equipmentType, IsEquip isEquip)
    {
        switch (equipmentType)
        {
            case EquipmentType.Weapon:
                {
                    this.currentWeaponData = null;
                    player.currentWeapon = null;
                    if (player.playerSkinBase.weaponTrf.childCount > 0)
                        ObjectPool.Instance.PushToPool(player.playerSkinBase.weaponTrf.GetChild(0).gameObject);

                    //Destroy(player.playerSkinBase.weaponTrf.GetChild(0).gameObject);
                    //데이터 기반으로 만들어야됨 player 위치에 
                }
                break;

            case EquipmentType.Shield:
                {
                    this.currentShieldData = null;
                    player.currentShield = null;

                    if (player.playerSkinBase.shieldTrf.childCount > 0)
                        ObjectPool.Instance.PushToPool(player.playerSkinBase.shieldTrf.GetChild(0).gameObject);

                    //Destroy(player.playerSkinBase.shieldTrf.GetChild(0).gameObject);
                }
                break;

            case EquipmentType.Accessory:
                {

                }
                break;
        }
    }

    //일단 인벤토리부터 해주자..
    //플레이어 현재 아이템, 상태, 스탯값 다 계산하기.

    //아이템 장착 시 알림 받고 스탯, 장착한 값 다 계산해서 플레이어한테 무기 다시 생성
    //
}
