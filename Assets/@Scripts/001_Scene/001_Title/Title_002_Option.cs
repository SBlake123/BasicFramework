using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Title_002_Option : StateBasePage
{
    public Button confirmBtn;
    public Button optionBackDropButton;
    public Button languageChangeBtn;

    public TextMeshProUGUI bgmTxt;
    public TextMeshProUGUI bgmValTxt;
    public TextMeshProUGUI fxTxt;
    public TextMeshProUGUI fxValTxt;
    public TextMeshProUGUI languageTitleTxt;
    public TextMeshProUGUI languageChangeBtnTxt;

    public Slider volSlider;
    public Slider fxSlider;

    private TitleSceneManager titleSceneManager;
    protected override async UniTask OnFirstSetting()
    {
        titleSceneManager = (TitleSceneManager)stateBaseSceneManager;
        optionBackDropButton.onClick.AddListener(async () => await titleSceneManager.ChangeState((int)TitleSceneState.MAIN));
        languageChangeBtn.onClick.AddListener(async () => await ChangeLanguage());

        volSlider.onValueChanged.AddListener((value) => SliderValue(value, SoundCategory.BGM));
        fxSlider.onValueChanged.AddListener((value) => SliderValue(value, SoundCategory.FX));
    }

    protected override async UniTask OnAfterFirstSetting()
    {
        if (!PlayerPrefs.HasKey("FXVOL")) PlayerPrefs.SetFloat("FXVOL", 0.5f);
        if (!PlayerPrefs.HasKey("BGMVOL")) PlayerPrefs.SetFloat("BGMVOL", 0.5f);

        bgmValTxt.text = Math.Truncate(PlayerPrefs.GetFloat("BGMVOL") * 100f).ToString();
        fxValTxt.text = Math.Truncate(PlayerPrefs.GetFloat("FXVOL") * 100f).ToString();
        volSlider.value = (float)PlayerPrefs.GetFloat("BGMVOL");
        fxSlider.value = (float)PlayerPrefs.GetFloat("FXVOL");

        await OptionTxtSetting();
    }

    private async UniTask ChangeLanguage()
    {
        LanguageType languageType = LanguageManager.Instance.GetCurrentLanguageType();

        switch (languageType)
        {
            case LanguageType.KR:
                {
                    languageType = LanguageType.EN;
                    LanguageManager.Instance.ChangeLanguageType(languageType);
                    await OptionTxtSetting();
                    break;
                }
            case LanguageType.EN:
                {
                    languageType = LanguageType.KR;
                    LanguageManager.Instance.ChangeLanguageType(languageType);
                    await OptionTxtSetting();
                    break;
                }
        }

        //언어 바꾸기
        //해당된 텍스트 언어 싹 바꾸기
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

    public void SliderValue(float value, SoundCategory category)
    {
        switch (category)
        {
            case SoundCategory.BGM:
                {
                    bgmValTxt.text = Math.Truncate(value * 100f).ToString();
                    PlayerPrefs.SetFloat("BGMVOL", value);

                    for (int i = 0; i < SoundManager.Instance.bgmSoundList.Count; i++)
                    {
                        SoundManager.Instance.bgmSoundList[i].audioSource.volume = PlayerPrefs.GetFloat("BGMVOL");
                    }
                }
                break;

            case SoundCategory.FX:
                {
                    fxValTxt.text = Math.Truncate(value * 100f).ToString();
                    PlayerPrefs.SetFloat("FXVOL", value);
                }
                break;
        }
    }

    private async UniTask OptionTxtSetting()
    {
        LanguageManager.Instance.GetLangScript(10006, LanguageManager.Instance.languageScriptDic, bgmTxt);
        LanguageManager.Instance.GetLangScript(10007, LanguageManager.Instance.languageScriptDic, fxTxt);
        LanguageManager.Instance.GetLangScript(10008, LanguageManager.Instance.languageScriptDic, languageTitleTxt);
   
        switch (LanguageManager.Instance.GetCurrentLanguageType())
        {
            case LanguageType.KR:
                {
                    LanguageManager.Instance.GetLangScript(10009, LanguageManager.Instance.languageScriptDic, languageChangeBtnTxt);
                    break;
                }
            case LanguageType.EN:
                {
                    LanguageManager.Instance.GetLangScript(10010, LanguageManager.Instance.languageScriptDic, languageChangeBtnTxt);
                    break;
                }
        }
        
    }
}
