using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum MonsterState 
{ 
    Idle, 
    Chase, 
    Attack, 
    Return, 
    Dead
}

[Serializable]
public class MonsterStats
{
    [Min(1)] public float maxHealth;
    [Min(0)] public float attackDamage;
    [Min(0)] public float moveSpeed;
    [Min(0)] public float detectionRange;
    [Min(0)] public float attackRange;
    [Range(1, 360)] public float attackAngle;
    [Min(0)] public float attackPreparation;
    [Min(.01f)] public float attackWindow;
    [Min(0)] public float attackRecovery;
    [Min(0)] public float leashRange;
}

// XY-plane prototype. Movement intentionally has no pathfinding yet.
public abstract class Monster_000_Base : MonoBehaviour
{
    public abstract UniTask ChangeState(int state);

    protected abstract UniTask OnStateChange();

    protected abstract UniTask Attack();

    protected abstract UniTask Die();
}
