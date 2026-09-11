using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public enum IngameSceneState
{
    NONE,
    READY,
    PLAYING,
    PAUSED,
    EXTRACTING,
    DEAD,
    RESULT
}

/// <summary>
/// Owns the high-level state of one in-game scene.
/// UI pages, fades, and result screens can subscribe to OnStateChanged later.
/// </summary>
public class IngameSceneManager : StateBaseSceneManager
{
    public IngameSessionManager ingameSessionManager;

    public IngameSceneState CurrentState { get; private set; } = IngameSceneState.NONE;
    public event Action<IngameSceneState> OnStateChanged;

    private void Start()
    {
        InitializeScene().Forget();
    }

    private async UniTask InitializeScene()
    {
        if (ingameSessionManager == null)
        {
            Debug.LogError("[IngameSceneManager] IngameSessionManager reference is not assigned.");
            return;
        }

        await ChangeState((int)IngameSceneState.READY);
        await ChangeState((int)IngameSceneState.PLAYING);
    }

    public override void SceneAllocate()
    {
        // Keep scene-wide references here when in-game pages are added.
    }

    public override async UniTask ChangeState(int state)
    {
        IngameSceneState nextState = (IngameSceneState)state;

        if (CurrentState == nextState)
        {
            return;
        }

        CurrentState = nextState;
        await OnStateChange();
    }

    public override async UniTask OnStateChange()
    {
        switch (CurrentState)
        {
            case IngameSceneState.PLAYING:
                ingameSessionManager?.BeginRaid();
                break;

            case IngameSceneState.EXTRACTING:
                if (ingameSessionManager != null)
                {
                    await ingameSessionManager.CompleteExtraction();
                }
                await ChangeState((int)IngameSceneState.RESULT);
                return;

            case IngameSceneState.DEAD:
                if (ingameSessionManager != null)
                {
                    await ingameSessionManager.CompleteDeath();
                }
                await ChangeState((int)IngameSceneState.RESULT);
                return;
        }

        OnStateChanged?.Invoke(CurrentState);
    }

    public void RequestExtraction()
    {
        ChangeState((int)IngameSceneState.EXTRACTING).Forget();
    }

    public void NotifyPlayerDied()
    {
        ChangeState((int)IngameSceneState.DEAD).Forget();
    }
}
