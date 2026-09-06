using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title_003_NewGame : StateBasePage
{

    private bool isFirstSetting = true;
    public async UniTask NewGamePageInit()
    {
        if (isFirstSetting)
        {
            isFirstSetting = false;
        }
    }
}
