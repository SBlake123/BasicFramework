using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

public enum MoveAnim { None, Idle, Move }

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
    private PlayerInputKeyCode playerInputKeyCode = new PlayerInputKeyCode();

    private float moveSpeed = 3f;
    private bool useVirtualJoystickInEditor;
    private bool nowEvading;
    private Vector2 movingVirtualJoystickInput;
    private Vector2 aimingVirtualJoystickInput;
    private Rigidbody movementBody;
    private Vector3 desiredVelocity;

    public Vector2 moveInput { get; private set; }
    public Vector2 aimInput { get; private set; }
    public Vector2 lastMoveDirection { get; private set; } = Vector2.right;
    public Vector2 lastAimDirection { get; private set; } = Vector2.right;
    public Vector2 lastLookDirection { get; private set; } = Vector2.right;
    public Vector2 dodgeDirection { get; private set; } = Vector2.right;

    private bool isAttackRequested;

    public event Action<Vector2> MoveInputChanged;
    public event Action<Vector2> AimInputChanged;
    public bool hasMovingInput { get; private set; }
    public bool hasAimInput { get; private set; }

    public bool isPlayerFacingRight { get; private set; } = true;
    private MoveAnim lastMoveAnim = MoveAnim.None; 

    private void OnDisable()
    {
        desiredVelocity = Vector3.zero;
    }

    private bool CanInputAction()
    {
        return !InputBlocked && !isDodging && !isDead;
    }

    public void SetMovingVirtualJoystickInput(Vector2 direction)
    {
        if (!CanInputAction()) return;

        movingVirtualJoystickInput = Vector2.ClampMagnitude(direction, 1f);

        if (movingVirtualJoystickInput.sqrMagnitude > 0.001f)
        {
            hasMovingInput = true;
            lastMoveDirection = movingVirtualJoystickInput.normalized;
            SetLastLookPosition();
        }

        MoveInputChanged?.Invoke(moveInput);
    }

    public void ClearMovingVirtualJoystickInput()
    {
        movingVirtualJoystickInput = Vector2.zero;
        hasMovingInput = false;
        moveInput = movingVirtualJoystickInput;
        MoveInputChanged?.Invoke(moveInput);
    }

    public void SetAimingVirtualJoystickInput(Vector2 direction)
    {
        if (!CanInputAction()) return;

        aimingVirtualJoystickInput = Vector2.ClampMagnitude(direction, 1f);

        if (aimingVirtualJoystickInput.sqrMagnitude > 0.001f)
        {
            hasAimInput = true;
            lastAimDirection = aimingVirtualJoystickInput.normalized;
            SetLastLookPosition();
        }

        AimInputChanged?.Invoke(aimInput);
    }

    public void SetLastLookPosition()
    {
        if (hasAimInput)
        {
            lastLookDirection = lastAimDirection;
        }
        else if (hasMovingInput)
        {        
            lastLookDirection = lastMoveDirection;
        }
    }

    public void ClearAimingVirtualJoystickInput()
    {
        aimingVirtualJoystickInput = Vector2.zero;
        hasAimInput = false;
        aimInput = aimingVirtualJoystickInput;
        AimInputChanged?.Invoke(aimInput);
    }


    private void ReadMoveInput()
    {
        if (Application.isMobilePlatform || useVirtualJoystickInEditor)
        {
            moveInput = movingVirtualJoystickInput;
            return;
        }

        moveInput = ReadKeyboardMoveInput();
        
    }

    private Vector2 ReadKeyboardMoveInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(playerInputKeyCode.playerLeft)) horizontal -= 1f;
        if (Input.GetKey(playerInputKeyCode.playerRight)) horizontal += 1f;
        if (Input.GetKey(playerInputKeyCode.playerDown)) vertical -= 1f;
        if (Input.GetKey(playerInputKeyCode.playerUp)) vertical += 1f;

        MoveInputChanged?.Invoke(moveInput);

        return new Vector2(horizontal, vertical).normalized;
    }

    private void MovePlayer()
    {
        if (!CanInputAction())
        {
            return;
        }

        if (moveInput == Vector2.zero)
        {
            desiredVelocity = Vector3.zero;
            return;
        }
       
        UpdateMoveSpeed();
        UpdateSpriteDirection();
        UpdateWeaponRotation();

        Vector3 moveDirection = new Vector3(moveInput.x, moveInput.y, 0f);
        desiredVelocity = moveDirection * moveSpeed;
    }

    private void AimPlayer()
    {
        UpdateSpriteDirection();
        UpdateWeaponRotation();
    }

    private async UniTask AttackPlayer()
    {
        if (CanAttack() == false) return;

        OnAttackRequested();
        await ChangeState(PlayerState.ATTACK);
    }

    private async UniTask DodgePlayer()
    {
        if (CanDodge() == false) return;

        IngameSessionManager.Instance.playerStats.currentStamina -= IngameSessionManager.Instance.dodgeStaminaCost;
        IngameUIManager.Instance.SetStaminaSlider();

        OnDodgeRequested();
        await ChangeState(PlayerState.DODGE);
    }

    private void OnAttackRequested()
    {
        isAttackRequested = true;
    }

    private void OnDodgeRequested()
    {
        isDodging = true;
    }

    private void UpdateMoveAnimation(Vector2 input)
    {
        if (!CanMoveAnimPlay()) return;

        float inputSize = input.magnitude;

        MoveAnim next = inputSize > 0.01f ? MoveAnim.Move : MoveAnim.Idle;   

        if (lastMoveAnim == next) return;

        lastMoveAnim = next;

        if (next == MoveAnim.Move)
            playerSkinBase.PlayMove();
        else if(next == MoveAnim.Idle)
            playerSkinBase.PlayIdle();
    }

    private void UpdateMoveSpeed()
    {
        moveSpeed = isAttack || isHit ? 2.5f : 3f;
    }

    private void UpdateSpriteDirection()
    {
        if (!playerSkinBase.needSprRelocation || isAttack)
        {
            return;
        }

        if (lastLookDirection.x > 0f)
        {
            isPlayerFacingRight = true;
            playerSkinBase.PlayerSprRelocationRight();
            UpdateWeaponRotation();
        }
        else if (lastLookDirection.x < 0f)
        {
            isPlayerFacingRight = false;
            playerSkinBase.PlayerSprRelocationLeft();
            UpdateWeaponRotation();
        }
    }
    public void UpdateWeaponRotation()
    {
        float angle = Mathf.Atan2(lastLookDirection.y, lastLookDirection.x) * Mathf.Rad2Deg;

        playerSkinBase.aimPivotTrf.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private bool CanMoveAnimPlay()
    {
        bool canMoveAnimPlay = false;
        if (!isAttack && !isDodging && !isDead) canMoveAnimPlay = true;
        return canMoveAnimPlay;
    }

    private bool CanAttack()
    {
        return !InputBlocked && !isDodging && !isDead && !isAttack && lastAttackCooldown <= 0f;
    }

    private bool CanDodge()
    {
        return !InputBlocked && !isDead && !isDodging && IngameSessionManager.Instance.playerStats.currentStamina
                >= IngameSessionManager.Instance.dodgeStaminaCost;
    }
}
