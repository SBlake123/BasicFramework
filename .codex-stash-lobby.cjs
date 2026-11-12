const fs=require('fs');
function edit(p,f){let b=fs.readFileSync(p),s;try{s=new TextDecoder('utf-8',{fatal:true}).decode(b)}catch{s=new TextDecoder('euc-kr').decode(b)} fs.writeFileSync(p,f(s.replace(/\r\n/g,'\n')));}
edit('Assets/@Scripts/002_Manager/SceneManager/LobbySceneManager.cs',s=>{
s=s.replace('    GO_TO_TITLE\n','    GO_TO_TITLE,\n    STASH\n');
s=s.replace('[SerializeField] private SpriteRenderer blacksmith;','[SerializeField] private SpriteRenderer blacksmith;\n    [SerializeField] private SpriteRenderer stash;');
s=s.replace('inventoryPage.stateBaseSceneManager = this;',`inventoryPage.stateBaseSceneManager = this;
        inventoryPage.CloseRequested = CloseInventory;
        inventoryPage.Inventory_000_Base.ConfigureLobbyInventory();
        optionPage.CloseRequested = () => ChangeState((int)LobbySceneState.EXPLORE);
        optionPage.QuitRequested = () => ChangeState((int)LobbySceneState.GO_TO_TITLE);`);
s=s.replace('        bool exploring = state == LobbySceneState.EXPLORE;',`        if (state != LobbySceneState.STASH)
            inventoryPage.Inventory_000_Base.CloseStorage();
        bool exploring = state == LobbySceneState.EXPLORE;`);
s=s.replace('            case LobbySceneState.INVENTORY:\n            case LobbySceneState.UPGRADE:', '            case LobbySceneState.STASH:\n            case LobbySceneState.INVENTORY:\n            case LobbySceneState.UPGRADE:');
s=s.replace('bool onPortal = Vector2.Distance(position, portal.position) <= PortalRadius;',`bool onPortal = Vector2.Distance(position, portal.position) <= PortalRadius;
        bool nearStash = stash != null && Vector2.Distance(position, stash.transform.position) <= NpcInteractionDistance;`);
s=s.replace('nearNpc ? "대장장이를 눌러 대화" : string.Empty);','nearStash ? "스태쉬를 눌러 보관함 열기" : nearNpc ? "대장장이를 눌러 대화" : string.Empty);');
s=s.replace('if (nearNpc && TryGetPointer(out Vector2 pointer))','if ((nearNpc || nearStash) && TryGetPointer(out Vector2 pointer))');
s=s.replace('            Bounds target = blacksmith.bounds;',`            if (HitsHudControl(pointer)) return;
            if (nearStash)
            {
                Bounds stashBounds = stash.bounds;
                stashBounds.Expand(new Vector3(0.4f, 0.4f, 0.2f));
                if (stashBounds.IntersectRay(ray))
                {
                    OpenStash().Forget();
                    return;
                }
            }
            if (!nearNpc) return;
            Bounds target = blacksmith.bounds;`);
const insert=`    private async UniTask OpenStash()
    {
        if (busy || state != LobbySceneState.EXPLORE) return;
        busy = true;
        screenGuard.SetActive(true);
        session.player.SetInputBlocked(true);
        hud.InputUISetActive(false);
        bool opened = false;
        try
        {
            await inventoryPage.Inventory_000_Base.OpenStorage();
            opened = true;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            saveError = "스태쉬를 열지 못했습니다. 다시 시도해 주세요.";
        }
        finally
        {
            if (this != null) { screenGuard.SetActive(false); busy = false; }
        }
        if (this == null) return;
        if (opened) await ChangeState((int)LobbySceneState.STASH);
        else { session.player.SetInputBlocked(false); hud.InputUISetActive(true); }
    }

`;
s=s.replace('    public async UniTask CloseInventory()',insert+'    public async UniTask CloseInventory()');
s=s.replace('if (busy || (state != LobbySceneState.INVENTORY && state != LobbySceneState.UPGRADE)) return;',`if (busy || inventoryPage.Inventory_000_Base.IsMovingItem ||
            (state != LobbySceneState.INVENTORY && state != LobbySceneState.UPGRADE && state != LobbySceneState.STASH)) return;`);
s=s.replace('            SaveProfile();','            if (!SaveProfile())\n            {\n                popup.setPopUpCode(true, "저장하지 못했습니다. 다시 시도해 주세요.", "확인");\n                return;\n            }');
s=s.replace('                case LobbySceneState.INVENTORY:\n                case LobbySceneState.UPGRADE:', '                case LobbySceneState.STASH:\n                case LobbySceneState.INVENTORY:\n                case LobbySceneState.UPGRADE:');
return s;
});
edit('Assets/@Scripts/001_Scene/002_Ingame/Pages/Ingame_002_Inventory.cs',s=>{
s=s.replace('    private IngameSceneManager IngameSceneManager;', '    private IngameSceneManager IngameSceneManager;\n    public Func<UniTask> CloseRequested { get; set; }\n    private IngameSessionManager subscribedSession;');
s=s.replace('if (stateBaseSceneManager is LobbySceneManager lobby)\n            {\n                await lobby.CloseInventory();', 'if (Inventory_000_Base.IsMovingItem) return;\n            if (CloseRequested != null)\n            {\n                await CloseRequested();');
s=s.replace('        IngameSessionManager.Instance.playerStatsChanged += Inventory_000_Base.SetInventoryStatusVal;', '        subscribedSession = IngameSessionManager.Instance;\n        subscribedSession.playerStatsChanged += Inventory_000_Base.SetInventoryStatusVal;');
s=s.replace('    protected override async UniTask OnAfterFirstSetting()',`    private void OnDestroy()
    {
        if (subscribedSession != null)
            subscribedSession.playerStatsChanged -= Inventory_000_Base.SetInventoryStatusVal;
    }

    protected override async UniTask OnAfterFirstSetting()`);
return s;
});
edit('Assets/@Scripts/001_Scene/002_Ingame/Pages/Ingame_003_Option.cs',s=>{
s=s.replace('    private IngameSceneManager IngameSceneManager;', '    private IngameSceneManager IngameSceneManager;\n    public Func<UniTask> CloseRequested { get; set; }\n    public Func<UniTask> QuitRequested { get; set; }');
s=s.replace('if (stateBaseSceneManager is LobbySceneManager lobby)\n                await lobby.ChangeState((int)LobbySceneState.EXPLORE);','if (CloseRequested != null)\n                await CloseRequested();');
s=s.replace('if (stateBaseSceneManager is LobbySceneManager lobby)\n                await lobby.ChangeState((int)LobbySceneState.GO_TO_TITLE);','if (QuitRequested != null)\n                await QuitRequested();');
return s;
});
