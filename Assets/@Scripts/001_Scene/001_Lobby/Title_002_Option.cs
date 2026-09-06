using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Title_002_Option : StateBasePage
{
    public Button confirmBtn;
    public Button cancelBtn;

    public Slider volSlider;
    public Slider fxSlider;

    public TMP_Dropdown languageDropdown;

    protected override async UniTask OnFirstSetting()
    {

    }

    //public async UniTask OptionInit()
    //{
    //    if (isFirstSetting)
    //    {
    //        // 사운드 크기
    //        // 언어 변경
    //        // 

    //        isFirstSetting = false;
    //    }
    //}
}
