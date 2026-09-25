using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnArea : MonoBehaviour
{

    //[SerializeField] public int areaNumber = 0;
    //[SerializeField] public int spawnPointNumber = 0;
    [SerializeField] private Vector2 size = new Vector2(0f, 0f);
    [SerializeField] private LayerMask spawnBlockingMask;
    public Transform spawnAreaTrf;
    private float checkRadius = 0.5f;
    private int maxAttempts = 30;

    public bool TryGetSpawnPosition(out Vector3 position)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 candidate = GetRandomPosition();

            bool overlaps = Physics.CheckSphere(candidate, checkRadius, spawnBlockingMask, QueryTriggerInteraction.Ignore);

            if (overlaps)
                continue;

            position = candidate;
            return true;
        }

        // 모든 시도에서 빈 공간을 찾지 못했다.
        position = default;
        return false;
    }

    public Vector3 GetRandomPosition()
    {
        Vector3 localPosition = new Vector3(
            Random.Range(-size.x * 0.5f, size.x * 0.5f),
            Random.Range(-size.y * 0.5f, size.y * 0.5f),
            0f
        );

        return transform.TransformPoint(localPosition);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(size.x, size.y, 0f));
    }
}
