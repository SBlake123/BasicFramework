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
    START_INGAME,

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
        await ScreenBlurFadeOut();

        screenGuard.SetActive(false);
        await UniTask.WaitForFixedUpdate();
    }

    protected override async UniTask BackKeySetting()
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
                    Title_001_Main title_001_Main = pages[(int)TitleSceneIdx.MAIN].GetComponent<Title_001_Main>();
                    await title_001_Main.Init();
                    title_001_Main.pageMain.SetActive(true);

                    for (int i = 1; i < pages.Length; i++)
                    {
                        pages[i].pageMain.SetActive(false);
                    }

                    break;
                }

            case TitleSceneState.OPTION:
                {
                    Title_002_Option title_002_Option = pages[(int)TitleSceneIdx.OPTION].GetComponent<Title_002_Option>();
                    await title_002_Option.Init();
                    title_002_Option.pageMain.SetActive(true);
                    Debug.Log("OPTION");
                    break;
                }

            case TitleSceneState.NEW_GAME:
                {
                    Title_003_NewGame title_003_NewGame = pages[(int)TitleSceneIdx.NEW_GAME].GetComponent<Title_003_NewGame>();
                    await title_003_NewGame.Init();
                    await title_003_NewGame.NewGameStart();
                    title_003_NewGame.pageMain.SetActive(true);
                    Debug.Log("NEW_GAME");
                    break;
                }

            case TitleSceneState.CONTINUE:
                {
                    Title_004_Continue title_004_Continue = pages[(int)TitleSceneIdx.CONTINUE].GetComponent<Title_004_Continue>();
                    await title_004_Continue.Init();
                    await title_004_Continue.ContinueCheck();
                    title_004_Continue.pageMain.SetActive(true);
                    Debug.Log("CONTINUE");

                    break;
                }
            case TitleSceneState.START_INGAME:
                {
                    Debug.Log("START_INGAME");
                    await ScreenBlurFadeIn();
                    await SceneLoadManager.Instance.LoadScene(GSceneName.INGAME_SCENE);



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

    private async UniTask ScreenBlurFadeIn()
    {
        screenBlur.gameObject.SetActive(true);

        Color color = screenBlur.color;
        color.a = 0f;
        screenBlur.color = color;

        await screenBlur.DOFade(1f, 2f).AsyncWaitForCompletion();
    }

    private async UniTask ScreenBlurFadeOut()
    {
        screenBlur.gameObject.SetActive(true);

        Color color = screenBlur.color;
        color.a = 1f;
        screenBlur.color = color;

        await screenBlur.DOFade(0f, 2f).OnComplete(() =>
        {
            screenBlur.gameObject.SetActive(false);
        }).AsyncWaitForCompletion();
    }




}
