using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateBasePage : MonoBehaviour
{
    public StateBaseSceneManager stateBaseSceneManager { get; set; }

    public GameObject pageMain;

    private bool isFirstSetting = true;

    public async UniTask Init()
    {
        if (isFirstSetting)
        {
            await OnFirstSetting();
        }
        await OnAfterFirstSetting();

        isFirstSetting = false;
    }

    //asyne
    protected abstract UniTask OnFirstSetting();

    protected abstract UniTask OnAfterFirstSetting();
}
