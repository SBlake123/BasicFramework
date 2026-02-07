using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public enum PlayerState
{
    NONE,
    IDLE,
    ATTACK,
    HIT,
    DIE
}

public partial class Player : MonoBehaviour
{
    public interface IPlayerState
    {
        UniTask EnterAsync(Player player, CancellationToken token);
    }
    private readonly Dictionary<PlayerState, IPlayerState> stateMap = new()
    {
        { PlayerState.IDLE, new IdleState() },
        { PlayerState.ATTACK, new AttackState() },
        { PlayerState.HIT, new HitState() },
        { PlayerState.DIE, new DieState() }
    };

    bool isAttack = false;
    bool isHit = false;

    CancellationTokenSource stateCts;
    // Update is called once per frame

    public class IdleState : IPlayerState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {
            Debug.Log($"Idle Start - Frame: {Time.frameCount}");
            Debug.Log("Idle State");

            player.AllStateBoolFalse();
            player.playerSkinBase.PlayIdle();
            // 입력 대기
            while (!token.IsCancellationRequested)
            {
                if (Input.GetKeyDown(player.playerInputKeyCode.playerAttack))
                {
                    await player.ChangeState(PlayerState.ATTACK);
                    return;
                }
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
    }

    public class AttackState : IPlayerState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {
            //Attack에 관한 메소드
            player.isAttack = true;
            player.playerSkinBase.PlayAttack();
            await UniTask.Delay(500, cancellationToken: token);
            await player.ChangeState(PlayerState.IDLE);
        }
    }

    public class HitState : IPlayerState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {
            player.isHit = true;
            player.playerSkinBase.PlayHit();
            await UniTask.Delay(500, cancellationToken: token);
            await player.ChangeState(PlayerState.IDLE);
        }

    }

    public class DieState : IPlayerState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {
   
        }
    }

    public async UniTask ChangeState(PlayerState state)
    {
        stateCts?.Cancel();
        stateCts = new CancellationTokenSource();

        if (playerState != state)
        {
            playerState = state;
            Debug.Log($"Now State : {state}");

            await OnStateChange(stateCts);
        }
    }

    public async UniTask OnStateChange(CancellationTokenSource stateCts)
    {
        while (!stateCts.IsCancellationRequested)
        {
            if (stateMap.TryGetValue(playerState, out var state)) 
            {
                await state.EnterAsync(this, stateCts.Token); 
            }
            await UniTask.Yield(PlayerLoopTiming.Update, stateCts.Token);
        }
    }
    
    void AllStateBoolFalse()
    {
        isAttack = false;
        isHit = false;
    }
}
