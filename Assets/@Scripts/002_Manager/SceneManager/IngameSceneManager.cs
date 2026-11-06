using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum IngameSceneState
{
    NONE,
    LOADING,
    INGAME,
    INVENTORY,
    LOOTING,
    OPTION,
    DEAD,
    EXTRACTING,
    RESULT,
    GO_TO_TITLE
}

public enum IngameSceneIdx
{
    INGAME,
    INVENTORY,
    OPTION
}

[Serializable]
public class MonsterSpawnEntry
{
    public int monsterId { get; set; }
    public int value { get; set; }
}

[Serializable]
public class MonsterSpawnData
{
    public bool useWeight { get; set; }
    public int totalSpawnCount { get; set; }
    public List<MonsterSpawnEntry> monsters { get; set; } = new List<MonsterSpawnEntry>();
}

public class IngameSceneManager : StateBaseSceneManager
{
    public IngameSceneState ingameSceneState { get; private set; } = IngameSceneState.NONE;

    public StateBasePage[] pages;
    public List<MonsterSpawnData> monsterSpawnDataList { get; set; } = new List<MonsterSpawnData>();
    public List<MonsterSpawnArea> monsterSpawnAreaList = new List<MonsterSpawnArea>();
    public List<Transform> itemBoxAreaList = new List<Transform>();
    public List<Portal> portalList = new List<Portal>();

    public GameObject screenGuard;
    public Image screenBlur;

    public event Action InventoryClosed;

    private void Start()
    {
        ChangeState((int)IngameSceneState.LOADING).Forget();
    }
    private async UniTask InitializeScene()
    {
        screenGuard.SetActive(true);
        SoundManager.Instance.StopSound("001_Title", SoundCategory.BGM).Forget();
        ScreenBlurInit();
        BackKeySetting().Forget();
        SceneAllocate();
        SubscribingEvent();
        SetDummyDataInSpawnData();
        await SetDummyDataInItemBoxAreaList();
        await SpawnMonsters(monsterSpawnAreaList, monsterSpawnDataList);
        await IngameUIManager.Instance.Init();
        await IngameSessionManager.Instance.Init();
        await ChangeState((int)IngameSceneState.INGAME);
        await ScreenBlurFadeOut();

        screenGuard.SetActive(false);
    }

