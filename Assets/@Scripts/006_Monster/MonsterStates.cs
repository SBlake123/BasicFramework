using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public interface IMonsterState
{
    UniTask EnterAsync(CancellationToken token);
}
public class IdleState : IMonsterState
{
    public async UniTask EnterAsync(CancellationToken token)
    {

    }
}

public class AttackState : IMonsterState
{
    public async UniTask EnterAsync(CancellationToken token)
    {
        //Attack에 관한 메소드

    }
}

public class DodgeState : IMonsterState
{
    public async UniTask EnterAsync(CancellationToken token)
    {

    }
}

public class DeadState : IMonsterState
{
    public async UniTask EnterAsync(CancellationToken token)
    {

    }
}

public class ReturnState : IMonsterState
{
    public async UniTask EnterAsync(CancellationToken token)
    {

    }
}