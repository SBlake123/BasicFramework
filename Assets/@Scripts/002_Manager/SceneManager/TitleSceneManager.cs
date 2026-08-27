using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class TitleSceneManager : MonoSingleton<TitleSceneManager>
{
    public List<RectTransform> skyRect = new List<RectTransform>();
    public List<RectTransform> groundRect = new List<RectTransform>();

    float skyRectMoveValue = 0.3f;
    float groundRectMoveValue = -0.2f; 

    void Start()
    {
        Debug.Log("TitleSceneManager Start");
        TitleSceneSetting();
    }

    private void TitleSceneSetting()
    {
        BackgroundSetting().Forget();
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

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

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

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);


                //await UniTask.Delay(1000);
            }
        }
    }
 
}
