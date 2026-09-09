using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum MonsterState { Idle, Chase, Attack, Return, Dead }

[Serializable]
public class MonsterStats
{
    [Min(1)] public float maxHealth = 75f;
    [Min(0)] public float attackDamage = 15f;
    [Min(0)] public float moveSpeed = 2.25f;
    [Min(0)] public float detectionRange = 4f;
    [Min(0)] public float attackRange = .9f;
    [Range(1, 360)] public float attackAngle = 90f;
    [Min(0)] public float attackPreparation = .6f;
    [Min(.01f)] public float attackWindow = .1f;
    [Min(0)] public float attackRecovery = 1f;
    [Min(0)] public float leashRange = 6f;
}

[Serializable]
public class MonsterHitEvent : UnityEvent<Transform, float> { }

// XY-plane prototype. Movement intentionally has no pathfinding yet.
public class Monster : MonoBehaviour
{
    public MonsterStats stats = new MonsterStats();
    [SerializeField] private Transform target;
    [SerializeField] private bool running = true;
    [SerializeField] private MonsterState currentState;
    [SerializeField] private float health;
    [Header("Connect combat / animation / loot here")]
    public MonsterHitEvent onAttackHit = new MonsterHitEvent();
    public UnityEvent onAttackStarted = new UnityEvent();
    public UnityEvent onDamaged = new UnityEvent();
    public UnityEvent onDied = new UnityEvent();

    public event Action<MonsterState> StateChanged;
    public MonsterState CurrentState => currentState;
    public float Health => health;
    public bool IsDead => health <= 0f;
    public Transform Target => target;
    public Vector3 Home { get; private set; }
    public Vector2 AttackDirection { get; internal set; }
    public bool HasTarget => target != null && target.gameObject.activeInHierarchy;
    public bool OutsideLeash => Vector2.Distance(Home, transform.position) > stats.leashRange;
    public Vector2 TargetOffset => HasTarget ? (Vector2)(target.position - transform.position) : Vector2.zero;

    private Dictionary<MonsterState, IMonsterState> states;
    private IMonsterState activeState;

    private void Awake()
    {
        health = Mathf.Max(1f, stats.maxHealth);
        Home = transform.position;
        states = new Dictionary<MonsterState, IMonsterState>
        {
            { MonsterState.Idle, new MonsterIdleState() },
            { MonsterState.Chase, new MonsterChaseState() },
            { MonsterState.Attack, new MonsterAttackState() },
            { MonsterState.Return, new MonsterReturnState() },
            { MonsterState.Dead, new MonsterDeadState() }
        };
        ChangeState(MonsterState.Idle);
    }

    private void Update()
    {
        if (running && !IsDead) activeState?.Tick(this, Time.deltaTime);
    }

    private void OnDisable()
    {
        // Interrupt an unfinished attack when pooled/hidden, without reviving death.
        if (states != null && !IsDead) ChangeState(MonsterState.Idle);
    }

    public void SetTarget(Transform value) { target = value; }
    public void SetRunning(bool value) { running = value; }

    public void ChangeState(MonsterState next)
    {
        if (states == null || (activeState != null && currentState == next)) return;
        if (IsDead && next != MonsterState.Dead) return;
        activeState?.Exit(this);
        currentState = next;
        activeState = states[next];
        if (next == MonsterState.Dead) health = 0;
        activeState.Enter(this);
        StateChanged?.Invoke(currentState);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0) return;
        health = Mathf.Max(0, health - amount);
        if (IsDead) ChangeState(MonsterState.Dead);
        else onDamaged.Invoke();
    }

    internal void MoveTowards(Vector3 destination, float deltaTime)
    {
        destination.z = transform.position.z;
        transform.position = Vector3.MoveTowards(transform.position, destination,
            Mathf.Max(0, stats.moveSpeed) * deltaTime);
    }

    internal bool CanHitTarget()
    {
        if (!HasTarget) return false;
        Vector2 offset = TargetOffset;
        return offset.magnitude <= stats.attackRange &&
            (offset.sqrMagnitude < .0001f || Vector2.Angle(AttackDirection, offset) <= stats.attackAngle * .5f);
    }
}
