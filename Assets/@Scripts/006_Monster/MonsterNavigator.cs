using System.Collections.Generic;
using UnityEngine;

public class MonsterNavigator : MonoBehaviour
{
    [Header("장애물 검사")]
    [SerializeField] private LayerMask obstacleLayers;

    // 이동용 콜라이더의 월드 크기 절반
    [SerializeField]
    private Vector3 halfSize =
        new Vector3(0.35f, 0.35f, 0.25f);

    // 몬스터 루트 → 이동용 콜라이더 중심
    [SerializeField] private Vector3 centerOffset;

    [Header("경로 탐색")]
    [SerializeField, Min(0.1f)] private float cellSize = 0.5f;
    [SerializeField, Min(0.1f)] private float repathInterval = 0.5f;
    [SerializeField, Min(16)] private int maxSearchNodes = 512;

    private readonly List<Vector3> path = new();
    private int pathIndex;
    private float nextSearchTime;

    private class Node
    {
        public Vector2Int cell;
        public int cost = int.MaxValue;
        public int heuristic;
        public Node parent;
        public bool closed;
        public bool walkable;

        public int Score => cost + heuristic;
    }

    private void OnDisable()
    {
        ClearPath();
    }

    public void ClearPath()
    {
        path.Clear();
        pathIndex = 0;
        nextSearchTime = 0f;
    }

    // 공격할 때 벽 너머인지 확인하는 용도
    public bool HasLineOfSight(Vector3 from, Vector3 to)
    {
        to.z = from.z;

        return !Physics.Linecast(
            from + centerOffset,
            to + centerOffset,
            obstacleLayers,
            QueryTriggerInteraction.Ignore);
    }

    public bool TryGetNextPoint(
        Vector3 current,
        Vector3 target,
        out Vector3 nextPoint)
    {
        target.z = current.z;
        nextPoint = current;

        // 매 프레임 탐색하지 않고 일정 간격으로 갱신
        if (Time.time >= nextSearchTime)
        {
            nextSearchTime = Time.time + repathInterval;
            path.Clear();
            pathIndex = 0;

            if (CanTravel(current, target))
            {
                path.Add(target);
            }
            else
            {
                FindPath(current, target);
            }
        }

        while (pathIndex < path.Count &&
               Vector2.Distance(current, path[pathIndex]) <= 0.05f)
        {
            pathIndex++;
        }

        if (pathIndex >= path.Count)
            return false;

        // 직접 이동 가능한 다음 경유지는 건너뛰기
        while (pathIndex + 1 < path.Count &&
               CanTravel(current, path[pathIndex + 1]))
        {
            pathIndex++;
        }

        // 이동 중 장애물이 생겼다면 다음 재탐색까지 정지
        if (!CanTravel(current, path[pathIndex]))
        {
            path.Clear();
            return false;
        }

        nextPoint = path[pathIndex];
        return true;
    }

    private bool IsFree(Vector3 position)
    {
        return !Physics.CheckBox(
            position + centerOffset,
            halfSize,
            Quaternion.identity,
            obstacleLayers,
            QueryTriggerInteraction.Ignore);
    }

    private bool CanTravel(Vector3 from, Vector3 to)
    {
        if (!IsFree(from) || !IsFree(to))
            return false;

        Vector3 delta = to - from;
        float distance = delta.magnitude;

        if (distance <= 0.001f)
            return true;

        return !Physics.BoxCast(
            from + centerOffset,
            halfSize,
            delta / distance,
            Quaternion.identity,
            distance,
            obstacleLayers,
            QueryTriggerInteraction.Ignore);
    }

    private bool FindPath(Vector3 origin, Vector3 target)
    {
        if (!IsFree(origin) || !IsFree(target))
            return false;

        // 현재 몬스터 위치를 임시 격자의 원점으로 사용
        Vector2Int goalCell = new Vector2Int(
            Mathf.RoundToInt((target.x - origin.x) / cellSize),
            Mathf.RoundToInt((target.y - origin.y) / cellSize));

        var nodes = new Dictionary<Vector2Int, Node>();
        var open = new List<Node>();

        Vector3 ToWorld(Vector2Int cell)
        {
            return origin + new Vector3(
                cell.x * cellSize,
                cell.y * cellSize,
                0f);
        }

        Node GetNode(Vector2Int cell)
        {
            if (nodes.TryGetValue(cell, out Node existing))
                return existing;

            int dx = Mathf.Abs(cell.x - goalCell.x);
            int dy = Mathf.Abs(cell.y - goalCell.y);

            var node = new Node
            {
                cell = cell,
                heuristic = 10 * (dx + dy) - 6 * Mathf.Min(dx, dy),
                walkable = IsFree(ToWorld(cell))
            };

            nodes.Add(cell, node);
            return node;
        }

        Node start = GetNode(Vector2Int.zero);
        Node goal = GetNode(goalCell);

        // 목표 주변 격자 중심이 막혀 있으면 이번 탐색은 실패
        if (!goal.walkable ||
            !CanTravel(ToWorld(goalCell), target))
            return false;

        start.cost = 0;
        open.Add(start);

        int searched = 0;

        while (open.Count > 0 && searched < maxSearchNodes)
        {
            int bestIndex = 0;

            for (int i = 1; i < open.Count; i++)
            {
                Node candidate = open[i];
                Node best = open[bestIndex];

                if (candidate.Score < best.Score ||
                    (candidate.Score == best.Score &&
                     candidate.heuristic < best.heuristic))
                {
                    bestIndex = i;
                }
            }

            Node current = open[bestIndex];
            open.RemoveAt(bestIndex);
            current.closed = true;
            searched++;

            if (current == goal)
            {
                for (Node node = goal; node.parent != null; node = node.parent)
                    path.Add(ToWorld(node.cell));

                path.Reverse();
                path.Add(target);
                return true;
            }

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (x == 0 && y == 0) continue;

                    Vector2Int nextCell =
                        current.cell + new Vector2Int(x, y);

                    Node next = GetNode(nextCell);

                    if (!next.walkable || next.closed)
                        continue;

                    // 대각선으로 벽 모서리를 통과하지 않도록 검사
                    if (x != 0 && y != 0)
                    {
                        Node horizontal = GetNode(
                            current.cell + new Vector2Int(x, 0));

                        Node vertical = GetNode(
                            current.cell + new Vector2Int(0, y));

                        if (!horizontal.walkable || !vertical.walkable)
                            continue;
                    }

                    int nextCost =
                        current.cost + (x == 0 || y == 0 ? 10 : 14);

                    if (nextCost >= next.cost)
                        continue;

                    // 칸 사이의 얇은 벽도 검사
                    if (!CanTravel(ToWorld(current.cell), ToWorld(nextCell)))
                        continue;

                    next.cost = nextCost;
                    next.parent = current;

                    if (!open.Contains(next))
                        open.Add(next);
                }
            }
        }

        return false;
    }
}