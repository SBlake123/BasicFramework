using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player : MonoBehaviour
{
    public WeaponBase currentWeapon;
    public WeaponBase currentShield;

    private void Update()
    {
        ReadMoveInput();
        MovePlayer();
        if (Input.GetKeyDown(playerInputKeyCode.playerAttack)) AttackPlayer().Forget();
    }

    public async UniTask Init()
    {
        moveSpeed = 3f;
        await ChangeState(PlayerState.IDLE);

        movementBody = GetComponent<Rigidbody>();
        IngameUIManager.Instance.AttackRequested += async () => await AttackPlayer();
        

        MoveInputChanged += UpdateMoveAnimation;

    }
}
