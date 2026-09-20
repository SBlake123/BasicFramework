using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DG.Tweening;

public class Monster_001_Skeleton : Monster_000_Base
{
    public MonsterSkinSkeleton monsterSkinSkeleton;
    public Transform facingObjectParentTrf;

    private MonsterState monsterState = MonsterState.IDLE;

    private CancellationTokenSource stateCts;

    [SerializeField] private Transform target;
    private MonsterStats monsterStats = new MonsterStats();
    public Rigidbody movementBody;
    private Vector3 spawnPosition;
    private Vector3? moveDestination;

    bool isAttack;
    bool isDeath;

    private void FixedUpdate()
    {
        if (movementBody == null) return;
        if (!moveDestination.HasValue)
        {

            return;
        }
        Vector3 delta = moveDestination.Value - movementBody.position;
        delta.z = 0;
        CharacterContactMovement.Move(movementBody, Vector3.ClampMagnitude(delta / Time.fixedDeltaTime, monsterStats.moveSpeed));
    }

    private void Awake()
    {
        movementBody = GetComponent<Rigidbody>();
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

    private async UniTask OnIdle()
    {
        CancellationToken token = stateCts.Token;
        Vector3 idleDestination = GetRandomSpawnPosition();

        try
        {
            while (!token.IsCancellationRequested)
            {
                if (CanDetectTarget())
                {
                    ChangeState((int)MonsterState.CHASE).Forget();
                    return;
                }

                monsterSkinSkeleton.anim.Play(GAnimName.SKELETON_MOVE);
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
                    ChangeState((int)(CanDetectTarget() ? MonsterState.CHASE : MonsterState.RETURN)).Forget();
                    return;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(monsterStats.attackPreparation), cancellationToken: token);

                if (!CanAttackTarget())
                {
                    ChangeState((int)(CanDetectTarget() ? MonsterState.CHASE : MonsterState.RETURN)).Forget();
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
        transform.DOShakePosition(0.5f, 0.2f).SetEase(Ease.Linear).OnComplete(() => Destroy(gameObject));

        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: destroyCancellationToken);
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

    //private bool HasLeftSpawnRange()
    //{
    //    if (monsterStats.leashRange <= 0f)
    //    {
    //        return false;
    //    }

    //    return Vector2.Distance(transform.position, spawnPosition) > monsterStats.leashRange;
    //}

    private void MoveTo(Vector3 destination)
    {
        moveDestination = new Vector3(destination.x, destination.y, transform.position.z);
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

    public override async UniTask TakeDamage(int attackDamage)
    {
        GameObject obj = await ObjectPool.Instance.PopFromPool(GPrefabName.ATTACK_EFFECT_BASE, (RectTransform)IngameUIManager.Instance.hudCanvas.transform, false);

        obj.transform.position = Camera.main.WorldToScreenPoint(damageTrf.transform.position);

        DamageVal damageVal = obj.GetComponent<DamageVal>();

        damageVal.SetDamage(attackDamage);
        damageVal.Play();

        CalculateDamage(attackDamage);
        //몬스터의 방어력 값 계산 후 적용
    }

    public void CalculateDamage(int attackDamage)
    {
        monsterStats.currentHp -= attackDamage;

        //죽음판정
        if (monsterStats.currentHp <= 0)
            ChangeState((int)MonsterState.DEAD).Forget();
    }

}
