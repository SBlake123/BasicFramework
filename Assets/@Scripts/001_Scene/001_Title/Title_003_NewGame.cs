using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title_003_NewGame : StateBasePage
{
    protected override async UniTask OnFirstSetting()
    {

    }

    protected override async UniTask OnAfterFirstSetting()
    {

    }
    public async UniTask NewGameStart()
    {
        stateBaseSceneManager.ChangeState((int)TitleSceneState.START_INGAME).Forget();
    }

    //public async UniTask NewGamePageInit()
    //{
    //    if (isFirstSetting)
    //    {
    //        isFirstSetting = false;
    //    }
    //}
}
