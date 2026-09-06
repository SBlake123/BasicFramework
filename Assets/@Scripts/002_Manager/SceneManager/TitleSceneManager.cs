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
    OPTION,
    NEW_GAME,
    CONTINUE
}
public enum TitleSceneState
{
    NONE,
    LOADING,
    MAIN,
    OPTION,
    NEW_GAME,
    CONTINUE,

    APPQUIT = 998,
    GOTOTITLE = 999
}

public partial class TitleSceneManager : StateBaseSceneManager
{
    private TitleSceneState titleSceneState = TitleSceneState.NONE;

    public StateBasePage[] pages;

    public GameObject screenGuard;
    public Image screenBlur;

    void Start()
    {
        Debug.Log("TitleSceneManager Start");
        TitleSceneSetting().Forget();
    }

    private async UniTask TitleSceneSetting()
    {
        screenGuard.SetActive(true);

        ScreenBlurInit();
        BackKeySetting().Forget();
        SceneAllocate();
        await ChangeState((int)TitleSceneState.MAIN);
        ScreenBlurFadeOut();

        screenGuard.SetActive(false);
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
            item.stateBaseSceneManager = this;
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
            case TitleSceneState.MAIN:
                {
                    Title_001_Main title_001_main = pages[(int)TitleSceneIdx.MAIN].GetComponent<Title_001_Main>();
                    await title_001_main.MainInit();

                    break;
                }
            case TitleSceneState.OPTION:
                {
                    Debug.Log("OPTION");
                    break;
                }
            case TitleSceneState.NEW_GAME:
                {
                    Debug.Log("NEW_GAME");
                    break;
                }
            case TitleSceneState.CONTINUE:
                {
                    Debug.Log("CONTINUE");

                    break;
                }
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




}
