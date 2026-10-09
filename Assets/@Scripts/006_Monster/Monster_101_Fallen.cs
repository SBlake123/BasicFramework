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

    public float moveSpeed { get; set; }

    private float cooldownTerm { get; set; }

    public void Start()
    {
        Init();
    }
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
        CharacterContactMovement.Move(movementBody, Vector3.ClampMagnitude(delta / Time.fixedDeltaTime, moveSpeed));
    }

    public override async UniTask Init()
    {
        await base.Init();
        moveSpeed = monsterStats.moveSpeed;
        monsterStats.maxHp = 200;
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
            Debug.Log($"FallenState{monsterState}");
            await OnStateChange();
        }
    }

    protected override async UniTask OnStateChange()
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

            case MonsterState.SPECIAL_ATTACK: //롤링 어택
                {
                    //챱챱 때리기
                    await OnRollingAttack();
                }
                break;

            case MonsterState.SUNKEN_ATTACK: //롤링 어택
                {
                    //챱챱 때리기
                    await OnSunkenAttack();
                }
                break;

            case MonsterState.COOLDOWN:
                {
                    //챱챱 때리기
                    await OnCooldown();
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



    float resumeMoveTime = 0f;
    bool isWaiting = false;


    protected async UniTask OnIdle()
    {
        CancellationToken token = stateCts.Token;
        Vector3 idleDestination = GetRandomPosition();

        monsterSkinFallen.anim.Play(GAnimName.MOVE);

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
                    monsterSkinFallen.anim.Play(GAnimName.MOVE);
                }

                if (IsArrived(idleDestination) || !CanMoveCheck(idleDestination))
                {
                    moveDestination = null;
                    CharacterContactMovement.Stop(movementBody);
                    monsterSkinFallen.anim.Play(GAnimName.MOVE);

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

        catch (MissingReferenceException)
        {

        }
    }

    protected async UniTask OnChase()
    {
        CancellationToken token = stateCts.Token;

        monsterSkinFallen.anim.Play(GAnimName.MOVE);

        ChoiceNextAttack(out MonsterState nextAttackState, out float nextAttackRange);

        try
        {
            while (!token.IsCancellationRequested)
            {
                if (target == null || !CanDetectTarget())// || HasLeftSpawnRange())
                {
                    ChangeState((int)MonsterState.RETURN).Forget();
                    return;
                }

                if (CanAttackTarget(nextAttackRange))
                {
                    ChangeState((int)nextAttackState).Forget();
                    return;
                }


                UpdateFacingDirection();
                MoveTo(target.position);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        catch (OperationCanceledException)
        {
        }
        catch (MissingReferenceException)
        {

        }
    }

    void ChoiceNextAttack(out MonsterState nextAttackState, out float nextAttackRange)
    {
        //MonsterState selectedState = UnityEngine.Random.Range(0, 2) == 0 ? MonsterState.ATTACK : MonsterState.SPECIAL_ATTACK;

        int randomValue = UnityEngine.Random.Range(0, 10);

        MonsterState selectedState = MonsterState.SUNKEN_ATTACK;//MonsterState.SPECIAL_ATTACK;

        if (randomValue < 2)
        {
            selectedState = MonsterState.SPECIAL_ATTACK; //MonsterState.SPECIAL_ATTACK;
        }
        else
        {
            selectedState = MonsterState.SUNKEN_ATTACK; //MonsterState.SPECIAL_ATTACK;
        } 

        switch (selectedState)
        {
            case MonsterState.ATTACK:
                {
                    nextAttackRange = monsterStats.attackRange;
                    break;
                }

            case MonsterState.SPECIAL_ATTACK:
                {
                    nextAttackRange = 10f;
                    break;
                }

            case MonsterState.SUNKEN_ATTACK:
                {
                    nextAttackRange = 5f;
                    break;
                }

            default:
                {
                    nextAttackRange = monsterStats.attackRange;
                    break;
                }
        }

        nextAttackState = selectedState;
    }

    protected async UniTask OnReturn()
    {
        CancellationToken token = stateCts.Token;

        try
        {
            monsterSkinFallen.anim.Play(GAnimName.MOVE);

            while (!token.IsCancellationRequested)
            {

                MoveTo(spawnPosition);
                UpdateFacingDirection();
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
        catch (MissingReferenceException)
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

                await AttackAsync(stateCts.Token);

                await UniTask.Delay(TimeSpan.FromSeconds(monsterStats.attackWindow), cancellationToken: token);
                await UniTask.Delay(TimeSpan.FromSeconds(monsterStats.attackRecovery), cancellationToken: token);
            }
        }

        catch (OperationCanceledException)
        {
        }
        catch (MissingReferenceException)
        {

        }
    }

    protected async UniTask OnRollingAttack()
    {
        CancellationToken token = stateCts.Token;

        try
        {
            Vector3 destination = target.position;
            Vector2 attackDirection = destination - transform.position;
            float nextShotTime = Time.time;

            MoveTo(target.position);
            monsterSkinFallen.anim.Play(GAnimName.ROLLING_ATTACK);
            moveSpeed = 4.5f;

            while (!IsArrived(destination))
            {
                token.ThrowIfCancellationRequested();

                if (Time.time >= nextShotTime)
                {
                    await RollingAttackAsync(stateCts.Token);
                    nextShotTime = Time.time + 0.7f;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            moveDestination = null;
            CharacterContactMovement.Stop(movementBody);


            token.ThrowIfCancellationRequested();
            SettingCooldown(3f);
            ChangeState((int)MonsterState.COOLDOWN).Forget();
        }

        catch (OperationCanceledException)
        {
        }
        catch (MissingReferenceException)
        {

        }
    }

    protected async UniTask OnCooldown()
    {
        CancellationToken token = stateCts.Token;

        try
        {
            monsterSkinFallen.anim.Play(GAnimName.IDLE);
            UpdateFacingDirection();
            await UniTask.Delay(TimeSpan.FromSeconds(cooldownTerm), cancellationToken: token);
            token.ThrowIfCancellationRequested();

            ChangeState((int)(CanDetectTarget() ? MonsterState.CHASE : MonsterState.IDLE)).Forget();
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

    protected async UniTask OnSunkenAttack()
    {
        CancellationToken token = stateCts.Token;

        try
        {
            monsterSkinFallen.anim.Play(GAnimName.IDLE);

            for (int i = 0; i < 3; i++)
            {
                GameObject obj = await ObjectPool.Instance.PopFromPool("FallenSunken", parent: null, false);
                obj.transform.position = target.position;
                obj.SetActive(true);

                await UniTask.WaitForSeconds(0.5f);
            }

            token.ThrowIfCancellationRequested();
            SettingCooldown(2f);
            ChangeState((int)MonsterState.COOLDOWN).Forget();
        }

        catch (MissingReferenceException)
        {

        }

        catch (OperationCanceledException)
        {

        }
    }

    protected void SettingCooldown(float cooldownVal)
    {
        this.cooldownTerm = cooldownVal;
    }
}
