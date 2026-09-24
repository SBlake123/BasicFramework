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
    MOVE,
    ATTACK,
    DODGE,
    HIT,
    DEAD
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
        //{ PlayerState.MOVE, new MoveState()},
        //{ PlayerState.HIT, new HitState() },
        { PlayerState.ATTACK, new AttackState() },
        { PlayerState.DODGE, new DodgeState() },
        { PlayerState.DEAD, new DieState() }
    };

    bool isAttack = false;
    bool isDodge = false;
    bool isHit = false;

    CancellationTokenSource stateCts;
    // Update is called once per frame

    public class IdleState : IPlayerState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {
            //Debug.Log($"Idle Start - Frame: {Time.frameCount}");

            player.AllStateBoolFalse();
            player.MoveInputChanged?.Invoke(player.moveInput);
        }
    }

    public class AttackState : IPlayerState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {
            //Attack에 관한 메소드
            player.lastMoveAnim = MoveAnim.None;
            player.isAttackRequested = false;
            player.isAttack = true;

            if (player.currentWeapon != null)
            {
                await player.currentWeapon.AttackAsync(player, token);
            }
            else
            {

            }

            await player.ChangeState(PlayerState.IDLE);

        }
    }

    //public class HitState : IPlayerState
    //{
    //    public async UniTask EnterAsync(Player player, CancellationToken token)
    //    {
    //        player.isHit = true;
    //        player.playerSkinBase.PlayHit();
    //        await UniTask.Delay(500);

    //        await player.ChangeState(PlayerState.IDLE);
    //    }

    //}

    //public class MoveState : IPlayerState
    //{  
    //    public async UniTask EnterAsync(Player player, CancellationToken token)
    //    {
    //        player.playerSkinBase.PlayMove();
    //    }
    //}

    public class DodgeState : IPlayerState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {
            player.isDodge = true;

        }
    }

    public class DieState : IPlayerState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {
            await player.OnDeath();
            PopupManager.Instance.setPopUpCode(false, LanguageManager.Instance.GetLangScript(10004), LanguageManager.Instance.GetLangScript(10002));
            PopupManager.Instance.AddMethodToBtn(async () => await SceneLoadManager.Instance.LoadScene(GSceneName.TITLE_SCENE));
        }
    }

    public async UniTask ChangeState(PlayerState state)
    {
        if (playerState == state)
            return;

        stateCts?.Cancel();
        stateCts = new CancellationTokenSource();

        if (playerState != state)
        {
            playerState = state;

            await OnStateChange(stateCts);
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

            //case PlayerState.MOVE:
            //    {
            //        await stateMap[PlayerState.MOVE].EnterAsync(this,stateCts.Token);

            //        break;
            //    }

            case PlayerState.ATTACK:
                {
                    await stateMap[PlayerState.ATTACK].EnterAsync(this, stateCts.Token);
                    break;
                }

            case PlayerState.DODGE:
                {

                    break;
                }

            case PlayerState.DEAD:
                {
                    await stateMap[PlayerState.DEAD].EnterAsync(this, stateCts.Token);

                    return;
                }
        }
        await UniTask.WaitForFixedUpdate();
    }

    public async UniTask OnStateChange(CancellationTokenSource stateCts)
    {
        if (stateMap.TryGetValue(playerState, out var state))
        {
            await state.EnterAsync(this, stateCts.Token);
        }
        await UniTask.Yield(PlayerLoopTiming.Update, stateCts.Token);
    }

    void AllStateBoolFalse()
    {
        isAttack = false;
        isDodge = false;
    }
}
