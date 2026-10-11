using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public enum MonsterState
{
    IDLE,
    CHASE,
    ATTACK,
    RETURN,
    DEAD,
    SPECIAL_ATTACK,
    SUNKEN_ATTACK,
    COOLDOWN
}

[Serializable]
public class MonsterData
{
    public int monsterId = 0;
    public string prefabName = "";
    public int currentHp = 0;
    public int maxHp = 0;
    public float moveSpeed = 0f;
    public float detectionRange = 0f;
    public float leashRange = 0f;
}

[Serializable]
public class MonsterAttackData
{
    public int monsterId { get; set; }
    public string attackId { get; set; }
    public float weightRate { get; set; }
    public float attackRange { get; set; }
    public float triggerRange { get; set; }
    public float damage { get; set; }
    public float cooldown { get; set; }
    public float attackPreparation { get; set; }
}

[Serializable]
public class MonsterStats
{
    public int currentHp = 40;
    public int maxHp = 100;
    public float attackDamage = 10f;
    public float moveSpeed = 2f;
    public float detectionRange = 20f;
    public float attackRange = 1f;
    public float attackAngle = 2f;
    public float attackPreparation = 0.5f;
    public float attackWindow = 0.001f;
    public float attackRecovery = 0.2f;
    public float leashRange = 3f;
}

[RequireComponent(typeof(PooledObject))]
// XY-plane prototype. Movement intentionally has no pathfinding yet.
public abstract class Monster_000_Base : MonoBehaviour
{
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(monsterData.detectionRange, monsterData.detectionRange, 0f));
    }


    public MonsterData monsterData { get; set; } = new MonsterData();
    public List<MonsterAttackData> monsterAttackDataList { get; set; } = new List<MonsterAttackData>();

    public Transform damageTrf;

    public Transform facingObjectParentTrf;

    public Rigidbody movementBody;

    [SerializeField] protected Transform target;
    [SerializeField] protected LayerMask obstacleLayers;

    protected Vector3 spawnPosition;

    protected MonsterState monsterState = MonsterState.IDLE;
    protected CancellationTokenSource stateCts;

    protected Vector3? moveDestination;
    protected bool isAttack;
    protected bool isDeath;

    public virtual async UniTask Init()
    {
        stateCts = new CancellationTokenSource();
        monsterState = MonsterState.IDLE;
        isDeath = false;
        isAttack = false;
        spawnPosition = transform.position;
        target = IngameSessionManager.Instance.player.GetComponent<Transform>();
        await OnStateChange();
    }

    public void SettingMonsterData(int monsterIdx)
    {
        monsterData = DataManager.Instance.GetData(monsterIdx, DataManager.Instance.monsterDataDic);
        monsterAttackDataList = DataManager.Instance.GetDataList(monsterIdx, DataManager.Instance.monsterAttackDataDic);
    }

    public abstract UniTask ChangeState(int state);

    protected abstract UniTask OnStateChange();

    protected abstract UniTask OnAttack();

    protected abstract UniTask OnDeath();

    public abstract UniTask TakeDamage(int attackDamage);
    protected void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    protected void UpdateFacingDirection()
    {
        if (isAttack || target == null || facingObjectParentTrf == null)
        {
            return;
        }

        Vector3 facingPosition;

        if (monsterState == MonsterState.IDLE || monsterState == MonsterState.RETURN)
        {
            if (!moveDestination.HasValue) return;
            facingPosition = moveDestination.Value;
        }
        else
        {
            if (target == null) return;
            facingPosition = target.position;
        }

        float directionX = facingPosition.x - transform.position.x;
        if (Mathf.Abs(directionX) < 0.001f) return;

        var facingDirection = directionX > 0f ? 1 : -1;

        facingObjectParentTrf.localScale = new Vector3(
            facingDirection,
            facingObjectParentTrf.localScale.y,
            facingObjectParentTrf.localScale.z
        );
    }
    protected bool CanDetectTarget()
    {
        if (target == null)
        {
            return false;
        }

        return Vector2.Distance(transform.position, target.position) <= monsterData.detectionRange;
    }
    protected bool CanAttackTarget(float attackRange = 0)
    {
        if (attackRange == 0) attackRange = monsterAttackDataList[0].attackRange;

        if (target == null)
        {
            return false;
        }

        return Vector2.Distance(transform.position, target.position) <= attackRange;
    }
    protected bool CanMoveCheck(Vector3 destination)
    {
        Vector3 origin = transform.position;
        destination.z = origin.z;

        Vector3 offset = destination - origin;
        float distance = offset.magnitude;

        if (distance > 0.001f)
        {
            Vector3 direction = offset / distance;
            float checkDistance = Mathf.Min(distance, monsterData.moveSpeed * Time.fixedDeltaTime + 1f);

            if (Physics.Raycast(origin, direction, checkDistance, obstacleLayers, QueryTriggerInteraction.Ignore))
            {
                moveDestination = null;
                return false;
            }
        }

        moveDestination = destination;
        return true;
    }
    protected void MoveTo(Vector3 destination)
    {
        moveDestination = new Vector3(destination.x, destination.y, transform.position.z);
    }
    protected bool IsArrived(Vector3 destination)
    {
        return Vector2.Distance(transform.position, destination) <= 0.05f;
    }
    protected Vector3 GetRandomPosition()
    {
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * monsterData.leashRange;
        return spawnPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);
    }
    protected void ResetForPool(Action onReset = null)
    {
        // 진행 중인 상태 행동을 취소한다.
        stateCts?.Cancel();
        stateCts?.Dispose();

        stateCts = null;
        moveDestination = null;

        transform.DOKill();

        onReset?.Invoke();
        isAttack = false;
        target = null;
        isDeath = true;
        monsterState = MonsterState.DEAD;

        ObjectPool.Instance.PushToPool(gameObject);
    }

    protected virtual void OnDestroy()
    {
        stateCts?.Cancel();
        stateCts?.Dispose();
        stateCts = null;
    }
}
