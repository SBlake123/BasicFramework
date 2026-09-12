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

    public Button confirmBtn;

    public Slider volSlider;
    public Slider fxSlider;
    protected override async UniTask OnFirstSetting()
    {
        IngameSceneManager = (IngameSceneManager)stateBaseSceneManager;
        inventoryBackDropButton.onClick.AddListener(async () => await IngameSceneManager.ChangeState((int)IngameSceneState.INGAME));
    }

    protected override async UniTask OnAfterFirstSetting()
    {

    }
}
