using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum MonsterState
{
    IDLE,
    CHASE,
    ATTACK,
    RETURN,
    DEAD
}

[Serializable]
public class MonsterStats
{
    public int currentHp = 40;
    public float maxHp = 100f;
    public float attackDamage = 10f;
    public float moveSpeed = 2f;
    public float detectionRange = 20f;
    public float attackRange = 0.8f;
    public float attackAngle = 2f;
    public float attackPreparation = 0.5f;
    public float attackWindow = 0.001f;
    public float attackRecovery = 0.2f;
    public float leashRange = 3f;
}

// XY-plane prototype. Movement intentionally has no pathfinding yet.
public abstract class Monster_000_Base : MonoBehaviour
{
    public MonsterStats monsterStats { get; set; } = new MonsterStats();

    public Transform damageTrf;

    public abstract UniTask ChangeState(int state);

    protected abstract UniTask OnStateChange();

    protected abstract UniTask OnAttack();

    protected abstract UniTask OnDeath();

    public abstract UniTask TakeDamage(int attackDamage);
}
