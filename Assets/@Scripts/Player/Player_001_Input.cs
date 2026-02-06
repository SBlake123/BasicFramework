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

public class PlayerInputKeyCode
{
    public KeyCode playerUp = KeyCode.UpArrow;
    public KeyCode playerDown = KeyCode.DownArrow;
    public KeyCode playerLeft = KeyCode.LeftArrow;
    public KeyCode playerRight = KeyCode.RightArrow;
    public KeyCode playerEvade = KeyCode.Space;
    public KeyCode playerAttack = KeyCode.Z;
}

public interface IPlayerState
{
    UniTask EnterAsync(Player player, CancellationToken token);
}

public partial class Player : MonoBehaviour
{
    PlayerInputKeyCode playerInputKeyCode = new PlayerInputKeyCode();
    float moveSpeed;
    bool nowEvading;

    private PlayerState playerState;

    private async UniTaskVoid PlayerInit()
    {
        moveSpeed = 3f;
        await ChangeState(PlayerState.IDLE);
        Debug.Log($"{playerState}");
    }

    private readonly Dictionary<PlayerState, IPlayerState> stateMap = new()
    {
        { PlayerState.IDLE, new IdleState() },
        { PlayerState.ATTACK, new AttackState() },
        { PlayerState.HIT, new HitState() },
        { PlayerState.DIE, new DieState() }
    };

    CancellationTokenSource stateCts;
    // Update is called once per frame

    public class IdleState : IPlayerState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {
            Debug.Log($"Idle Start - Frame: {Time.frameCount}");
            Debug.Log("Idle State");
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
            player.playerSkinBase.Attack();
            await UniTask.Delay(2000, cancellationToken: token);
            await player.ChangeState(PlayerState.IDLE);
        }
    }

    public class HitState : IPlayerState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {
            //맞았을 때 액션

        }

    }

    public class DieState : IPlayerState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {
            //죽었을 때 액션
        }
    }
    //이동, 이동중 액션 시 이동 못 함 같은 경우? 멈췄을때는 멈추기
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
            if (stateMap.TryGetValue(playerState, out var state))  // ② IDLE 찾음 → IdleState 객체 반환
            {
                await state.EnterAsync(this, stateCts.Token);  // ③ ← 여기서 IdleState.EnterAsync() 실행!!!
            }
            await UniTask.Yield(PlayerLoopTiming.Update, stateCts.Token);  // ④ 다음 프레임 대기
        }
    }

    bool CanInputAction()
    {
        var canInputAction = true;

        if (nowEvading) canInputAction = false;

        return canInputAction;
    }

    async UniTask PlayerActionCheck()
    {
        while(true)
        {
            PlayerMove();
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        //PlayerEvade();

        //void PlayerAttack()
        //{
        //    if (CanInputAction())
        //    {
        //        if (Input.GetKey(playerInputKeyCode.playerAttack))
        //        {
        //            Debug.Log("Attack Input");

        //            playerSkinBase.Attack();
        //        };
        //    }

        //}

        void PlayerEvade()
        {
            if (CanInputAction())
            {

            }
        }

        void PlayerMove()
        {
            if (CanInputAction())
            {
                Vector3 moveDirection = Vector3.zero;

                if (Input.GetKey(playerInputKeyCode.playerUp))  moveDirection += Vector3.up; 
                if (Input.GetKey(playerInputKeyCode.playerDown)) moveDirection += Vector3.down;
                if (Input.GetKey(playerInputKeyCode.playerLeft)) moveDirection += Vector3.left;
                if (Input.GetKey(playerInputKeyCode.playerRight)) moveDirection += Vector3.right;

                if (moveDirection != Vector3.zero)
                {
                    moveDirection = moveDirection.normalized; // 길이를 1로 만듦
                    transform.position += moveDirection * moveSpeed * Time.deltaTime;
                }
            }
        }
    }


}
