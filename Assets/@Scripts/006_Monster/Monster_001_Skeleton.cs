using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DG.Tweening;

public partial class Monster_001_Skeleton : Monster_000_Base
{
    public MonsterSkinSkeleton monsterSkinSkeleton;

    public Transform attackHitBoxTrf;


    private void FixedUpdate()
    {
        if (movementBody == null) return;
        if (!moveDestination.HasValue)
        {
            CharacterContactMovement.Stop(movementBody);
            return;
        }
        Vector3 delta = moveDestination.Value - movementBody.position;
        delta.z = 0;
        CharacterContactMovement.Move(movementBody, Vector3.ClampMagnitude(delta / Time.fixedDeltaTime, monsterStats.moveSpeed));
    }

    public override async UniTask Init()
    {
        await base.Init();
    }

    public override async UniTask ChangeState(int state)
    {
        if (monsterState != (MonsterState)state)
        {
            moveDestination = null;
            stateCts?.Cancel();
            stateCts?.Dispose();
            stateCts = new CancellationTokenSource();

            monsterState = (MonsterState)state;

            //Debug.Log($"MonsterState : {state}");

            await OnStateChange();
        }
    }

    protected override async UniTask OnStateChange()
    {
        switch (monsterState)
        {

            case MonsterState.IDLE:
                {
                    //기본적인 대기 단계에서는 스폰 범위를 돌아다니는 자유 행동까지는 가능하다.
                    await OnIdle();
                }
                break;

            case MonsterState.CHASE:
                {
                    //적의 시야에 플레이어가 들어왔을 때 인지하고 공격을 시도한다.
                    await OnChase();
                }
                break;
            case MonsterState.ATTACK:
                {
                    //범위안에 들어왔을 때 공격이 가능한 상태일 시 공격한다.
                    //공격이 끝나고 나서는 다시 공격하는 상태인지 판단해야 한다.
                    await OnAttack();
                }
                break;
            case MonsterState.RETURN:
                {
                    //Return되는 경우는 플레이어가 사라진 상태에서 Chase 범위를 벗어났을 때 뿐.
                    await OnReturn();
                }
                break;
            case MonsterState.DEAD:
                {
                    //죽는 단계는 끝이기 때문에 연결할 필요가 없다.
                    await OnDeath();
                }
                break;
        }

        await UniTask.WaitForFixedUpdate();
    }

    float resumeMoveTime = 0f;
    bool isWaiting = false;

    private async UniTask OnIdle()
    {
        CancellationToken token = stateCts.Token;
        Vector3 idleDestination = GetRandomPosition();

        monsterSkinSkeleton.anim.Play(GAnimName.SKELETON_MOVE);

        try
        {
            while (!token.IsCancellationRequested)
            {
                if (CanDetectTarget())
                {
                    ChangeState((int)MonsterState.CHASE).Forget();
                    return;
                }

                if (isWaiting)
                {
                    if (Time.time < resumeMoveTime)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                        continue;
                    }

                    isWaiting = false;
                    idleDestination = GetRandomPosition();
                    monsterSkinSkeleton.anim.Play(GAnimName.SKELETON_MOVE);
                }

                if (IsArrived(idleDestination) || !CanMoveCheck(idleDestination))
                {
                    moveDestination = null;
                    CharacterContactMovement.Stop(movementBody);
                    monsterSkinSkeleton.anim.Play(GAnimName.SKELETON_IDLE);

                    isWaiting = true;
                    resumeMoveTime = Time.time + 2f;
                }
                else
                {
                    MoveTo(idleDestination);
                    UpdateFacingDirection();
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }

    private async UniTask OnChase()
    {
        CancellationToken token = stateCts.Token;

        try
        {
            while (!token.IsCancellationRequested)
            {
                if (target == null || !CanDetectTarget())// || HasLeftSpawnRange())
                {
                    ChangeState((int)MonsterState.RETURN).Forget();
                    return;
                }


                if (CanAttackTarget())
                {
                    ChangeState((int)MonsterState.ATTACK).Forget();
                    return;
                }

                monsterSkinSkeleton.anim.Play(GAnimName.SKELETON_MOVE);
                UpdateFacingDirection();
                MoveTo(target.position);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    protected override async UniTask OnAttack()
    {
        CancellationToken token = stateCts.Token;

        try
        {
            while (!token.IsCancellationRequested)
            {
                if (!CanAttackTarget())
                {
                    ChangeState((int)(CanDetectTarget() ? MonsterState.CHASE : MonsterState.RETURN)).Forget();
                    return;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(monsterStats.attackPreparation), cancellationToken: token);

                if (!CanAttackTarget())
                {
                    ChangeState((int)(CanDetectTarget() ? MonsterState.CHASE : MonsterState.RETURN)).Forget();
                    return;
                }

                await AttackAsync(IngameSessionManager.Instance.player, stateCts.Token);

                // Player의 피해 함수가 만들어지면 이 위치에서 호출한다.

                await UniTask.Delay(TimeSpan.FromSeconds(monsterStats.attackWindow), cancellationToken: token);
                await UniTask.Delay(TimeSpan.FromSeconds(monsterStats.attackRecovery), cancellationToken: token);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async UniTask OnReturn()
    {
        CancellationToken token = stateCts.Token;

        try
        {
            monsterSkinSkeleton.anim.Play(GAnimName.SKELETON_MOVE);
            UpdateFacingDirection();
            while (!token.IsCancellationRequested)
            {

                MoveTo(spawnPosition);

                if (IsArrived(spawnPosition))
                {
                    ChangeState((int)MonsterState.IDLE).Forget();
                    return;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    protected override async UniTask OnDeath()
    {
        transform.DOShakePosition(0.5f, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
        {
            ResetForPool(() =>
            {
                if (attackHitBoxTrf != null)
                    attackHitBoxTrf.gameObject.SetActive(false);
            });
        });

        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: destroyCancellationToken);
    }
}
