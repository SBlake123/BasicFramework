using System.Collections.Generic;
using UnityEngine;

public sealed class MonsterNavigator : MonoBehaviour
{
    [SerializeField] private GridPathfinder pathfinder;
    [SerializeField, Min(0.05f)] private float repathInterval = 0.4f;
    [SerializeField, Min(0.05f)] private float targetMovementThreshold = 0.5f;
    [SerializeField, Min(0.02f)] private float waypointArrivalDistance = 0.15f;

    private readonly List<Vector3> waypoints = new List<Vector3>(32);

    private int waypointIndex;
    private bool hasPath;
    private float nextRepathTime;
    private Vector3 lastGoal;

    public void ClearPath()
    {
        waypoints.Clear();
        waypointIndex = 0;
        hasPath = false;
        nextRepathTime = 0f;
    }

    public bool TryGetNextPoint(Vector3 currentPosition, Vector3 targetPosition, out Vector3 nextPoint)
    {
        nextPoint = currentPosition;

        if (pathfinder == null) pathfinder = GridPathfinder.Instance;
        if (pathfinder == null) return false;

        bool moved = (targetPosition - lastGoal).sqrMagnitude >= targetMovementThreshold * targetMovementThreshold;

        if (Time.time >= nextRepathTime && (!hasPath || moved))
        {
            nextRepathTime = Time.time + repathInterval;
            lastGoal = targetPosition;

            hasPath = pathfinder.TryFindPath(currentPosition, targetPosition, waypoints);
            waypointIndex = 0;
        }

        if (!hasPath) return false;

        while (waypointIndex < waypoints.Count && Vector2.Distance(currentPosition, waypoints[waypointIndex]) <= waypointArrivalDistance)
            waypointIndex++;

        if (waypointIndex >= waypoints.Count) return false;

        nextPoint = waypoints[waypointIndex];
        return true;
    }
}