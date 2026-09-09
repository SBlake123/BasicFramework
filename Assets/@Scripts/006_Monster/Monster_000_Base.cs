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
    [Min(1)] public float maxHealth = 10f;
    [Min(0)] public float attackDamage = 10f;
    [Min(0)] public float moveSpeed = 2f;
    [Min(0)] public float detectionRange = 1000f;
    [Min(0)] public float attackRange = 0.8f;
    [Range(1, 360)] public float attackAngle = 2f;
    [Min(0)] public float attackPreparation = 0.5f;
    [Min(.01f)] public float attackWindow = 0.001f;
    [Min(0)] public float attackRecovery = 0.2f;
    [Min(0)] public float leashRange = 3f;
}

// XY-plane prototype. Movement intentionally has no pathfinding yet.
public abstract class Monster_000_Base : MonoBehaviour
{
    public abstract UniTask ChangeState(int state);

    protected abstract UniTask OnStateChange();

    protected abstract UniTask Attack();

    protected abstract UniTask Die();
}
