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
        var saveData = PlayerDataManager.Instance.LoadFromDisk();

        if (saveData != null)
        {

            PopupManager.Instance.setPopUpCode(true, LanguageManager.Instance.GetLangScript(10014), LanguageManager.Instance.GetLangScript(10012),
                                                    LanguageManager.Instance.GetLangScript(10013));

            PopupManager.Instance.AddMethodToBtn(() =>
            {
                PlayerDataManager.Instance.DeleteData(true);
                stateBaseSceneManager.ChangeState((int)TitleSceneState.START_INGAME).Forget();
            }, async () => { await stateBaseSceneManager.ChangeState((int)TitleSceneState.MAIN); },
                async () => { await stateBaseSceneManager.ChangeState((int)TitleSceneState.MAIN); });

            //로드 후 인게임 씬에 적용.
            //GameManager -> IngameDataManager
            //

        }
        else
        {
            stateBaseSceneManager.ChangeState((int)TitleSceneState.START_INGAME).Forget();
        }
    }

    //public async UniTask NewGamePageInit()
    //{
    //    if (isFirstSetting)
    //    {
    //        isFirstSetting = false;
    //    }
    //}
}
