using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(PooledObject))]
public class CrawlingFlame : MonoBehaviour
{
    public LayerMask hitLayers;

    public Animator anim;

    public BoxCollider col;

    public void OnEnable()
    {
        PlaySequenceAsync().Forget();
    }

    public Transform target;
    public Transform flameBody;
    public Transform boom;

    public bool isMoving = true;
    public float moveSpeed { get; set; } = 8f;
    private Tween swayTween;


    [SerializeField] private float turnSpeed = 6f;

    private Vector3 forward;
    private Vector3 moveDirection;
    private Vector3 wanderDirection;
    private float wanderTimer;
    private float currentSpeed;
    private float desiredSpeed;
    private float currentSideSpeed;
    private float desiredSideSpeed;
    private float sideTimer;
    private float sideSpeed = 4f;

    private float detectionRange = 20f;
    public LayerMask monsterLayers;
    public void Init(Vector2 lookDirection)
    {
        forward = lookDirection.sqrMagnitude > 0.001f ? new Vector3(lookDirection.x, lookDirection.y, 0f).normalized : Vector3.right;

        isMoving = true;
        boom.gameObject.SetActive(false);
        moveDirection = forward;
        wanderDirection = forward;
        currentSpeed = moveSpeed;
        desiredSpeed = moveSpeed;
        currentSideSpeed = 0f;
        desiredSideSpeed = 0f;
        sideTimer = 0f;
        wanderTimer = 0f;
        FindInitialTarget();

    }

    private void FindInitialTarget()
    {
        target = null;
        float closestDistance = float.MaxValue;

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, monsterLayers, QueryTriggerInteraction.Collide);

        foreach (Collider hit in hits)
        {
            var monster = hit.GetComponentInParent<Monster_000_Base>();
            if (monster == null || !monster.gameObject.activeInHierarchy) continue;

            float distance = (monster.transform.position - transform.position).sqrMagnitude;
            if (distance >= closestDistance) continue;

            closestDistance = distance;
            target = monster.transform;
        }
    }

    private void Update()
    {
        if(isMoving)
        {
            float dt = Time.deltaTime;
            wanderTimer -= dt;

            if (wanderTimer <= 0f)
            {
                wanderTimer = Random.Range(0.2f, 0.7f);
                float angle = Random.Range(-55f, 55f);

                wanderDirection = Quaternion.Euler(0f, 0f, angle) * forward;
                desiredSpeed = moveSpeed * Random.Range(0.6f, 1.4f);
            }

            Vector3 desiredDirection = wanderDirection;

            if (target != null && target.gameObject.activeInHierarchy)
            {
                desiredDirection = target.position - transform.position;
                desiredDirection.z = 0f;
                desiredDirection.Normalize();
            }

            moveDirection = Vector3.RotateTowards(moveDirection, desiredDirection, turnSpeed * dt, 0f);
            currentSpeed = Mathf.Lerp(currentSpeed, desiredSpeed, 1f - Mathf.Exp(-4f * dt));

            sideTimer -= dt;

            if (sideTimer <= 0f)
            {
                sideTimer = Random.Range(0.15f, 0.6f);
                desiredSideSpeed = Random.Range(-sideSpeed, sideSpeed);
            }

            currentSideSpeed = Mathf.Lerp(currentSideSpeed, desiredSideSpeed, 1f - Mathf.Exp(-6f * dt));

            Vector3 sideDirection = new Vector3(-moveDirection.y, moveDirection.x, 0f);
            transform.position += (moveDirection * currentSpeed + sideDirection * currentSideSpeed) * dt;
        }

    }

    private void OnDisable()
    {
        swayTween?.Kill();
        target = null;
    }

    private async UniTask PlaySequenceAsync()
    {
        var token = this.GetCancellationTokenOnDestroy();

        await PlayAndWaitAsync("Start", token);
        await PlayAndWaitAsync("Burn", token);
    }

    private async UniTask PlayAndWaitAsync(string stateName, CancellationToken token)
    {
        anim.Play(stateName, 0, 0f);

        // Animator가 재생 상태를 반영할 때까지 대기
        await UniTask.Yield(PlayerLoopTiming.Update, token);

        await UniTask.WaitUntil(() =>
        {
            var state = anim.GetCurrentAnimatorStateInfo(0);
            return state.IsName(stateName) && state.normalizedTime >= 1f;
        }, cancellationToken: token);
    }
}
