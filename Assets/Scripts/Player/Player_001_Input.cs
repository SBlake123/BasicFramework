using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputKeyCode
{
    public KeyCode playerUp = KeyCode.W;
    public KeyCode playerDown = KeyCode.S;
    public KeyCode playerLeft = KeyCode.A;
    public KeyCode playerRight = KeyCode.D;
    public KeyCode playerEvade = KeyCode.Space;
}

public class Player_001_Input : MonoBehaviour
{
    PlayerInputKeyCode playerInputKeyCode;
    float moveSpeed = 1f;
    bool nowEvading;
    // Update is called once per frame
    void Update()
    {
        PlayerActionCheck();
    }

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
                if (Input.GetKey(playerInputKeyCode.playerUp)) moveDirection += Vector3.forward;
                if (Input.GetKey(playerInputKeyCode.playerDown)) moveDirection += Vector3.back;
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
