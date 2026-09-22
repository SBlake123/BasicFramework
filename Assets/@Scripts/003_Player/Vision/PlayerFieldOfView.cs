using System.Collections.Generic;
using UnityEngine;

public class PlayerFieldOfView : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Transform visionOrigin;
    private const float NearViewRadius = 1f;
    private const float ViewDistance = 30f;
    private const float ViewAngle = 130f;
    private const int RayCount = 360;
    private const int EdgeRefinementSteps = 5;
    private const float EdgeDistanceThreshold = 0.5f;
    private const float ConeEdgeOffset = 0.001f;
    [SerializeField] private LayerMask obstacleMask;

    private Mesh viewMesh;
    private readonly List<float> rayAngles = new List<float>(RayCount + 5);
    private readonly List<RaySample> raySamples = new List<RaySample>(RayCount + 32);
    private readonly List<Vector3> vertices = new List<Vector3>(RayCount + 34);
    private readonly List<int> triangles = new List<int>(RayCount * 3 + 96);

    private struct RaySample
    {
        public Vector3 point;
        public float distance;
        public float maxDistance;
        public bool hit;

        public RaySample(Vector3 point, float distance, float maxDistance, bool hit)
        {
            this.point = point;
            this.distance = distance;
            this.maxDistance = maxDistance;
            this.hit = hit;
        }
    }

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
        rayAngles.Clear();
        for (int i = 0; i <= RayCount; i++)
        {
            rayAngles.Add(360f * i / RayCount);
        }

        AddConeEdgeAngles(facingAngle - ViewAngle * 0.5f);
        AddConeEdgeAngles(facingAngle + ViewAngle * 0.5f);
        rayAngles.Sort();

        raySamples.Clear();
        RaySample previous = CastRay(origin, rayAngles[0], facingAngle);
        raySamples.Add(previous);
        for (int i = 1; i < rayAngles.Count; i++)
        {
            RaySample current = CastRay(origin, rayAngles[i], facingAngle);
            RefineEdge(origin, facingAngle, rayAngles[i - 1], previous, rayAngles[i], current, EdgeRefinementSteps);
            raySamples.Add(current);
            previous = current;
        }

        vertices.Clear();
        triangles.Clear();
        vertices.Add(transform.InverseTransformPoint(origin));
        foreach (RaySample sample in raySamples)
        {
            vertices.Add(transform.InverseTransformPoint(sample.point));
        }

        for (int i = 0; i < raySamples.Count - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i + 2);
            triangles.Add(i + 1);
        }

        viewMesh.Clear();
        viewMesh.SetVertices(vertices);
        viewMesh.SetTriangles(triangles, 0);
        viewMesh.RecalculateBounds();
    }

    private void AddConeEdgeAngles(float angle)
    {
        rayAngles.Add(Mathf.Repeat(angle - ConeEdgeOffset, 360f));
        rayAngles.Add(Mathf.Repeat(angle + ConeEdgeOffset, 360f));
    }

    private RaySample CastRay(Vector3 origin, float angle, float facingAngle)
    {
        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(facingAngle, angle));
        float maxDistance = angleDifference <= ViewAngle * 0.5f ? ViewDistance : NearViewRadius;
        Vector3 direction = GetDirection(angle);
        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            return new RaySample(hit.point, hit.distance, maxDistance, true);
        }

        return new RaySample(origin + direction * maxDistance, maxDistance, maxDistance, false);
    }

    private void RefineEdge(Vector3 origin, float facingAngle, float leftAngle, RaySample left,
        float rightAngle, RaySample right, int stepsRemaining)
    {
        if (stepsRemaining == 0 || left.maxDistance != right.maxDistance ||
            (left.hit == right.hit && Mathf.Abs(left.distance - right.distance) < EdgeDistanceThreshold))
        {
            return;
        }

        float middleAngle = (leftAngle + rightAngle) * 0.5f;
        RaySample middle = CastRay(origin, middleAngle, facingAngle);
        RefineEdge(origin, facingAngle, leftAngle, left, middleAngle, middle, stepsRemaining - 1);
        raySamples.Add(middle);
        RefineEdge(origin, facingAngle, middleAngle, middle, rightAngle, right, stepsRemaining - 1);
    }

    private Vector3 GetDirection(float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0f);
    }
}
