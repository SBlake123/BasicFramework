const fs = require('fs');
const path = require('path');
function read(p) { const b=fs.readFileSync(p); try{return new TextDecoder('utf-8',{fatal:true}).decode(b);}catch{return new TextDecoder('euc-kr').decode(b);} }
function write(p,s){fs.writeFileSync(p,s.replace(/\r\n/g,'\n'),'utf8');}
function edit(p,fn){write(p,fn(read(p)));}
const scripts='Assets/@Scripts/';
for(const [oldName,newName] of [['02_Lobby','02_Ingame_Lobby'],['03_Ingame','03_Ingame_Raid_01']]){
 for(const ext of ['.unity','.unity.meta']) fs.renameSync(`Assets/@Scenes/${oldName}${ext}`,`Assets/@Scenes/${newName}${ext}`);
 for(const p of [scripts+'000_Base/GlobalString/GSceneName.cs','ProjectSettings/EditorBuildSettings.asset']) edit(p,s=>s.replaceAll(oldName,newName));
}
edit(scripts+'003_Player/PlayerData/PlayerSaveData.cs',s=>s.replace('public int playCnt = 0;','public int playCnt = 0;\n    // Permanent storage is independent of the profile taken into a raid.\n    public List<ItemData> stashItems = new List<ItemData>();'));
edit(scripts+'002_Manager/SystemManager/Player/PlayerDataManager.cs',s=>{
 s=s.replace('public PlayerProfileData Profile => saveData.profileData;',`public PlayerProfileData Profile => saveData.profileData;

    public List<ItemData> GetStashItems()
    {
        PlayerLoad();
        return (saveData.stashItems ?? new List<ItemData>()).ConvertAll(item => item.DeepCopy());
    }`);
 s=s.replace('public bool SaveLobbyProfile(PlayerProfileData profile)','public bool SaveLobbyProfile(PlayerProfileData profile, List<ItemData> stashItems = null)');
 s=s.replace('playCnt = saveData.playCnt,\r\n            profileData', 'playCnt = saveData.playCnt,\n            stashItems = (stashItems ?? saveData.stashItems ?? new List<ItemData>()).ConvertAll(item => item.DeepCopy()),\n            profileData');
 if(!s.includes('stashItems = (stashItems')) throw Error('SaveLobbyProfile insertion failed');
 return s;
});
edit(scripts+'007_Inventory/InventoryGrid/InventoryGrid_000_Base.cs',s=>s.replace('    LOOTING\r\n','    LOOTING,\n    STORAGE\n'));
for(const name of ['WeaponBase','ShieldBase']) edit(scripts+`004_Item/ItemScriptBase/${name}.cs`,s=>s.replace('healAmount = healAmount,','healAmount = healAmount,\n            itemExplanation = itemExplanation,'));
edit(scripts+'007_Inventory/Inventory_000_Base.cs',s=>{
 s=s.replace('grid.inventoryCategory = InventoryCategory.LOOTING;', 'grid.inventoryCategory = IsLobbyInventory ? InventoryCategory.STORAGE : InventoryCategory.LOOTING;');
 s=s.replace('public void SaveLootingBoxItems()\r\n    {','public void SaveLootingBoxItems()\n    {\n        if (IsLobbyInventory) return;');
 s=s.replace('        options.Sort((a, b) => a.optionId.CompareTo(b.optionId));','        options?.Sort((a, b) => a.optionId.CompareTo(b.optionId));');
 s=s.replace('        obj.transform.localPosition = Vector3.zero;',`        var group = obj.GetComponent<CanvasGroup>();
        if (group != null) { group.blocksRaycasts = true; group.alpha = 1f; }
        obj.transform.localPosition = Vector3.zero;`);
 let start=s.indexOf('    public async UniTask InventoryClosed()');
 let end=s.indexOf('    public void SettingItemConfigWindow(',start);
 if(start<0||end<0)throw Error('inventory method range');
 s=s.slice(0,start)+`    public UniTask InventoryClosed()
    {
        CancelDrags();
        IngameSessionManager.Instance.playerIngameData.invenItems = ExtractingInvenItems();
        SaveLootingBoxItems();
        return UniTask.CompletedTask;
    }

`+s.slice(end);
 return s;
});
// RefreshCurrentItem must assign the data to the spawned weapon instance too.
edit(scripts+'002_Manager/SystemManager/Ingame/IngameSessionManager_PlayerStats.cs',s=>s.replace('player.currentWeapon = player.playerSkinBase.swingPivotTrf.GetChild(0).GetComponent<WeaponBase>();', 'player.currentWeapon = obj.GetComponent<WeaponBase>();\n                                player.currentWeapon.weaponData = currentWeaponData;'));
