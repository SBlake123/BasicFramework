using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Title_001_Main : StateBasePage
{
    public Button newGameButton;
    public Button continueGameButton;
    public Button optionButton;
    public TextMeshProUGUI newGameBtnTxt;
    public TextMeshProUGUI continueBtnTxt;


    public List<RectTransform> skyRect = new List<RectTransform>();
    public List<RectTransform> groundRect = new List<RectTransform>();

    float skyRectMoveValue = 0.3f;
    float groundRectMoveValue = -0.2f;

    protected override async UniTask OnFirstSetting()
    {
        BackgroundSetting().Forget();
        CanContinueCheck();
        AddListenerToButton();
        Debug.Log("ON_INIT");
    }

    protected override async UniTask OnAfterFirstSetting()
    {
        LanguageManager.Instance.GetLangScript(10000, LanguageManager.Instance.languageScriptDic, newGameBtnTxt);
        LanguageManager.Instance.GetLangScript(10001, LanguageManager.Instance.languageScriptDic, continueBtnTxt);
    }
    //public async UniTask MainInit()
    //{
    //    if (isFirstSetting)
    //    {
    //        BackgroundSetting().Forget();
    //        AddListenerToButton();
    //        isFirstSetting = false;
    //    }
    //}

    public void CanContinueCheck()
    {
        var saveData = SaveLoadManager.Instance.Load();

        if (saveData == null)
        {
            //로드 후 인게임 씬에 적용.
            //GameManager -> IngameDataManager
            //
            continueGameButton.gameObject.SetActive(false);
        }
    }

    public void NewGameStart()
    {
        stateBaseSceneManager.ChangeState((int)TitleSceneState.NEW_GAME).Forget();
    }

    public void ContinueStart()
    {
        stateBaseSceneManager.ChangeState((int)TitleSceneState.CONTINUE).Forget();
    }

    public void OptionStart()
    {
        stateBaseSceneManager.ChangeState((int)TitleSceneState.OPTION).Forget();
    }

    public void AddListenerToButton()
    {
        newGameButton.onClick.AddListener(() => NewGameStart());
        continueGameButton.onClick.AddListener(() => ContinueStart());
        optionButton.onClick.AddListener(() => OptionStart());
    }

    private async UniTask BackgroundSetting()
    {
        SkyRectPlay().Forget();
        //GroundRectPlay().Forget();

        async UniTask SkyRectPlay()
        {
            Vector2 skyRectMoveVec = new Vector2(skyRectMoveValue, 0);

            while (true)
            {
                for (int i = 0; i < skyRect.Count; i++)
                {
                    skyRect[i].anchoredPosition = skyRect[i].anchoredPosition + skyRectMoveVec;
                }

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, destroyCancellationToken);

                //await UniTask.Delay(1000);
            }

        }

        async UniTask GroundRectPlay()
        {
            Vector2 groundRectMoveVec = new Vector2(groundRectMoveValue, 0);

            while (true)
            {
                for (int i = 0; i < groundRect.Count; i++)
                {
                    groundRect[i].anchoredPosition = groundRect[i].anchoredPosition + groundRectMoveVec;
                }

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, destroyCancellationToken);


                //await UniTask.Delay(1000);
            }
        }
    }
}
