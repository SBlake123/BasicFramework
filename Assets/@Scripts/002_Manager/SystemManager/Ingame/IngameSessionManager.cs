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
        RestoreStamina().Forget();
        IngameUIManager.Instance.SetHpSlider();
    }

    private void ConnectVirtualJoystick()
    {
        movingVirtualJoystick.SetTarget(player);
        aimVirtualJoystick.SetTarget(player);
    }

    private bool isRestoringStamina;

    public async UniTask RestoreStamina()
    {
        if (isRestoringStamina) return;
        isRestoringStamina = true;

        const float recoveryPerSecond = 20f;
        var token = destroyCancellationToken;

        try
        {
            while (!player.isDead)
            {
                await UniTask.NextFrame(cancellationToken: token);

                if (player.isDead) break;
                if (player.isDodging) continue;

                var stats = playerStats;

                stats.currentStamina = Mathf.Min(
                    stats.currentStamina + recoveryPerSecond * Time.deltaTime,
                    stats.maxStamina);

                IngameUIManager.Instance.SetStaminaSlider();
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        finally
        {
            isRestoringStamina = false;
        }
    }
}
