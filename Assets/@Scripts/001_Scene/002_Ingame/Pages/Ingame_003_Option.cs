using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ingame_003_Option : StateBasePage
{
    private IngameSceneManager IngameSceneManager;
    public Button OptionBackDropButton;
    protected override async UniTask OnFirstSetting()
    {
        IngameSceneManager = (IngameSceneManager)stateBaseSceneManager;
        OptionBackDropButton.onClick.AddListener(async () => await IngameSceneManager.ChangeState((int)IngameSceneState.INGAME));
    }

    //오ㅂ션창 열었다 닫았다

}
