using Cysharp.Threading.Tasks;
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
    private PlayerInputKeyCode playerInputKeyCode = new PlayerInputKeyCode();

    [Header("Input")]
    [SerializeField] private bool useVirtualJoystickInEditor;

    private float moveSpeed;
    private bool nowEvading;
    private Vector2 moveInput;
    private Vector2 virtualJoystickInput;

    private PlayerState playerState;

    
    private bool CanInputAction()
    {
        return !nowEvading && !isHit;
    }

    /// <summary>
    /// Virtual joystick calls this while the player drags it.
    /// </summary>
    public void SetVirtualJoystickInput(Vector2 direction)
    {
        virtualJoystickInput = Vector2.ClampMagnitude(direction, 1f);
    }

    /// <summary>
    /// Virtual joystick calls this when the player's finger leaves it.
    /// </summary>
    public void ClearVirtualJoystickInput()
    {
        virtualJoystickInput = Vector2.zero;
    }

    private void ReadMoveInput()
    {
        if (Application.isMobilePlatform || useVirtualJoystickInEditor)
        {
            moveInput = virtualJoystickInput;
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

        return new Vector2(horizontal, vertical).normalized;
    }

    private void MovePlayer()
    {
        if (!CanInputAction() || moveInput == Vector2.zero)
        {
            return;
        }

        UpdateMoveSpeed();
        UpdateSpriteDirection();

        Vector3 moveDirection = new Vector3(moveInput.x, moveInput.y, 0f);
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
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
            playerSkinBase.PlayerSprRelocationRight();
        }
        else if (moveInput.x < 0f)
        {
            playerSkinBase.PlayerSprRelocationLeft();
        }
    }

    // Legacy PlayerActionCheck() read keyboard input and moved the player in a
    // UniTask loop. The same responsibility now lives in Update, ReadMoveInput,
    // and MovePlayer so keyboard and a virtual joystick use one movement path.
}
