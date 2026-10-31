using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public enum PlayerState
{
    NONE,
    IDLE,
    MOVE,
    ATTACK,
    DODGE,
    HIT,
    DEAD
}

public interface IPlayerState
{
    UniTask EnterAsync(Player player, CancellationToken token);
}

public partial class Player : MonoBehaviour
{
    private readonly Dictionary<PlayerState, IPlayerState> stateMap = new()
    {
        { PlayerState.IDLE, new IdleState() },
        { PlayerState.ATTACK, new AttackState() },
        { PlayerState.DODGE, new DodgeState() },
        { PlayerState.DEAD, new DeadState() }
    };

    private PlayerState playerState;
    private CancellationTokenSource stateCts;

    private bool isAttack = false;
    private bool isHit = false;
    public bool isDodging { get; set; } = false;
    public bool isDead { get; set; } = false;

    public event Action PlayerDied;

    public class IdleState : IPlayerState
    {
        public UniTask EnterAsync(Player player, CancellationToken token)
        {
            player.AllStateBoolFalse();
            player.lastMoveAnim = MoveAnim.None;
            player.MoveInputChanged?.Invoke(player.moveInput);

            return UniTask.CompletedTask;
        }
    }

    public class AttackState : IPlayerState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {
            if (player.lastAttackCooldown > 0)
            {
                await player.ChangeState(PlayerState.IDLE);
                return;
            }

            player.lastMoveAnim = MoveAnim.None;
            player.isAttackRequested = false;
            player.isAttack = true;

            if (player.currentWeapon != null)
            {
                player.lastAttackCooldown += IngameSessionManager.Instance.currentWeaponData.attackSpeed;
                player.playerSkinBase.ChangePlayerSwingPivot();
                player.SetLastLookPosition();

                foreach (var item in IngameSessionManager.Instance.specialAttributeList)
                {
                    item.TriggerAsync().Forget();
                }

                await player.currentWeapon.AttackAsync(player, token);
            }
            else
            {

            }

            await player.ChangeState(PlayerState.IDLE);

        }
    }

    float lastAttackCooldown = 0f;

    public async UniTask CheckAttackCooldown()
    {
        try
        {
            while (true)
            {
                lastAttackCooldown -= Time.deltaTime;

                if (lastAttackCooldown <= 0f)
                    lastAttackCooldown = 0f;

                await UniTask.Yield(PlayerLoopTiming.Update, destroyCancellationToken);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }

    public class DodgeState : IPlayerState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {
            player.isDodging = true;
            player.OnOffHitBox(false);
            player.dodgeDirection = player.lastLookDirection;
            player.playerSkinBase.EquipItemActivate(false);
            player.playerSkinBase.PlayDodge();

            while (true)
            {
                var state = player.playerSkinBase.anim.GetCurrentAnimatorStateInfo(0);

                if (state.IsName(GSkinSprName.DODGE) && state.normalizedTime >= 1f) break;

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            player.playerSkinBase.EquipItemActivate(true);
            player.EndDodge();
            await player.ChangeState(PlayerState.IDLE);
        }
    }

    public void EndDodge()
    {
        isDodging = false;

        OnOffHitBox(true);
    }

    public class DeadState : IPlayerState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {
            await player.OnDeath();
            player.PlayerDied?.Invoke();
        }
    }

    public async UniTask ChangeState(PlayerState nextState)
    {
        if (isDead && nextState != PlayerState.DEAD) return;

        if (playerState == nextState)
            return;

        stateCts?.Cancel();

        var cts = CancellationTokenSource.CreateLinkedTokenSource(
            destroyCancellationToken);

        stateCts = cts;
        playerState = nextState;

        try
        {
            await OnStateChange();
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            
        }
        finally
        {
            if (ReferenceEquals(stateCts, cts))
                stateCts = null;

            cts.Dispose();
        }

    }
    public async UniTask OnStateChange()
    {
        switch (playerState)
        {
            case PlayerState.IDLE:
                {
                    await stateMap[PlayerState.IDLE].EnterAsync(this, stateCts.Token);
                    break;
                }

            case PlayerState.ATTACK:
                {
                    await stateMap[PlayerState.ATTACK].EnterAsync(this, stateCts.Token);
                    break;
                }

            case PlayerState.DODGE:
                {
                    await stateMap[PlayerState.DODGE].EnterAsync(this, stateCts.Token);
                    break;
                }

            case PlayerState.DEAD:
                {
                    await stateMap[PlayerState.DEAD].EnterAsync(this, stateCts.Token);
                    return;
                }
        }
    }
    void AllStateBoolFalse()
    {
        isAttack = false;
        isDodging = false;
    }
}
