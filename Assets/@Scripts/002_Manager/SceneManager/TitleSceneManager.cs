using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class TitleSceneManager : MonoSingleton<TitleSceneManager>
{
    //public 

    // Start is called before the first frame update
    void Start()
    {
        TitleSceneSetting().Forget();
    }

    private async UniTask TitleSceneSetting()
    {

    }


    private async UniTask BackgroundSetting()
    {
        //중앙 기준점으로 자기 길이 반 이상 나갔을 때 위치 바꾸기
    }
 
}
