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
    public VirtualJoystick movingVirtualJoystick;
    public AimVirtualJoystick aimVirtualJoystick;

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

    public async UniTask BeginRaid()
    {
        if (IsRaidActive)
        {
            return;
        }

        PlayerDataManager.Instance.PlayerLoad();
        SetPlayerData();
        SetDummyData();
        await SetPlayerEquipment();
        await SetPlayerStatus();
        IsRaidActive = true;

        ConnectVirtualJoystick();
    }

    private void ConnectVirtualJoystick()
    {
        movingVirtualJoystick.SetTarget(player);
        aimVirtualJoystick.SetTarget(player);
    }
}
