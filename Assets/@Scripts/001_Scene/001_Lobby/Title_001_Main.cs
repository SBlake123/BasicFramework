using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title_001_Main : StateBasePage
{
    public Button newGameButton;
    public Button continueGameButton;

    public List<RectTransform> skyRect = new List<RectTransform>();
    public List<RectTransform> groundRect = new List<RectTransform>();

    float skyRectMoveValue = 0.3f;
    float groundRectMoveValue = -0.2f;

    protected override async UniTask OnFirstSetting()
    {
        BackgroundSetting().Forget();
        AddListenerToButton();
        Debug.Log("ON_INIT");
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

    public async UniTask Test()
    {
        await stateBaseSceneManager.ChangeState((int)TitleSceneState.NEW_GAME);
    }

    public async UniTask Test2()
    {
        await stateBaseSceneManager.ChangeState((int)TitleSceneState.CONTINUE);
    }
    public void AddListenerToButton()
    {
        newGameButton.onClick.AddListener(async () => await Test());
        continueGameButton.onClick.AddListener(async () => await Test2());
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
