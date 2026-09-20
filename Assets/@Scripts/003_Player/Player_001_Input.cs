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

    [Header("Input")]
    [SerializeField] private bool useVirtualJoystickInEditor;

    private float moveSpeed;
    private bool nowEvading;
    public Vector2 moveInput;
    public Vector2 aimInput;
    private Vector2 movingVirtualJoystickInput;
    private Vector2 aimingVirtualJoystickInput;
    private Rigidbody movementBody;
    private Vector3 desiredVelocity;

    public Vector2 lastMoveDirection { get; private set; } = Vector2.right;
    public Vector2 lastAimDirection { get; private set; } = Vector2.right;
    public Vector2 lastLookDirection { get; private set; } = Vector2.right;

    private bool isAttackRequested;

    public event Action<Vector2> MoveInputChanged;
    public event Action<Vector2> AimInputChanged;
    public bool isPlayerFacingRight = true;
    private MoveAnim lastMoveAnim = MoveAnim.None; 

    private void OnDisable()
    {
        desiredVelocity = Vector3.zero;

    }

    private PlayerState playerState;

    
    private bool CanInputAction()
    {
        return !nowEvading && !isHit;
    }

    /// <summary>
    /// Virtual joystick calls this while the player drags it.
    /// </summary>
    public void SetMovingVirtualJoystickInput(Vector2 direction)
    {
        movingVirtualJoystickInput = Vector2.ClampMagnitude(direction, 1f);

        if (movingVirtualJoystickInput.sqrMagnitude > 0.001f)
        {
            lastMoveDirection = movingVirtualJoystickInput.normalized;

        }

        MoveInputChanged?.Invoke(moveInput);
    }

    /// <summary>
    /// Virtual joystick calls this when the player's finger leaves it.
    /// </summary>
    public void ClearMovingVirtualJoystickInput()
    {
        movingVirtualJoystickInput = Vector2.zero;
        moveInput = movingVirtualJoystickInput;
        MoveInputChanged?.Invoke(moveInput);
    }

    public void SetAimingVirtualJoystickInput(Vector2 direction)
    {
        aimingVirtualJoystickInput = Vector2.ClampMagnitude(direction, 1f);

        if (aimingVirtualJoystickInput.sqrMagnitude > 0.001f)
        {
            lastAimDirection = aimingVirtualJoystickInput.normalized;
        }

        AimInputChanged?.Invoke(aimInput);
    }

    public void ClearAimingVirtualJoystickInput()
    {
        aimingVirtualJoystickInput = Vector2.zero;
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
        //Debug.Log("Move");

        
        
        if (!CanInputAction() || moveInput == Vector2.zero)
        {
            desiredVelocity = Vector3.zero;

            return;
        }
        

        //ChangeState(PlayerState.MOVE);
        UpdateMoveSpeed();
        UpdateSpriteDirection();
        UpdateWeaponRotation();

        Vector3 moveDirection = new Vector3(moveInput.x, moveInput.y, 0f);
        desiredVelocity = moveDirection * moveSpeed;
    }

    private async UniTask AttackPlayer()
    {
        OnAttackRequested();
        await ChangeState(PlayerState.ATTACK);
    }

    private void OnAttackRequested()
    {
        isAttackRequested = true;
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

        if (moveInput.x > 0f)
        {
            isPlayerFacingRight = true;
            playerSkinBase.PlayerSprRelocationRight();
            UpdateWeaponRotation();
        }
        else if (moveInput.x < 0f)
        {
            isPlayerFacingRight = false;
            playerSkinBase.PlayerSprRelocationLeft();
            UpdateWeaponRotation();
        }
    }
    public void UpdateWeaponRotation()
    {
        float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;

        playerSkinBase.weaponTrf.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private bool CanMoveAnimPlay()
    {
        bool canMoveAnimPlay = false;
        if (!isAttack && !isDodge) canMoveAnimPlay = true;
        return canMoveAnimPlay;
    }


    // Legacy PlayerActionCheck() read keyboard input and moved the player in a
    // UniTask loop. The same responsibility now lives in Update, ReadMoveInput,
    // and MovePlayer so keyboard and a virtual joystick use one movement path.
}
