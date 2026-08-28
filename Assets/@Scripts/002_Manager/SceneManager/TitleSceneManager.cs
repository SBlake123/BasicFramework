using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TitleSceneManager : MonoSingleton<TitleSceneManager>
{
    public List<RectTransform> skyRect = new List<RectTransform>();
    public List<RectTransform> groundRect = new List<RectTransform>();

    float skyRectMoveValue = 0.3f;
    float groundRectMoveValue = -0.2f;

    public Image screenBlur;

    void Start()
    {
        Debug.Log("TitleSceneManager Start");
        TitleSceneSetting();
    }

    private void TitleSceneSetting()
    {
        ScreenBlurInit();
        BackgroundSetting().Forget();
        ScreenBlurFadeOut();
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
