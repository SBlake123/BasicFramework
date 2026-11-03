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

    public BoxCollider boxCol;

    [SerializeField] private MonsterNavigator navigator;

    private float cooldownTerm;
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
        CharacterContactMovement.Move(movementBody, Vector3.ClampMagnitude(delta / Time.fixedDeltaTime, monsterData.moveSpeed));
    }

    public override async UniTask Init()
    {
        boxCol.enabled = true;
        monsterData.detectionRange = 6f;
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
        try
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

                case MonsterState.COOLDOWN:
                    {
                        //범위안에 들어왔을 때 공격이 가능한 상태일 시 공격한다.
                        //공격이 끝나고 나서는 다시 공격하는 상태인지 판단해야 한다.
                        await OnCooldown();
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

            await UniTask.NextFrame();
        }
        catch (NullReferenceException)
        {

        }

    }

    float resumeMoveTime = 0f;
    bool isWaiting = false;

    private async UniTask OnIdle()
    {
        CancellationToken token = stateCts.Token;

        Vector3 idleDestination = transform.position;
        bool hasDestination = false;

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
                    hasDestination = false;
                }

                if (!hasDestination)
                {
                    hasDestination = TryChooseIdleDestination(out idleDestination);

                    if (hasDestination)
                        monsterSkinSkeleton.anim.Play(GAnimName.SKELETON_MOVE);
                }

                if (!hasDestination || IsArrived(idleDestination))
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

        if (navigator == null)
            navigator = GetComponent<MonsterNavigator>();

        if (navigator == null)
            throw new InvalidOperationException(
                $"{name}: MonsterNavigator가 필요합니다.");

        navigator.ClearPath();

        bool wasMoving = false;
        monsterSkinSkeleton.anim.Play(GAnimName.SKELETON_IDLE);

        try
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                if (target == null ||
                    !target.gameObject.activeInHierarchy ||
                    !CanDetectTarget())
                {
                    ChangeState((int)MonsterState.RETURN).Forget();
                    return;
                }

                // 벽 너머에서는 거리만 보고 공격하지 않음
                if (CanAttackTarget() &&
                    navigator.HasLineOfSight(
                        movementBody.position, target.position))
                {
                    ChangeState((int)MonsterState.ATTACK).Forget();
                    return;
                }

                bool moving = navigator.TryGetNextPoint(
                    movementBody.position,
                    target.position,
                    out Vector3 nextPoint);

                if (moving)
                {
                    MoveTo(nextPoint);
                    UpdateFacingDirection();
                }
                else
                {
                    moveDestination = null;
                    CharacterContactMovement.Stop(movementBody);
                }

                if (moving != wasMoving)
                {
                    monsterSkinSkeleton.anim.Play(
                        moving
                            ? GAnimName.SKELETON_MOVE
                            : GAnimName.SKELETON_IDLE);

                    wasMoving = moving;
                }

                await UniTask.NextFrame(cancellationToken: token);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
    }

    protected override async UniTask OnAttack()
    {
        CancellationToken token = stateCts.Token;

        try
        {
            await UniTask.WaitForSeconds(0.5f, cancellationToken: token);

            monsterSkinSkeleton.anim.Play(GAnimName.ATTACK);

            GameObject obj = await ObjectPool.Instance.PopFromPool(GPrefabName.SKELETON_CLAW, parent: null, false);

            obj.transform.position = transform.position + (target.position - transform.position).normalized;
            obj.SetActive(true);

            token.ThrowIfCancellationRequested();
            SettingCooldown(1f);
            ChangeState((int)MonsterState.COOLDOWN).Forget();
        }

        catch (OperationCanceledException)
        {

        }

        catch (MissingReferenceException)
        {

        }

        void SettingCooldown(float cooldownVal)
        {
            this.cooldownTerm = cooldownVal;
        }
    }

    protected async UniTask OnCooldown()
    {
        CancellationToken token = stateCts.Token;

        try
        {
            monsterSkinSkeleton.anim.Play(GAnimName.SKELETON_IDLE);
            UpdateFacingDirection();
            await UniTask.Delay(TimeSpan.FromSeconds(cooldownTerm), cancellationToken: token);
            token.ThrowIfCancellationRequested();

            ChangeState((int)MonsterState.IDLE).Forget();
        }

        catch (OperationCanceledException)
        {
        }
    }


    private async UniTask OnReturn()
    {
        CancellationToken token = stateCts.Token;

        navigator.ClearPath();

        bool wasMoving = false;
        monsterSkinSkeleton.anim.Play(GAnimName.SKELETON_IDLE);

        try
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                if (IsArrived(spawnPosition))
                {
                    moveDestination = null;
                    CharacterContactMovement.Stop(movementBody);

                    ChangeState((int)MonsterState.IDLE).Forget();
                    return;
                }

                bool moving = navigator.TryGetNextPoint(
                    movementBody.position,
                    spawnPosition,
                    out Vector3 nextPoint);

                if (moving)
                {
                    MoveTo(nextPoint);
                    UpdateFacingDirection();
                }
                else
                {
                    moveDestination = null;
                    CharacterContactMovement.Stop(movementBody);
                }

                if (moving != wasMoving)
                {
                    monsterSkinSkeleton.anim.Play(
                        moving
                            ? GAnimName.SKELETON_MOVE
                            : GAnimName.SKELETON_IDLE);

                    wasMoving = moving;
                }

                await UniTask.NextFrame(cancellationToken: token);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
    }

    protected override async UniTask OnDeath()
    {
        boxCol.enabled = false;

        monsterSkinSkeleton.anim.Play(GAnimName.DEAD);

        DOTween.Sequence().Append(transform.DOShakePosition(0.5f, 0.2f).SetEase(Ease.Linear)).AppendInterval(0.5f).OnComplete(() =>
        {
            ResetForPool(() =>
            {
                if (attackHitBoxTrf != null)
                    attackHitBoxTrf.gameObject.SetActive(false);
            });
        });

        await ItemLootingManager.Instance.MakingItemLootingBox("Box_Common", monsterData.monsterId, transform);

        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: destroyCancellationToken);
    }

    private float wallGap = 0.8f;
    private bool TryGetReachableDestination(Vector3 destination, out Vector3 result)
    {
        Vector3 origin = movementBody.position;
        destination.z = origin.z;
        result = origin;

        Vector3 offset = destination - origin;
        float distance = offset.magnitude;

        if (distance <= 0.05f) return false;

        Vector3 direction = offset / distance;
        Vector3 scale = boxCol.transform.lossyScale;

        Vector3 halfExtents = new Vector3(
            Mathf.Abs(boxCol.size.x * scale.x),
            Mathf.Abs(boxCol.size.y * scale.y),
            Mathf.Abs(boxCol.size.z * scale.z)
        ) * 0.5f;

        Vector3 center = boxCol.transform.TransformPoint(boxCol.center);

        if (Physics.BoxCast(center, halfExtents, direction, out RaycastHit hit, boxCol.transform.rotation, distance, obstacleLayers, QueryTriggerInteraction.Ignore))
        {
            float allowedDistance = Mathf.Max(0f, hit.distance - wallGap);
            result = origin + direction * allowedDistance;
        }
        else
        {
            result = destination;
        }

        return Vector2.Distance(origin, result) > 0.05f;
    }

    private LayerMask monsterLayers;
    private float idleClearance = 0.6f;
    private bool TryChooseIdleDestination(out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 candidate = GetRandomPosition();

            if (!TryGetReachableDestination(candidate, out result)) continue;

            Collider[] nearby = Physics.OverlapSphere(result, idleClearance, monsterLayers, QueryTriggerInteraction.Ignore);

            bool occupied = false;

            foreach (Collider col in nearby)
            {
                if (col.transform == transform || col.transform.IsChildOf(transform)) continue;

                occupied = true;
                break;
            }

            if (!occupied) return true;
        }

        result = transform.position;
        return false;
    }
}
