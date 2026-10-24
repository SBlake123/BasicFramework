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
    public VirtualJoystick movingVirtualJoystick;
    public AimVirtualJoystick aimVirtualJoystick;

    public async UniTask Init()
    {
        ConnectVirtualJoystick();
        await player.Init();
        await PlayerSessionInit();
    }

    public async UniTask PlayerSessionInit()
    {
        PlayerDataManager.Instance.PlayerLoad();
        SetPlayerData();
        await SetPlayerEquipment();
        await SetPlayerStatus();
        SetHpAndStamina();
        IngameUIManager.Instance.SetHpSlider();
    }

    private void ConnectVirtualJoystick()
    {
        movingVirtualJoystick.SetTarget(player);
        aimVirtualJoystick.SetTarget(player);
    }
}
