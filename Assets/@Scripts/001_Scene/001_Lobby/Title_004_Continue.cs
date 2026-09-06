using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title_004_Continue : StateBasePage
{
    private bool isFirstSetting = true;
    public async UniTask ContinuePageInit()
    {
        if (isFirstSetting)
        {
            isFirstSetting = false;
        }
    }
}
