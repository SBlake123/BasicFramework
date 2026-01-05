using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LobbySceneIdx
{
    NEWMAIN,
    STAGESELECT, //or NEWSTAGESELECT
    ATTENDENCE,
    POSTMAIN,
    SHOP,
    BUYHP,
    OPTION,
    SOCIAL,
    NOTICE
}

public enum LobbySceneState
{
    NONE
}

public class LobbyPage : MonoBehaviour
{
    public LobbySceneManager lobbySceneManager { get; set; }
}

public class LobbySceneManager : StateBaseScene
{
    private LobbySceneState lobbySceneState = LobbySceneState.NONE;

    public GameObject screenGuard;

    public LobbyPage[] pages;

    private async UniTask BackKeySetting()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                switch (lobbySceneState)
                {
                    default:
                        break;
                }
            }
            await UniTask.Delay(TimeSpan.FromSeconds(0.02d));
        }
    }
    //로비 구성
    public override async UniTask ChangeState(int _state)
    {
        throw new System.NotImplementedException();
    }

    public override async UniTask OnStateChange()
    {
        screenGuard.SetActive(true);

        switch (lobbySceneState)
        {

        }
        if (screenGuard != null) screenGuard.SetActive(false);
    }

    public override async UniTask SceneAllocate()
    {
        foreach (var item in pages)
        {
            item.lobbySceneManager = this;
        }

        await UniTask.WaitForFixedUpdate();
    }
}
