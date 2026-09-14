using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ingame_003_Option : StateBasePage
{
    private IngameSceneManager IngameSceneManager;
    public Button optionBackDropButton;
    public Button quitBtn;

    public TextMeshProUGUI bgmTxt;
    public TextMeshProUGUI bgmValTxt;
    public TextMeshProUGUI fxTxt;
    public TextMeshProUGUI fxValTxt;
    public TextMeshProUGUI quitBtnTxt;

    public Slider volSlider;
    public Slider fxSlider;

    private TitleSceneManager titleSceneManager;
    protected override async UniTask OnFirstSetting()
    {
        IngameSceneManager = (IngameSceneManager)stateBaseSceneManager;
        optionBackDropButton.onClick.AddListener(async () => await IngameSceneManager.ChangeState((int)IngameSceneState.INGAME));
        quitBtn.onClick.AddListener(async () =>  await QuitBtnClicked()); 

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

        LanguageManager.Instance.GetLangScript(10006, LanguageManager.Instance.languageScriptDic, bgmTxt);
        LanguageManager.Instance.GetLangScript(10007, LanguageManager.Instance.languageScriptDic, fxTxt);
        LanguageManager.Instance.GetLangScript(10005, LanguageManager.Instance.languageScriptDic, quitBtnTxt);
    }
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

    public async UniTask QuitBtnClicked()
    {
        PopupManager.Instance.setPopUpCode(false, LanguageManager.Instance.GetLangScript(10004), LanguageManager.Instance.GetLangScript(10002));
        PopupManager.Instance.AddMethodToBtn(async () => await IngameSceneManager.ChangeState((int)IngameSceneState.GO_TO_TITLE));
    }

}
