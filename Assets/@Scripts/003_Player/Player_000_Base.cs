using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player : MonoBehaviour
{
    void Start()
    {
        PlayerInit().Forget();
    }

    private void Update()
    {
        ReadMoveInput();
        MovePlayer();
    }

    private async UniTaskVoid PlayerInit()
    {
        moveSpeed = 3f;
        await ChangeState(PlayerState.IDLE);
        Debug.Log($"{playerState}");
    }

}
