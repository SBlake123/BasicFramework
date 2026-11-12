using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ingame_002_Inventory : StateBasePage
{
    private IngameSceneManager IngameSceneManager;
    public Func<UniTask> CloseRequested { get; set; }
    private IngameSessionManager subscribedSession;
    public Inventory_000_Base Inventory_000_Base;
    public Button inventoryBackDropButton;
    protected override UniTask OnFirstSetting()
    {
        IngameSceneManager = stateBaseSceneManager as IngameSceneManager;
        inventoryBackDropButton.onClick.AddListener(async () =>
        {
            if (Inventory_000_Base.IsMovingItem) return;
            if (CloseRequested != null)
            {
                await CloseRequested();
                return;
            }

            await Inventory_000_Base.InventoryClosed();
            await IngameSceneManager.ChangeState((int)IngameSceneState.INGAME);
        });

        subscribedSession = IngameSessionManager.Instance;
        subscribedSession.playerStatsChanged += Inventory_000_Base.SetInventoryStatusVal;

        return UniTask.CompletedTask;
    }

    private void OnDestroy()
    {
        if (subscribedSession != null)
            subscribedSession.playerStatsChanged -= Inventory_000_Base.SetInventoryStatusVal;
    }

    protected override async UniTask OnAfterFirstSetting()
    {
        await Inventory_000_Base.Init();
    }
}
