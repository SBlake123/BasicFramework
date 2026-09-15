using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns one raid session. It receives meaningful player changes and creates
/// checkpoints; it deliberately does not own player movement or combat logic.
/// </summary>
public partial class IngameSessionManager : MonoSingleton<IngameSessionManager>
{
    public Player player;
    public Canvas hudCanvas;
    public float checkpointIntervalSeconds = 30f;
    public VirtualJoystick virtualJoystick;
    public bool IsRaidActive { get; private set; }
    public event Action OnRaidExtracted;
    public event Action OnRaidFailed;

    public async UniTask Init()
    {
        ConnectVirtualJoystick();
        await player.Init();
    }

    public void SetPlayer(Player loadedPlayer)
    {
        player = loadedPlayer;
        ConnectVirtualJoystick();
    }

    public void SetHudCanvas(Canvas loadedHudCanvas)
    {
        hudCanvas = loadedHudCanvas;
        ConnectVirtualJoystick();
    }

    public void BeginRaid()
    {
        if (IsRaidActive)
        {
            return;
        }

        PlayerDataManager.Instance.PlayerLoad();
        IsRaidActive = true;

        ConnectVirtualJoystick();
        CapturePlayerTransform();
        CheckpointLoop().Forget();
    }

    /// <summary>
    /// Call this from future PlayerVitals when health or stamina changes.
    /// </summary>
    public void ReportPlayerVitals(float health, float stamina)
    {
        if (!IsRaidActive)
        {
            return;
        }
    }

    /// <summary>
    /// Call this from future inventory code after loot, drop, or equipment changes.
    /// </summary>

    public async UniTask CompleteExtraction()
    {
        if (!IsRaidActive)
        {
            return;
        }

        CapturePlayerTransform();
        IsRaidActive = false;
        OnRaidExtracted?.Invoke();
        await UniTask.CompletedTask;
    }

    public async UniTask CompleteDeath()
    {
        if (!IsRaidActive)
        {
            return;
        }

        CapturePlayerTransform();
        IsRaidActive = false;
        OnRaidFailed?.Invoke();
        await UniTask.CompletedTask;
    }

    private async UniTask CheckpointLoop()
    {
        while (IsRaidActive)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(checkpointIntervalSeconds), cancellationToken: destroyCancellationToken);

            if (!IsRaidActive)
            {
                break;
            }

            CapturePlayerTransform();
        }
    }

    private void CapturePlayerTransform()
    {
        if (player == null)
        {
            return;
        }

    }

    private void ConnectVirtualJoystick()
    {
        virtualJoystick.SetTarget(player);
    }
}
