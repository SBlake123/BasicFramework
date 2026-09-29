using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DG.Tweening;

public partial class Monster_101_Fallen : Monster_000_Base
{

    public MonsterSkinFallen monsterSkinFallen;

    public Transform attackHitBoxTrf;
    protected override async UniTask OnStateChange()
    {

    }
    public override async UniTask ChangeState(int state)
    {
        switch (monsterState)
        {

            case MonsterState.IDLE:
                {
                    await OnIdle();
                }
                break;

            case MonsterState.CHASE:
                {
                    //그냥 쫓아가기 or 데굴데굴 구르기 신공

                    await OnChase();
                }
                break;

            case MonsterState.ATTACK:
                {
                    //챱챱 때리기
                    await OnAttack();
                }
                break;

            case MonsterState.RETURN:
                {
                    await OnReturn();
                }
                break;

            case MonsterState.DEAD:
                {
                    await OnDeath();
                }
                break;
        }

        await UniTask.WaitForFixedUpdate();
    }

    public override async UniTask Init()
    {
        
    }

    public override async UniTask TakeDamage(int attackDamage)
    {
        
    }

    float resumeMoveTime = 0f;
    bool isWaiting = false;


    protected async UniTask OnIdle()
    {
        CancellationToken token = stateCts.Token;
        Vector3 idleDestination = GetRandomPosition();

        //monsterSkinSkeleton.anim.Play(GAnimName.SKELETON_MOVE);

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
                    //monsterSkinSkeleton.anim.Play(GAnimName.SKELETON_MOVE);
                }

                if (IsArrived(idleDestination) || !CanMoveCheck(idleDestination))
                {
                    moveDestination = null;
                    CharacterContactMovement.Stop(movementBody);
                    //monsterSkinSkeleton.anim.Play(GAnimName.SKELETON_IDLE);

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

    protected async UniTask OnChase()
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

                //monsterSkinSkeleton.anim.Play("SkeletonMove");
                UpdateFacingDirection();
                MoveTo(target.position);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        catch (OperationCanceledException)
        {
        }
    }

    protected async UniTask OnReturn()
    {

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

                await UniTask.Delay(TimeSpan.FromSeconds(monsterStats.attackWindow), cancellationToken: token);
                await UniTask.Delay(TimeSpan.FromSeconds(monsterStats.attackRecovery), cancellationToken: token);
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
