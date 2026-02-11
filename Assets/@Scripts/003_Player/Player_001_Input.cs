using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInputKeyCode
{
    public KeyCode playerUp = KeyCode.UpArrow;
    public KeyCode playerDown = KeyCode.DownArrow;
    public KeyCode playerLeft = KeyCode.LeftArrow;
    public KeyCode playerRight = KeyCode.RightArrow;
    public KeyCode playerEvade = KeyCode.Space;
    public KeyCode playerAttack = KeyCode.Z;
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

    bool CanInputAction()
    {
        var canInputAction = true;

        if (nowEvading || isHit) canInputAction = false;

        return canInputAction;
    }

    async UniTask PlayerActionCheck()
    {
        while (true)
        {
            PlayerMove();
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

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
                Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);

                SpriteRelocationCheck(moveDirection);
                SlowCheck();

                if (moveDirection != Vector3.zero)
                {
                    moveDirection = moveDirection.normalized; // 길이를 1로 만듦
                    transform.position += moveDirection * moveSpeed * Time.deltaTime;
                }
            }

            void SpriteRelocationCheck(Vector3 moveDirection)
            {
                if (playerSkinBase.needSprRelocation && isAttack == false)
                {
                    if (moveDirection.x > 0)
                    {
                        playerSkinBase.PlayerSprRelocationRight();
                    }

                    else if (moveDirection.x < 0)
                    {
                        playerSkinBase.PlayerSprRelocationLeft();
                    }
                }
            }

            void SlowCheck()
            {
                if (isAttack || isHit)
                {
                    moveSpeed = 2.5f;
                }
                else moveSpeed = 3f;
            }
        }
    }


}
