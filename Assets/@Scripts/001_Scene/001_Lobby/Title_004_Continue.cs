using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title_004_Continue : StateBasePage
{
    protected override async UniTask OnFirstSetting()
    {

    }

    public async UniTask ContinueCheck()
    {
        var saveData = SaveLoadManager.Instance.Load();

        if (saveData != null)
        {

        }
    }
    //public async UniTask ContinuePageInit()
    //{
    //    if (isFirstSetting)
    //    {
    //        isFirstSetting = false;
    //    }
    //}


}
