using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnArea : MonoBehaviour
{
    [SerializeField] private Vector2 size = new Vector2(5f, 3f);

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
