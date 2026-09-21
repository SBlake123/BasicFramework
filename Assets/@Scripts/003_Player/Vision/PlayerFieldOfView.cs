using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFieldOfView : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Transform visionOrigin;
    private float nearViewRadius = 1f;
    private float viewDistance = 30f;
     private float viewAngle = 130f;
    private int rayCount = 80;
    [SerializeField] private LayerMask obstacleMask;


    private Mesh viewMesh;

    private void Awake()
    {
        viewMesh = new Mesh { name = "Player View Mesh" };
        GetComponent<MeshFilter>().mesh = viewMesh;
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sortingLayerName = "Default";
        meshRenderer.sortingOrder = 100;
    }

    private void LateUpdate()
    {
        if (player == null || visionOrigin == null) return;
        CreateViewMesh(player.lastLookDirection);
    }

    private void CreateViewMesh(Vector2 facingDirection)
    {
        Vector3 origin = visionOrigin.position;
        float facingAngle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = transform.InverseTransformPoint(origin);

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = 360f * i / rayCount;
            float angleDifference = Mathf.Abs(Mathf.DeltaAngle(facingAngle, angle));
            float distance = angleDifference <= viewAngle * 0.5f ? viewDistance : nearViewRadius;
            Vector3 direction = GetDirection(angle);
            Vector3 point = Physics.Raycast(origin, direction, out RaycastHit hit, distance, obstacleMask, QueryTriggerInteraction.Ignore) ? hit.point : origin + direction * distance;
            vertices[i + 1] = transform.InverseTransformPoint(point);
        }

        for (int i = 0; i < rayCount; i++)
        {
            int index = i * 3;
            triangles[index] = 0;
            triangles[index + 1] = i + 2;
            triangles[index + 2] = i + 1;
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateBounds();
    }

    private Vector3 GetDirection(float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0f);
    }
}
