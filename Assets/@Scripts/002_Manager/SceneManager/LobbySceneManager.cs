using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LobbySceneIdx
{
    MAIN,
    GAMESTART,
    OPTION,
    EXIT
}

public enum LobbySceneState
{
    NONE
}

public class LobbyPage : MonoBehaviour
{
    public LobbySceneManager lobbySceneManager { get; set; }
}

public class LobbySceneManager : MonoSingleton<LobbySceneManager>, StateBaseSceneManager
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

    public async UniTask SceneAllocate()
    {
        foreach (var item in pages)
        {
            item.lobbySceneManager = this;
        }

        await UniTask.WaitForFixedUpdate();
    }

    public async UniTask ChangeState(int _state)
    {
        if (lobbySceneState != (LobbySceneState)_state)
        {
            lobbySceneState = (LobbySceneState)_state;

            await OnStateChange();
        }
    }

    public async UniTask OnStateChange()
    {
        screenGuard.SetActive(true);

        switch (lobbySceneState)
        {

        }
        if (screenGuard != null) screenGuard.SetActive(false);

        await UniTask.WaitForFixedUpdate();

    }

}
