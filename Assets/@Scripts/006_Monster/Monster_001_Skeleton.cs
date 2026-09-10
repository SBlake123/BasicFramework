using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

//public enum MonsterState
//{
//    Idle,
//    Chase,
//    Attack,
//    Return,
//    Dead
//}

public class Monster_001_Skeleton : Monster_000_Base
{
    public MonsterSkinSkeleton monsterSkinSkeleton;
    public Transform facingObjectParentTrf;

    private MonsterState monsterState = MonsterState.Idle;

    private CancellationTokenSource stateCts;

    [SerializeField] private Transform target;
    private MonsterStats monsterStats = new MonsterStats();
    private Vector3 spawnPosition;

    bool isAttack;
    bool isDeath;

    private void Awake()
    {
        spawnPosition = transform.position;
        MonsterInit().Forget();
    }

    async UniTask MonsterInit()
    {
        stateCts = new CancellationTokenSource();
        await OnStateChange();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public override async UniTask ChangeState(int state)
    {
        if (monsterState != (MonsterState)state)
        {
            stateCts?.Cancel();
            stateCts?.Dispose();
            stateCts = new CancellationTokenSource();

            monsterState = (MonsterState)state;

            Debug.Log($"MonsterState : {state}");

            await OnStateChange();
        }
    }

    protected override async UniTask OnStateChange()
    {
        switch (monsterState)
        {
            
            case MonsterState.Idle:
                {
                    //기본적인 대기 단계에서는 스폰 범위를 돌아다니는 자유 행동까지는 가능하다.
                    await Idle();
                }
                break;

            case MonsterState.Chase:
                {
                    //적의 시야에 플레이어가 들어왔을 때 인지하고 공격을 시도한다.
                    await Chase();
                }
                break;
            case MonsterState.Attack:
                {
                    //범위안에 들어왔을 때 공격이 가능한 상태일 시 공격한다.
                    //공격이 끝나고 나서는 다시 공격하는 상태인지 판단해야 한다.
                    await OnAttack();
                }
                break;
            case MonsterState.Return:
                {
                    //Return되는 경우는 플레이어가 사라진 상태에서 Chase 범위를 벗어났을 때 뿐.
                    await Return();
                }
                break;
            case MonsterState.Dead:
                {
                    //죽는 단계는 끝이기 때문에 연결할 필요가 없다.
                    await OnDeath();
                }
                break;
        }

        await UniTask.WaitForFixedUpdate();
    }

    private UniTask Idle()
    {
        return IdleAction();
    }

    private UniTask Chase()
    {
        return ChaseAction();
    }

    private UniTask Return()
    {
        return ReturnAction();
    }

    private async UniTask IdleAction()
    {
        CancellationToken token = stateCts.Token;
        Vector3 idleDestination = GetRandomSpawnPosition();

        try
        {
            while (!token.IsCancellationRequested)
            {
                if (CanDetectTarget())
                {
                    ChangeState((int)MonsterState.Chase).Forget();
                    return;
                }

                monsterSkinSkeleton.anim.Play("SkeletonMove");
                UpdateFacingDirection();
                MoveTo(idleDestination);

                if (IsArrived(idleDestination))
                {
                    idleDestination = GetRandomSpawnPosition();
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async UniTask ChaseAction()
    {
        CancellationToken token = stateCts.Token;

        try
        {
            while (!token.IsCancellationRequested)
            {
                if (target == null || !CanDetectTarget() || HasLeftSpawnRange())
                {
                    ChangeState((int)MonsterState.Return).Forget();
                    return;
                }


                if (CanAttackTarget())
                {
                    ChangeState((int)MonsterState.Attack).Forget();
                    return;
                }

                monsterSkinSkeleton.anim.Play("SkeletonMove");
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
                    ChangeState((int)(CanDetectTarget() ? MonsterState.Chase : MonsterState.Return)).Forget();
                    return;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(monsterStats.attackPreparation), cancellationToken: token);

                if (!CanAttackTarget())
                {
                    ChangeState((int)(CanDetectTarget() ? MonsterState.Chase : MonsterState.Return)).Forget();
                    return;
                }

                // Player의 피해 함수가 만들어지면 이 위치에서 호출한다.
                Debug.Log($"{name} Attack");

                await UniTask.Delay(TimeSpan.FromSeconds(monsterStats.attackWindow), cancellationToken: token);
                await UniTask.Delay(TimeSpan.FromSeconds(monsterStats.attackRecovery), cancellationToken: token);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async UniTask ReturnAction()
    {
        CancellationToken token = stateCts.Token;

        try
        {
            monsterSkinSkeleton.anim.Play("SkeletonMove");
            UpdateFacingDirection();
            while (!token.IsCancellationRequested)
            {

                MoveTo(spawnPosition);

                if (IsArrived(spawnPosition))
                {
                    ChangeState((int)MonsterState.Idle).Forget();
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
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: destroyCancellationToken);
        Destroy(gameObject);
    }

    private bool CanDetectTarget()
    {
        if (target == null)
        {
            return false;
        }

        return Vector2.Distance(transform.position, target.position) <= monsterStats.detectionRange;
    }

    private bool CanAttackTarget()
    {
        if (target == null)
        {
            return false;
        }

        return Vector2.Distance(transform.position, target.position) <= monsterStats.attackRange;
    }

    private bool HasLeftSpawnRange()
    {
        if (monsterStats.leashRange <= 0f)
        {
            return false;
        }

        return Vector2.Distance(transform.position, spawnPosition) > monsterStats.leashRange;
    }

    private void MoveTo(Vector3 destination)
    {
        Vector3 flatDestination = new Vector3(destination.x, destination.y, transform.position.z);
        transform.position = Vector3.MoveTowards(
            transform.position,
            flatDestination,
            monsterStats.moveSpeed * Time.deltaTime);
    }

    private bool IsArrived(Vector3 destination)
    {
        return Vector2.Distance(transform.position, destination) <= 0.05f;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * monsterStats.leashRange;
        return spawnPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);
    }

    private void UpdateFacingDirection()
    {
        if (isAttack)
        {
            return;
        }

        var directionX = target.position.x - transform.position.x;


        var facingDirection = directionX > 0f ? 1 : -1;

        facingObjectParentTrf.localScale = new Vector3(
            facingDirection,
            facingObjectParentTrf.localScale.y,
            facingObjectParentTrf.localScale.z
        );

    }

    private void OnDestroy()
    {
        stateCts?.Cancel();
        stateCts?.Dispose();
    }

}
