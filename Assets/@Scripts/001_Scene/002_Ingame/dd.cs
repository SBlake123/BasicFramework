using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dd : MonoBehaviour
{
    //private Vector2 lastMoveDirection = Vector2.right;
    //private void MovePlayer()
    //{
    //    if (!CanInputAction() || playerState == PlayerState.DODGE || playerState == PlayerState.DIE)
    //    {
    //        desiredVelocity = Vector3.zero;
    //        return;
    //    }
    //    if (moveInput.sqrMagnitude > .0001f) lastMoveDirection = moveInput.normalized;
    //    UpdateMoveSpeed();
    //    desiredVelocity = new Vector3(moveInput.x, moveInput.y, 0) * moveSpeed;
    //    // Movement continues during Attack, but must not replace its animation/state.
    //    if (playerState == PlayerState.ATTACK) return;
    //    UpdateSpriteDirection();
    //    ChangeState(GetLocomotionState()).Forget();
    //}

    //private PlayerState GetLocomotionState()
    //{
    //    return moveInput.sqrMagnitude > .0001f ? PlayerState.MOVE : PlayerState.IDLE;
    //}

    //private bool CanStartAction()
    //{
    //    return isActiveAndEnabled && (playerState == PlayerState.IDLE || playerState == PlayerState.MOVE);
    //}

    //[SerializeField, Min(.01f)] private float attackDuration = .5f;
    //[SerializeField, Min(0)] private float dodgeDistance = 2f;
    //[SerializeField, Min(.01f)] private float dodgeDuration = .35f;

    //public async UniTask Init()
    //{
    //    movementBody = GetComponent<Rigidbody>();
    //    if (boundUI != null)
    //    {
    //        boundUI.AttackRequested -= AttackPlayer;
    //        boundUI.DodgeRequested -= DodgePlayer;
    //    }
    //    boundUI = IngameUIManager.Instance;
    //    boundUI.AttackRequested += AttackPlayer;
    //    boundUI.DodgeRequested += DodgePlayer;
    //}

    //private void FixedUpdate()
    //{
    //    if (playerState == PlayerState.DODGE)
    //    {
    //        float step = Mathf.Min(Time.fixedDeltaTime, dodgeTimeRemaining);
    //        float distance = dodgeDistance * step / Mathf.Max(.01f, dodgeDuration);
    //        CharacterContactMovement.Move(movementBody, (Vector3)dodgeDirection * (distance / Time.fixedDeltaTime));
    //        dodgeTimeRemaining = Mathf.Max(0f, dodgeTimeRemaining - step);
    //        return;
    //    }
    //    if (movementBody != null)
    //        CharacterContactMovement.Move(movementBody, CanInputAction() ? desiredVelocity : Vector3.zero);
    //}

    //private void OnDisable()
    //{
    //    stateCts?.Cancel();
    //    desiredVelocity = Vector3.zero;
    //    moveInput = virtualJoystickInput = Vector2.zero;
    //    isAttackRequested = isDodgeRequested = false;
    //    dodgeTimeRemaining = 0;
    //    AllStateBoolFalse();
    //    if (playerState != PlayerState.DIE) playerState = PlayerState.NONE;
    //}

    //private void OnDestroy()
    //{
    //    if (boundUI != null)
    //    {
    //        boundUI.AttackRequested -= AttackPlayer;
    //        boundUI.DodgeRequested -= DodgePlayer;
    //    }
    //    stateCts?.Cancel();
    //    stateCts?.Dispose();
    //}
    //private PlayerState playerState;

    //public interface IPlayerState
    //{
    //    UniTask EnterAsync(Player player, CancellationToken token);
    //}
    //private readonly Dictionary<PlayerState, IPlayerState> stateMap = new()
    //{
    //    { PlayerState.IDLE, new IdleState() },
    //    { PlayerState.MOVE, new MoveState() },
    //    { PlayerState.ATTACK, new AttackState() },
    //    { PlayerState.DODGE, new DodgeState() },
    //    { PlayerState.DIE, new DieState() }
    //};
    //bool isAttack, isDodge, isHit;
    //CancellationTokenSource stateCts;

    //public class IdleState : IPlayerState
    //{
    //    public async UniTask EnterAsync(Player player, CancellationToken token)
    //    {
    //        player.playerSkinBase.PlayIdle();
    //        await player.WaitForAction(token);
    //    }
    //}
    //public class MoveState : IPlayerState
    //{
    //    public async UniTask EnterAsync(Player player, CancellationToken token)
    //    {
    //        player.playerSkinBase.PlayMove();
    //        await player.WaitForAction(token);
    //    }
    //}
    //// Both locomotion states accept keyboard and UI requests through one path.
    //private async UniTask WaitForAction(CancellationToken token)
    //{
    //    while (!token.IsCancellationRequested)
    //    {
    //        if (Input.GetKeyDown(playerInputKeyCode.playerEvade) || isDodgeRequested)
    //        {
    //            ChangeState(PlayerState.DODGE).Forget();
    //            return;
    //        }
    //        if (Input.GetKeyDown(playerInputKeyCode.playerAttack) || isAttackRequested)
    //        {
    //            ChangeState(PlayerState.ATTACK).Forget();
    //            return;
    //        }
    //        await UniTask.Yield(PlayerLoopTiming.Update, token);
    //    }
    //}
    //public class AttackState : IPlayerState
    //{
    //    public async UniTask EnterAsync(Player player, CancellationToken token)
    //    {
    //        player.playerSkinBase.PlayAttack();
    //        await UniTask.Delay(TimeSpan.FromSeconds(player.attackDuration), cancellationToken: token);
    //        token.ThrowIfCancellationRequested();
    //        player.ReadMoveInput();
    //        player.ChangeState(player.GetLocomotionState()).Forget();
    //    }
    //}
    //public class DodgeState : IPlayerState
    //{
    //    public async UniTask EnterAsync(Player player, CancellationToken token)
    //    {
    //        player.ReadMoveInput();
    //        if (player.moveInput.sqrMagnitude > .0001f)
    //            player.lastMoveDirection = player.moveInput.normalized;
    //        player.dodgeDirection = player.lastMoveDirection;
    //        player.dodgeTimeRemaining = Mathf.Max(.01f, player.dodgeDuration);
    //        player.desiredVelocity = Vector3.zero;
    //        player.UpdateSpriteDirection();
    //        player.playerSkinBase.PlayDodge();
    //        await UniTask.WaitUntil(() => player.dodgeTimeRemaining <= 0, cancellationToken: token);
    //        token.ThrowIfCancellationRequested();
    //        player.ReadMoveInput();
    //        player.ChangeState(player.GetLocomotionState()).Forget();
    //    }
    //}
    //public class DieState : IPlayerState
    //{
    //    public UniTask EnterAsync(Player player, CancellationToken token)
    //    {
    //        player.desiredVelocity = Vector3.zero;
    //        player.playerSkinBase.PlayDie();
    //        return UniTask.CompletedTask;
    //    }
    //}

    //public async UniTask ChangeState(PlayerState state)
    //{
    //    if (playerState == state || !stateMap.ContainsKey(state) || playerState == PlayerState.DIE) return;
    //    // Attack/dodge cannot restart or interrupt an action already in progress.
    //    if ((state == PlayerState.ATTACK || state == PlayerState.DODGE) && !CanStartAction()) return;
    //    stateCts?.Cancel();
    //    stateCts?.Dispose();
    //    stateCts = new CancellationTokenSource();
    //    CancellationToken token = stateCts.Token;
    //    playerState = state;
    //    AllStateBoolFalse();
    //    isAttack = state == PlayerState.ATTACK;
    //    isDodge = nowEvading = state == PlayerState.DODGE;
    //    if (isAttack || isDodge) isAttackRequested = isDodgeRequested = false;
    //    try { await OnStateChange(stateCts); }
    //    catch (OperationCanceledException) when (token.IsCancellationRequested) { }
    //}
    //public UniTask OnStateChange()
    //{
    //    return stateCts == null ? UniTask.CompletedTask : OnStateChange(stateCts);
    //}
    //public async UniTask OnStateChange(CancellationTokenSource source)
    //{
    //    // Enter once per transition. The state owns its input wait or action delay.
    //    if (stateMap.TryGetValue(playerState, out var state))
    //        await state.EnterAsync(this, source.Token);
    //}
    //void AllStateBoolFalse()
    //{
    //    isAttack = isDodge = isHit = nowEvading = false;
    //}
}
