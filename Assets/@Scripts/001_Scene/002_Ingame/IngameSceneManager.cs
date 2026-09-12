using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

public enum IngameSceneState
{
    NONE,
    LOADING,
    INGAME,
    INVENTORY,
    OPTION,
    DEAD,
    EXTRACTING,
    RESULT
}

public enum IngameSceneIdx
{
    INGAME,
    INVENTORY,
    OPTION
}


public class IngameSceneManager : StateBaseSceneManager
{
    private IngameSceneState ingameSceneState = IngameSceneState.NONE;

    public StateBasePage[] pages;

    public GameObject screenGuard;
    public Image screenBlur;

    public IngameUIManager ingameUIManager;

    public event Action PlayerDied;
    public event Action InventoryClosed;

    public IngameSceneState CurrentState { get; private set; } = IngameSceneState.NONE;

    private void Start()
    {
        InitializeScene().Forget();
        //InventoryClosed += ingameUIManager.
    }

    private async UniTask InitializeScene()
    {
        screenGuard.SetActive(true);

        BackKeySetting().Forget();
        SceneAllocate();
        SubscribingEvent();
        await ingameUIManager.Init();
        await ChangeState((int)IngameSceneState.LOADING);
        await ChangeState((int)IngameSceneState.INGAME);

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
        ingameUIManager.InventoryRequested += async () => await ChangeState((int)IngameSceneState.INVENTORY);
        //ingameUIManager.AttackRequested += async () => await ChangeState((int)IngameSceneState.INVENTORY);
    }

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

        if (CurrentState == nextState)
        {
            return;
        }

        CurrentState = nextState;
        await OnStateChange();
    }

    public override async UniTask OnStateChange()
    {
        screenGuard.SetActive(true);

        Debug.Log($"NowState : {CurrentState}");

        switch (CurrentState)
        {
            case IngameSceneState.NONE:
                IngameSessionManager.Instance.BeginRaid();
                break;

            case IngameSceneState.LOADING:
                IngameSessionManager.Instance.BeginRaid();
                break;

            case IngameSceneState.INGAME:
                {


                    //그냥 게임창 상태로 복귀,
                    //그거는 켜야 됨 조이스틱
                    ingameUIManager.InputUISetActive(true);


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
                    ingameUIManager.JoyStickUISetActive(false);
                    ingame_002_Inventory.pageMain.SetActive(true);
                }

                break;

            case IngameSceneState.OPTION:
                {
                    Ingame_003_Option ingame_003_Option = pages[(int)IngameSceneIdx.OPTION].GetComponent<Ingame_003_Option>();
                    await ingame_003_Option.Init();
                    ingame_003_Option.pageMain.SetActive(true);
                }

                break;


            case IngameSceneState.EXTRACTING:

                await IngameSessionManager.Instance.CompleteExtraction();

                await ChangeState((int)IngameSceneState.RESULT);
                return;

            case IngameSceneState.DEAD:

                await IngameSessionManager.Instance.CompleteExtraction();

                await ChangeState((int)IngameSceneState.RESULT);
                return;
        }

        screenGuard.SetActive(false);
    }

    public void RequestExtraction()
    {
        ChangeState((int)IngameSceneState.EXTRACTING).Forget();
    }

    public void NotifyPlayerDied()
    {
        ChangeState((int)IngameSceneState.DEAD).Forget();
    }
}
