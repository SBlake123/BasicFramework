using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DG.Tweening;

public partial class Monster_101_Fallen : Monster_000_Base
{
    protected override async UniTask OnStateChange()
    {

    }
    public override async UniTask ChangeState(int state)
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

    public override async UniTask Init()
    {
        
    }

    public override async UniTask TakeDamage(int attackDamage)
    {
        
    }

    protected async UniTask OnIdle()
    {

    }

    protected async UniTask OnChase()
    {

    }

    protected async UniTask OnReturn()
    {

    }

    protected override async UniTask OnAttack()
    {
        
    }

    protected override async UniTask OnDeath()
    {
       
    }

}
