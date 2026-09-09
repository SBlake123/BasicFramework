using UnityEngine;

public interface IMonsterState
{
    void Enter(Monster monster);
    void Tick(Monster monster, float deltaTime);
    void Exit(Monster monster);
}

public abstract class MonsterStateBase : IMonsterState
{
    public virtual void Enter(Monster monster) { }
    public abstract void Tick(Monster monster, float deltaTime);
    public virtual void Exit(Monster monster) { }
}

public sealed class MonsterIdleState : MonsterStateBase
{
    public override void Tick(Monster monster, float deltaTime)
    {
        if (monster.HasTarget && monster.TargetOffset.magnitude <= monster.stats.detectionRange)
            monster.ChangeState(MonsterState.Chase);
    }
}

public sealed class MonsterChaseState : MonsterStateBase
{
    public override void Tick(Monster monster, float deltaTime)
    {
        if (!monster.HasTarget || monster.OutsideLeash)
        {
            monster.ChangeState(MonsterState.Return);
            return;
        }
        if (monster.TargetOffset.magnitude <= monster.stats.attackRange)
        {
            monster.ChangeState(MonsterState.Attack);
            return;
        }
        monster.MoveTowards(monster.Target.position, deltaTime);
    }
}

public sealed class MonsterAttackState : MonsterStateBase
{
    private float elapsed;
    private bool hit;
    private Transform lockedTarget;

    public override void Enter(Monster monster)
    {
        elapsed = 0;
        hit = false;
        lockedTarget = monster.Target;
        monster.AttackDirection = monster.TargetOffset.normalized;
        monster.onAttackStarted.Invoke();
    }

    public override void Tick(Monster monster, float deltaTime)
    {
        if (!monster.HasTarget || monster.Target != lockedTarget)
        {
            monster.ChangeState(MonsterState.Return);
            return;
        }
        float previous = elapsed;
        elapsed += deltaTime;
        float start = monster.stats.attackPreparation;
        float end = start + monster.stats.attackWindow;
        // Resolve even if a slow frame spans the entire active window.
        if (!hit && elapsed >= start && previous < end && monster.CanHitTarget())
        {
            hit = true;
            monster.onAttackHit.Invoke(lockedTarget, monster.stats.attackDamage);
            if (monster.CurrentState != MonsterState.Attack) return;
        }
        if (elapsed >= end + monster.stats.attackRecovery)
            monster.ChangeState(monster.OutsideLeash ? MonsterState.Return : MonsterState.Chase);
    }

    public override void Exit(Monster monster) { lockedTarget = null; }
}

public sealed class MonsterReturnState : MonsterStateBase
{
    public override void Tick(Monster monster, float deltaTime)
    {
        monster.MoveTowards(monster.Home, deltaTime);
        if (Vector2.Distance(monster.transform.position, monster.Home) <= .05f)
            monster.ChangeState(MonsterState.Idle);
    }
}

public sealed class MonsterDeadState : MonsterStateBase
{
    public override void Enter(Monster monster) { monster.onDied.Invoke(); }
    public override void Tick(Monster monster, float deltaTime) { }
}
