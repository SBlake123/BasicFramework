using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public enum TitleSceneIdx
{
    MAIN,
    NOTICE,
    ACCOUNTLIST,
    ACCOUNTLINK,
    SIGNIN,
    LOGINPLATFORM,
    OPTION
}
public enum TitleSceneState
{
    NONE,
    LOADING,
    MAIN,
    OPTION,
    GOTOLOBBY,

    APPQUIT = 998,
    GOTOTITLE = 999
}

public partial class TitleSceneManager : StateBaseSceneManager
{
    private TitleSceneState titleSceneState = TitleSceneState.NONE;

    public TitlePage[] pages;

    public GameObject screenGuard;
    public Image screenBlur;

    public List<RectTransform> skyRect = new List<RectTransform>();
    public List<RectTransform> groundRect = new List<RectTransform>();

    float skyRectMoveValue = 0.3f;
    float groundRectMoveValue = -0.2f;

    void Start()
    {
        Debug.Log("TitleSceneManager Start");
        TitleSceneSetting().Forget();
    }

    private async UniTask TitleSceneSetting()
    {
        ScreenBlurInit();
        BackgroundSetting().Forget();
        BackKeySetting().Forget();
        SceneAllocate();
        ScreenBlurFadeOut();

        await UniTask.WaitForFixedUpdate();
    }

    private async UniTask BackKeySetting()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                switch (titleSceneState)
                {
                    default:
                        break;
                }
            }
            await UniTask.Yield(PlayerLoopTiming.Update, destroyCancellationToken);
        }
    }

    public override void SceneAllocate()
    {
        foreach (var item in pages)
        {
            item.StateBaseSceneManager = this;
        }
    }

    public override async UniTask ChangeState(int state)
    {
        if (titleSceneState != (TitleSceneState)state)
        {
            titleSceneState = (TitleSceneState)state;

            await OnStateChange();
        }
    }

    public override async UniTask OnStateChange()
    {
        screenGuard.SetActive(true);

        switch (titleSceneState)
        {

        }
        if (screenGuard != null) screenGuard.SetActive(false);

        await UniTask.WaitForFixedUpdate();
    }

    private void ScreenBlurInit()
    {
        screenBlur.gameObject.SetActive(true);
        Color color = screenBlur.color;
        color.a = 1f;
        screenBlur.color = color;
    }

    private void ScreenBlurFadeOut()
    {
        screenBlur.DOFade(0f, 2f).OnComplete(() =>
        {
            screenBlur.gameObject.SetActive(false);
        });
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
                for(int i = 0; i < groundRect.Count; i++)
                {
                    groundRect[i].anchoredPosition = groundRect[i].anchoredPosition + groundRectMoveVec;
                }

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, destroyCancellationToken);


                //await UniTask.Delay(1000);
            }
        }
    }


}
