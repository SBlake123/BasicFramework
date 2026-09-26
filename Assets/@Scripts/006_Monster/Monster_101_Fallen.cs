using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DG.Tweening;

public partial class Monster_101_Fallen : Monster_000_Base
{
    public interface IMonsterState
    {
        UniTask EnterAsync(Player player, CancellationToken token);
    }

    private readonly Dictionary<MonsterState, IMonsterState> stateMap = new()
    {
        { MonsterState.IDLE, new IdleState() },
        { MonsterState.CHASE, new DodgeState() },
        { MonsterState.ATTACK, new AttackState() },
        { MonsterState.RETURN, new ReturnState() },
        { MonsterState.DEAD, new DeadState() }
    };

    public class IdleState : IMonsterState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {
            
        }
    }

    public class AttackState : IMonsterState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {
            //Attack에 관한 메소드

        }
    }

    public class DodgeState : IMonsterState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {

        }
    }

    public class DeadState : IMonsterState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {

        }
    }

    public class ReturnState : IMonsterState
    {
        public async UniTask EnterAsync(Player player, CancellationToken token)
        {

        }
    }

    public override async UniTask ChangeState(int state)
    {
        
    }

    public override async UniTask Init()
    {
        
    }

    public override async UniTask TakeDamage(int attackDamage)
    {
        
    }

    protected override async UniTask OnAttack()
    {
        
    }

    protected override async UniTask OnDeath()
    {
       
    }

    protected override async UniTask OnStateChange()
    {
        
    }

}
