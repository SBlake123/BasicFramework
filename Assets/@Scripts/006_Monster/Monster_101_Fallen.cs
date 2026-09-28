using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DG.Tweening;

public partial class Monster_101_Fallen : Monster_000_Base
{
    

    private readonly Dictionary<MonsterState, IMonsterState> stateMap = new()
    {
        { MonsterState.IDLE, new IdleState() },
        { MonsterState.CHASE, new DodgeState() },
        { MonsterState.ATTACK, new AttackState() },
        { MonsterState.RETURN, new ReturnState() },
        { MonsterState.DEAD, new DeadState() }
    };

   

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
