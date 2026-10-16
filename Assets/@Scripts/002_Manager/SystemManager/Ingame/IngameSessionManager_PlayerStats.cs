using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class PlayerRuntimeStats
{
    public float currentHp { get; set; }
    public float maxHp { get; set; }
    public float baseHp { get; set; } = 100f;
    public float currentStamina { get; set; }
    public float maxStamina { get; set; } 
    public float baseStamina { get; set; } = 100f;

    public float attackDamage { get; set; }
    public float noWeaponDamage { get; set; } = 1f;
    public float defense { get; set; }
    public float noShieldDefense { get; set; } = 1f;
    public float attackFlatBonus { get; set; }
    public float attackPercentBonus { get; set; }
    public float defenseFlatBonus { get; set; }
    public float defensePercentBonus { get; set; }
    public float hpFlatBonus { get; set; }
    public float hpPercentBonus { get; set; }


    public void ResetEquipmentBonus()
    {
        attackFlatBonus = 0f;
        attackPercentBonus = 0f;

        defenseFlatBonus = 0f;
        defensePercentBonus = 0f;

        hpFlatBonus = 0f;
        hpPercentBonus = 0f;
    }

    public void CalculateAttackDamage(float baseAttackDamage)
    {
        attackDamage = (int)((baseAttackDamage + attackFlatBonus) * (1f + attackPercentBonus));
    }

    public void CalculateDefense(float baseDefense)
    {
        defense = (int)((baseDefense + defenseFlatBonus) * (1f + defensePercentBonus));
    }

    public void CalculateMaxHpAndStanima(float baseHp, float baseStamina)
    {
        bool wasFullHp = maxHp > 0f && currentHp >= maxHp;

        float hpRatio = maxHp > 0f ? Mathf.Clamp01(currentHp / maxHp) : 0f;

        maxHp = (int)((baseHp + hpFlatBonus) * (1f + hpPercentBonus));
        maxStamina = (int)baseStamina;

        currentHp = wasFullHp ? (int)maxHp : (int)(maxHp * hpRatio);
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
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

    public List<EquipmentEffectBase> specialAttributeList { get; set; } = new List<EquipmentEffectBase>();

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
        //playerIngameData.invenItems.Add(DataManager.Instance.GetItemData<WeaponData>(1001).DeepCopy());
        //playerIngameData.invenItems.Add(DataManager.Instance.GetItemData<ShieldData>(2001).DeepCopy());
        playerIngameData.invenItems.Add(ItemLootingManager.Instance.CreateStashItem(1001, 1, ItemRarity.UNCOMMON));
        playerIngameData.invenItems.Add(ItemLootingManager.Instance.CreateStashItem(2001, 2, ItemRarity.RARE));
        playerIngameData.invenItems.Add(ItemLootingManager.Instance.CreateStashItem(5000, 3, ItemRarity.UNIQUE));
        playerIngameData.invenItems.Add(ItemLootingManager.Instance.CreateStashItem(7000, 4, ItemRarity.UNIQUE));
        playerIngameData.invenItems.Add(ItemLootingManager.Instance.CreateStashItem(7000, 5, ItemRarity.UNIQUE));

        //ItemData itemData_1 = DataManager.Instance.GetItemData<WeaponData>(1001).DeepCopy();
        //itemData_1.gridIdx = 10;

        //ItemData itemData_3 = DataManager.Instance.GetItemData<ShieldData>(2001).DeepCopy();
        //itemData_3.gridIdx = 11;

        //playerIngameData.invenItems.Add(itemData_3);
        //playerIngameData.invenItems.Add(itemData_1);

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


                            GameObject obj = await ObjectPool.Instance.PopFromPool(string.Format(GScriptAddress.equipItem, currentWeaponData.itemKey), player.playerSkinBase.swingPivotTrf);
                            obj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
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

        float baseHp = playerStats.baseHp;

        float baseStamina = playerStats.baseStamina;

        Debug.Log($"baseAttackDamage : {baseAttackDamage}");

        playerStats.CalculateAttackDamage(baseAttackDamage);
        playerStats.CalculateDefense(baseDefense);
        playerStats.CalculateMaxHpAndStanima(baseHp, baseStamina);
        //SetDefense();
        IngameUIManager.Instance.SetHpSlider();

    }

    public void CalculateStatCoefficients()
    {
        playerStats.ResetEquipmentBonus();

        foreach (var item in specialAttributeList)
        {
            item.Release();
        }

        specialAttributeList.Clear();



        ApplyOptions(currentWeaponData?.additionalOptionsList);
        ApplyOptions(currentShieldData?.additionalOptionsList);
        
        foreach (AccessoryData item in currentAccessoryDataArr)
        {
            ApplyOptions(item?.additionalOptionsList);
        }

        //악세서리 추가

        void ApplyOptions(List<ItemOptionData> options)
        {
            if (options == null)
                return;

            foreach (ItemOptionData option in options)
            {
                switch ((StatType)option.optionId)
                {
                    case StatType.ADDITIONAL_DAMAGE:
                        {
                            playerStats.attackFlatBonus += option.value;

                            break;
                        }

                    case StatType.TOTAL_DAMAGE:
                        {
                            playerStats.attackPercentBonus += (float) option.value / 100;

                            break;
                        }

                    case StatType.ADDITIONAL_DEFENSE:
                        {
                            playerStats.defenseFlatBonus += option.value;

                            break;
                        }

                    case StatType.TOTAL_DEFENSE:
                        {

                            playerStats.defensePercentBonus += (float) option.value / 100;

                            break;
                        }

                    case StatType.ADDITIONAL_HEALTH:
                        {

                            playerStats.hpFlatBonus += option.value;

                            break;
                        }

                    case StatType.TOTAL_HEALTH:
                        {

                            playerStats.hpPercentBonus += (float) option.value / 100;

                            break;
                        }

                    case StatType.ATTACK_RANGE:
                        {
                            // PlayerRuntimeStats에 공격 범위 필드를 만든 뒤 계산
                            break;
                        }

                    default:
                        {
                            CheckSpecialAttribute(option);
                            break;
                        }
                }
            }
        }

        
    }


    async UniTask CheckSpecialAttribute(ItemOptionData itemOptionData)
    {
        switch (itemOptionData.optionId)
        {
            case 95001:
                {
                    FlameOnHitEffect flameOnHitEffect = new FlameOnHitEffect();
                    flameOnHitEffect.Initialize(player, itemOptionData);

                    specialAttributeList.Add(flameOnHitEffect);
                }
                break;
            case 95002:
                {
                    BurningAuraEffect burningAuraEffect = new BurningAuraEffect();
                    burningAuraEffect.Initialize(player, itemOptionData);

                    specialAttributeList.Add(burningAuraEffect);
                }
                break;
            default: break;
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

    public async UniTask RefreshPlayerStatus()
    {
        await SetPlayerStatus();
        playerStatsChanged?.Invoke();
    }

    public async UniTask RefreshPlayerStatus(ItemData itemData, EquipmentType equipmentType, IsEquip isEquip)
    {
        //await RefreshCurrentItem(itemData, equipmentType, isEquip);
        await SetPlayerStatus();
        playerStatsChanged?.Invoke();
    }

    public async UniTask RefreshPlayerStatus(EquipmentType equipmentType, IsEquip isEquip)
    {
       // await RefreshCurrentItem(equipmentType, isEquip);
        await SetPlayerStatus();
        playerStatsChanged?.Invoke();
    }

    public async UniTask RefreshCurrentItem(InventoryGrid_000_Base grid, IsEquip isEquip)
    {
        switch (isEquip)
        {
            case IsEquip.YES:
                {
                    switch (grid.gridCategory)
                    {
                        case GridCategory.WEAPON:
                            {
                                this.currentWeaponData = null;
                                player.currentWeapon = null;
                                if (player.playerSkinBase.swingPivotTrf.childCount > 0)
                                    ObjectPool.Instance.PushToPool(player.playerSkinBase.swingPivotTrf.GetChild(0).gameObject);
                                //Destroy(player.playerSkinBase.weaponTrf.GetChild(0).gameObject);


                                WeaponData currentWeaponData = (WeaponData)grid.itemData.DeepCopy();
                                this.currentWeaponData = currentWeaponData;
                                Debug.Log($"this.currentWeaponData.attackDamage : {this.currentWeaponData.attackDamage}");

                                GameObject obj = await ObjectPool.Instance.PopFromPool(string.Format(GScriptAddress.equipItem, currentWeaponData.itemKey), player.playerSkinBase.swingPivotTrf);
                                obj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

                                //Instantiate(await ResourceManager.Instance.LoadAsset<GameObject>(string.Format(GScriptAddress.equipItem, currentWeaponData.itemKey)), player.playerSkinBase.weaponTrf);

                                player.currentWeapon = player.playerSkinBase.swingPivotTrf.GetChild(0).GetComponent<WeaponBase>();
                                //데이터 기반으로 만들어야됨 player 위치에 
                                //Destroy(player.playerSkinBase.weaponTrf.GetChild(0).gameObject);
                                //데이터 기반으로 만들어야됨 player 위치에 
                            }
                            break;

                        case GridCategory.SHIELD:
                            {
                                this.currentShieldData = null;
                                player.currentShield = null;

                                if (player.playerSkinBase.shieldTrf.childCount > 0)
                                    ObjectPool.Instance.PushToPool(player.playerSkinBase.shieldTrf.GetChild(0).gameObject);

                                //Destroy(player.playerSkinBase.shieldTrf.GetChild(0).gameObject);

                                ShieldData currentShieldData = (ShieldData)grid.itemData.DeepCopy();
                                this.currentShieldData = currentShieldData;

                                await ObjectPool.Instance.PopFromPool(string.Format(GScriptAddress.equipItem, currentShieldData.itemKey), player.playerSkinBase.shieldTrf);

                                //Instantiate(await ResourceManager.Instance.LoadAsset<GameObject>(string.Format(GScriptAddress.equipItem, currentShieldData.itemKey)), player.playerSkinBase.shieldTrf);

                                player.currentShield = player.playerSkinBase.shieldTrf.GetChild(0).GetComponent<ShieldBase>();

                                //Destroy(player.playerSkinBase.shieldTrf.GetChild(0).gameObject);
                            }
                            break;

                        case GridCategory.ACC:
                            {
                                currentAccessoryDataArr[grid.gridIdx] = grid.itemData.DeepCopy();
                            }
                            break;
                    }
                }
                break;

            case IsEquip.NO:
                {
                    switch (grid.gridCategory)
                    {
                        case GridCategory.WEAPON:
                            {
                                this.currentWeaponData = null;
                                player.currentWeapon = null;
                                if (player.playerSkinBase.swingPivotTrf.childCount > 0)
                                    ObjectPool.Instance.PushToPool(player.playerSkinBase.swingPivotTrf.GetChild(0).gameObject);

                                //Destroy(player.playerSkinBase.weaponTrf.GetChild(0).gameObject);
                                //데이터 기반으로 만들어야됨 player 위치에 
                            }
                            break;

                        case GridCategory.SHIELD:
                            {
                                this.currentShieldData = null;
                                player.currentShield = null;

                                if (player.playerSkinBase.shieldTrf.childCount > 0)
                                    ObjectPool.Instance.PushToPool(player.playerSkinBase.shieldTrf.GetChild(0).gameObject);

                                //Destroy(player.playerSkinBase.shieldTrf.GetChild(0).gameObject);
                            }
                            break;

                        case GridCategory.ACC:
                            {
                                currentAccessoryDataArr[grid.gridIdx] = null;
                            }
                            break;
                    }
                }
                break;
        }
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
                    if (player.playerSkinBase.swingPivotTrf.childCount > 0)
                        ObjectPool.Instance.PushToPool(player.playerSkinBase.swingPivotTrf.GetChild(0).gameObject);
                    //Destroy(player.playerSkinBase.weaponTrf.GetChild(0).gameObject);


                    WeaponData currentWeaponData = (WeaponData)itemData.DeepCopy();
                    this.currentWeaponData = currentWeaponData;
                    Debug.Log($"this.currentWeaponData.attackDamage : {this.currentWeaponData.attackDamage}");

                    GameObject obj = await ObjectPool.Instance.PopFromPool(string.Format(GScriptAddress.equipItem, currentWeaponData.itemKey), player.playerSkinBase.swingPivotTrf);
                    obj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

                    //Instantiate(await ResourceManager.Instance.LoadAsset<GameObject>(string.Format(GScriptAddress.equipItem, currentWeaponData.itemKey)), player.playerSkinBase.weaponTrf);

                    player.currentWeapon = player.playerSkinBase.swingPivotTrf.GetChild(0).GetComponent<WeaponBase>();
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
                    if (player.playerSkinBase.swingPivotTrf.childCount > 0)
                        ObjectPool.Instance.PushToPool(player.playerSkinBase.swingPivotTrf.GetChild(0).gameObject);

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

    public int CalculateDamageDealt()
    {
        return (int)(playerStats.attackDamage * UnityEngine.Random.Range(0.8f, 1.2f));
    }

    public int CalculateDamageAfterDefense(int damage)
    {
        int defenseConstant = 40;

        float finalDamage = damage * defenseConstant / (defenseConstant + Mathf.Max(0f, playerStats.defense));

        return (int)(finalDamage * UnityEngine.Random.Range(0.9f, 1.1f));
    }

    //일단 인벤토리부터 해주자..
    //플레이어 현재 아이템, 상태, 스탯값 다 계산하기.

    //아이템 장착 시 알림 받고 스탯, 장착한 값 다 계산해서 플레이어한테 무기 다시 생성
    //
}
