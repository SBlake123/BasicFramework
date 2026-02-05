using System.Collections;
using System.Collections.Generic;
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
    float moveSpeed = 5f;
    bool nowEvading;
    // Update is called once per frame


    //이동, 이동중 액션 시 이동 못 함 같은 경우? 멈췄을때는 멈추기


    bool CanInputAction()
    {
        var canInputAction = true;

        if (nowEvading) canInputAction = false;

        return canInputAction;
    }

    void PlayerActionCheck()
    {
        PlayerEvade();
        PlayerMove();
        PlayerAttack();
        void PlayerAttack()
        {
            if (CanInputAction())
            {
                if (Input.GetKey(playerInputKeyCode.playerAttack))
                {
                    playerSkinBase.Attack();
                };
            }
            
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
                Vector3 moveDirection = Vector3.zero;
                if (Input.GetKey(playerInputKeyCode.playerUp)) moveDirection += Vector3.up;
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
