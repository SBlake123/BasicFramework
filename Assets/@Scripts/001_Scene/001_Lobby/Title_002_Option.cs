using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title_002_Option : StateBasePage
{

    private bool isFirstSetting = true;

    public Slider volSlider;
    public Slider fxSlider;
    public async UniTask OptionInit()
    {
        if (isFirstSetting)
        {
            // 사운드 크기
            // 언어 변경
            // 

            isFirstSetting = false;
        }
    }

}