    protected override async UniTask BackKeySetting()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                switch (ingameSceneState)
                {
                    default:
                        break;
                }
            }
            await UniTask.Yield(PlayerLoopTiming.Update, destroyCancellationToken);
        }
    }

    protected void SubscribingEvent()
    {
        IngameUIManager.Instance.InventoryRequested += async () => await ChangeState((int)IngameSceneState.INVENTORY);
        IngameUIManager.Instance.OptionRequested += async () => await ChangeState((int)IngameSceneState.OPTION);
        IngameSessionManager.Instance.player.PlayerDied += async () => await ChangeState((int)IngameSceneState.DEAD);
        ItemLootingManager.Instance.LootingBoxOpenRequested += async () => await ChangeState((int)IngameSceneState.LOOTING);
        //ingameUIManager.AttackRequested += async () => await ChangeState((int)IngameSceneState.INVENTORY);

        foreach (var item in portalList)
        {
            item.EscapeSuccess += async () => await ChangeState((int)IngameSceneState.EXTRACTING);
            item.Init();
        }
    }
    #region DUMMY
    void SetDummyDataInSpawnData()
    {
        monsterSpawnDataList = new List<MonsterSpawnData>
    {
        new MonsterSpawnData
        {
            useWeight = false,
            monsters = new List<MonsterSpawnEntry>
            {
                new MonsterSpawnEntry
                {
                    monsterId = 30001,
                    value = 1
                }
            }
        },
        new MonsterSpawnData
        {
            useWeight = false,
            monsters = new List<MonsterSpawnEntry>
            {
                new MonsterSpawnEntry
                {
                    monsterId = 30001,
                    value = 2
                }
            }
        },
        new MonsterSpawnData
        {
            useWeight = true,
            totalSpawnCount = 3,
            monsters = new List<MonsterSpawnEntry>
            {
                new MonsterSpawnEntry
                {
                    monsterId = 30001,
                    value = 100
                }
            }
        },
        new MonsterSpawnData
        {
            useWeight = false,
            totalSpawnCount = 3,
            monsters = new List<MonsterSpawnEntry>
            {
                new MonsterSpawnEntry
                {
                    monsterId = 30001,
                    value = 2
                },

                new MonsterSpawnEntry
                {
                    monsterId = 30002,
                    value = 1
                }
            }
        }
    };
    }

    async UniTask SetDummyDataInItemBoxAreaList()
    {
        List<ItemData> itemData1 = new()
        {
            ItemLootingManager.Instance.CreateStashItem(1001, 0, ItemRarity.UNCOMMON),
            ItemLootingManager.Instance.CreateStashItem(2001, 1, ItemRarity.RARE),
        };

        List<ItemData> itemData2 = new()
        {
            ItemLootingManager.Instance.CreateStashItem(5000, 0, ItemRarity.UNIQUE),
            ItemLootingManager.Instance.CreateStashItem(7000, 1, ItemRarity.UNIQUE)
        };

        await ItemLootingManager.Instance.MakingItemLootingBox("Box_Common", itemData1, itemBoxAreaList[0]);
        await ItemLootingManager.Instance.MakingItemLootingBox("Box_Unique", itemData2, itemBoxAreaList[1]);

    }

    async UniTask SpawnMonsters(List<MonsterSpawnArea> monsterSpawnAreaList, List<MonsterSpawnData> monsterSpawnDataList)
    {
        //spawnArea List 순서를 알기 때문에 문서로 이제 ㅋ.ㅋ.ㅋ
        //랜덤 배치할 것.

        for (int i = 0; i < monsterSpawnDataList.Count; i++)
        {
            if (monsterSpawnDataList[i].useWeight)
            {
                for (int j = 0; j < monsterSpawnDataList[i].totalSpawnCount; j++)
                {
                    if (!monsterSpawnAreaList[i].TryGetSpawnPosition(out Vector3 spawnPosition)) continue;

                    MonsterSpawnEntry monster = SelectMonsterByWeight(monsterSpawnDataList[i]);

                    string monsterName = DataManager.Instance.GetData(monster.monsterId, DataManager.Instance.monsterDataDic).prefabName;

                    GameObject obj = await ObjectPool.Instance.PopFromPool((monsterName), monsterSpawnAreaList[i].spawnAreaTrf);
                    obj.transform.position = spawnPosition;

                    Monster_000_Base monsterBase = obj.GetComponent<Monster_000_Base>();

                    monsterBase.SettingMonsterData(monster.monsterId);
                    monsterBase.Init().Forget();
                }
            }
            else
            {
                foreach (MonsterSpawnEntry monster in monsterSpawnDataList[i].monsters)
                {
                    for (int j = 0; j < monster.value; j++)
                    {
                        if (!monsterSpawnAreaList[i].TryGetSpawnPosition(out Vector3 spawnPosition)) continue;

                        string monsterName = DataManager.Instance.GetData(monster.monsterId, DataManager.Instance.monsterDataDic).prefabName;

                        GameObject obj = await ObjectPool.Instance.PopFromPool((monsterName), monsterSpawnAreaList[i].spawnAreaTrf);
                        obj.transform.position = spawnPosition;

                        Monster_000_Base monsterBase = obj.GetComponent<Monster_000_Base>();

                        monsterBase.SettingMonsterData(monster.monsterId);
                        monsterBase.Init().Forget();
                    }
                }
            }
        }
    }

    private MonsterSpawnEntry SelectMonsterByWeight(MonsterSpawnData monsterSpawnData)
    {
        int totalWeight = 0;

        foreach (MonsterSpawnEntry monster in monsterSpawnData.monsters)
        {
            if (monster.value <= 0)
                continue;

            totalWeight += monster.value;
        }

        if (totalWeight == 0)
            return null;

        int randomValue = UnityEngine.Random.Range(0, totalWeight);

        int accumulatedWeight = 0;

        foreach (MonsterSpawnEntry monster in monsterSpawnData.monsters)
        {
            if (monster.value <= 0)
                continue;

            accumulatedWeight += monster.value;

            if (randomValue < accumulatedWeight)
                return monster;
        }

        return null;
    }
    #endregion

    public override void SceneAllocate()
    {
        foreach (var item in pages)
        {
            item.stateBaseSceneManager = this;
        }
    }

    public override async UniTask ChangeState(int state)
    {
        IngameSceneState nextState = (IngameSceneState)state;

        if (ingameSceneState == nextState)
        {
            return;
        }

        ingameSceneState = nextState;
        await OnStateChange();
    }

    public override async UniTask OnStateChange()
    {
        screenGuard.SetActive(true);

        Debug.Log($"NowState : {ingameSceneState}");

        switch (ingameSceneState)
        {
            case IngameSceneState.NONE:

                break;

            case IngameSceneState.LOADING:
                await InitializeScene();

                break;

            case IngameSceneState.INGAME:
                {
                    GameManager.Instance.SetGameplayPaused(false, (Action)null);
                    SoundManager.Instance.UnPauseSound(SoundCategory.FX);
                    IngameUIManager.Instance.InputUISetActive(true);

                    for (int i = 1; i < pages.Length; i++)
                    {
                        pages[i].pageMain.SetActive(false);
                    }
                }

                break;

            case IngameSceneState.INVENTORY:
                {
                    Ingame_002_Inventory ingame_002_Inventory = pages[(int)IngameSceneIdx.INVENTORY].GetComponent<Ingame_002_Inventory>();
                    await ingame_002_Inventory.Init();
                    IngameUIManager.Instance.JoyStickUISetActive(false);
                    ingame_002_Inventory.pageMain.SetActive(true);
                }

                break;

            case IngameSceneState.LOOTING:
                {
                    Ingame_002_Inventory ingame_002_Inventory = pages[(int)IngameSceneIdx.INVENTORY].GetComponent<Ingame_002_Inventory>();
                    await ingame_002_Inventory.Init();
                    await ingame_002_Inventory.Inventory_000_Base.LootingBoxItemSetting();

                    //looting Setting
                    IngameUIManager.Instance.JoyStickUISetActive(false);
                    ingame_002_Inventory.pageMain.SetActive(true);
                }

                break;

            case IngameSceneState.OPTION:
                {
                    GameManager.Instance.SetGameplayPaused(true, (Action)null);
                    SoundManager.Instance.PauseSound(SoundCategory.FX);
                    Ingame_003_Option ingame_003_Option = pages[(int)IngameSceneIdx.OPTION].GetComponent<Ingame_003_Option>();
                    await ingame_003_Option.Init();
                    ingame_003_Option.pageMain.SetActive(true);
                }

                break;


            case IngameSceneState.EXTRACTING:
                {
                    IngameUIManager.Instance.JoyStickUISetActive(false);
                    Ingame_002_Inventory ingame_002_Inventory = pages[(int)IngameSceneIdx.INVENTORY].GetComponent<Ingame_002_Inventory>();
                    PlayerDataManager.Instance.SaveToDisk(ingame_002_Inventory.Inventory_000_Base.ExtractingInvenItems());
                    PopupManager.Instance.setPopUpCode(false, LanguageManager.Instance.GetLangScript(10015), LanguageManager.Instance.GetLangScript(10002));
                    PopupManager.Instance.AddMethodToBtn(async () =>
                    {
                        await ChangeState((int)IngameSceneState.RESULT);
                    },
                    null,
                    async () =>
                    {
                        await ChangeState((int)IngameSceneState.RESULT);
                    });
                }

                break;

            case IngameSceneState.DEAD:
                {
                    PlayerDataManager.Instance.DeleteData(false);

                    PopupManager.Instance.setPopUpCode(false, LanguageManager.Instance.GetLangScript(10004), LanguageManager.Instance.GetLangScript(10002));
                    PopupManager.Instance.AddMethodToBtn(async () =>
                    {
                        await ChangeState((int)IngameSceneState.RESULT);
                    },
                    null,
                    async () =>
                    {
                        await ChangeState((int)IngameSceneState.RESULT);
                    });
                }
                break;

            case IngameSceneState.RESULT:
                {
                    await ChangeState((int)IngameSceneState.GO_TO_TITLE);
                }
                return;

            case IngameSceneState.GO_TO_TITLE:

                GameManager.Instance.SetGameplayPaused(false, (Action)null);
                await SceneLoadManager.Instance.LoadScene(GSceneName.TITLE_SCENE);
                return;
        }



        screenGuard.SetActive(false);
    }

    private void ScreenBlurInit()
    {
        screenBlur.gameObject.SetActive(true);
        Color color = screenBlur.color;
        color.a = 1f;
        screenBlur.color = color;
    }

    private async UniTask ScreenBlurFadeIn()
    {
        screenBlur.gameObject.SetActive(true);

        Color color = screenBlur.color;
        color.a = 0f;
        screenBlur.color = color;

        await screenBlur.DOFade(1f, 1f).AsyncWaitForCompletion();
    }

    private async UniTask ScreenBlurFadeOut()
    {
        screenBlur.gameObject.SetActive(true);

        Color color = screenBlur.color;
        color.a = 1f;
        screenBlur.color = color;

        await screenBlur.DOFade(0f, 1f).OnComplete(() =>
        {
            screenBlur.gameObject.SetActive(false);
        }).AsyncWaitForCompletion();
    }
}
