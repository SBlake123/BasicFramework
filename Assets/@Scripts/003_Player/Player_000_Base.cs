using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player : MonoBehaviour
{
    public WeaponBase currentWeapon { get; set; }
    public ShieldBase currentShield { get; set; }

    public BoxCollider playerHitBox;
    private void Update()
    {
        ReadMoveInput();
        MovePlayer();
        AimPlayer();
        if (Input.GetKeyDown(playerInputKeyCode.playerAttack)) AttackPlayer().Forget();
    }
    private void FixedUpdate()
    {
        if (movementBody != null)
        {
            if (isDodging)
            {
                CharacterContactMovement.Move(movementBody, lastLookDirection * 5f);
                return;
            }

            CharacterContactMovement.Move(movementBody, CanInputAction() ? desiredVelocity : Vector3.zero);
        }
    }

    public async UniTask Init()
    {
        moveSpeed = 3f;
        await ChangeState(PlayerState.IDLE);

        movementBody = GetComponent<Rigidbody>();
        IngameUIManager.Instance.AttackRequested += async () => await AttackPlayer();
        IngameUIManager.Instance.DodgeRequested += async () => await DodgePlayer();


        MoveInputChanged += UpdateMoveAnimation;

    }
}
