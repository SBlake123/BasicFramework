using Cysharp.Threading.Tasks;
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
    private MonsterState monsterState = MonsterState.Idle;

    private CancellationTokenSource stateCts;

    private void Awake()
    {
        MonsterInit().Forget();
    }

    async UniTask MonsterInit()
    {
        await ChangeState((int)monsterState);
    }

    public override async UniTask ChangeState(int state)
    {
        if (monsterState != (MonsterState)state)
        {
            monsterState = (MonsterState)state;

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
                }
                break;
            case MonsterState.Chase:
                {
                    //적의 시야에 플레이어가 들어왔을 때 인지하고 공격을 시도한다.
                }
                break;
            case MonsterState.Attack:
                {
                    //범위안에 들어왔을 때 공격이 가능한 상태일 시 공격한다.
                    //공격이 끝나고 나서는 다시 공격하는 상태인지 판단해야 한다.
                }
                break;
            case MonsterState.Return:
                {
                    //Return되는 경우는 플레이어가 사라진 상태에서 Chase 범위를 벗어났을 때 뿐.
                }
                break;
            case MonsterState.Dead:
                {
                    //죽는 단계는 끝이기 때문에 연결할 필요가 없다.
                }
                break;
        }

        await UniTask.WaitForFixedUpdate();
    }

    protected override async UniTask Attack()
    {
        
    }

    protected override async UniTask Die()
    {
        
    }

}
