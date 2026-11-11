using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ingame_002_Inventory : StateBasePage
{
    private IngameSceneManager IngameSceneManager;
    public Inventory_000_Base Inventory_000_Base;
    public Button inventoryBackDropButton;
    protected override UniTask OnFirstSetting()
    {
        IngameSceneManager = stateBaseSceneManager as IngameSceneManager;
        inventoryBackDropButton.onClick.AddListener(async () =>
        {
            if (stateBaseSceneManager is LobbySceneManager lobby)
            {
                await lobby.CloseInventory();
                return;
            }

            await Inventory_000_Base.InventoryClosed();
            await IngameSceneManager.ChangeState((int)IngameSceneState.INGAME);
        });

        IngameSessionManager.Instance.playerStatsChanged += Inventory_000_Base.SetInventoryStatusVal;

        return UniTask.CompletedTask;
    }

    protected override async UniTask OnAfterFirstSetting()
    {
        await Inventory_000_Base.Init();
    }
}
