
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class GridPathfinder : MonoBehaviour
{
    public static GridPathfinder Instance { get; private set; }

    [SerializeField] private Vector2Int gridSize = new Vector2Int(40, 40);
    [SerializeField, Min(0.1f)] private float cellSize = 0.5f;
    [SerializeField] private Vector3 agentHalfExtents = new Vector3(0.28f, 0.28f, 0.3f);
    [SerializeField] private LayerMask staticObstacleLayers;
    [SerializeField] private bool drawGizmos = true;

    private int width, height;
    private bool[] walkable;
    private int[] gCost, parent, openedStamp, closedStamp, heap, heapPosition;
    private int searchStamp, heapCount, searchGoal;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Only one GridPathfinder should exist.", this);
            enabled = false;
            return;
        }

        Instance = this;
        BakeGrid();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void BakeGrid()
    {
        width = Mathf.Max(1, gridSize.x);
        height = Mathf.Max(1, gridSize.y);

        int count = width * height;

        walkable = new bool[count];
        gCost = new int[count];
        parent = new int[count];
        openedStamp = new int[count];
        closedStamp = new int[count];
        heap = new int[count];
        heapPosition = new int[count];

        searchStamp = 0;

        for (int i = 0; i < count; i++)
        {
            walkable[i] = !Physics.CheckBox(IndexToWorld(i, transform.position.z), agentHalfExtents, Quaternion.identity, staticObstacleLayers, QueryTriggerInteraction.Ignore);
        }
    }

    public bool TryFindPath(Vector3 start, Vector3 goal, List<Vector3> result)
    {
        result.Clear();

        if (walkable == null) BakeGrid();

        if (!TryWorldToIndex(start, out int startIndex) || !TryWorldToIndex(goal, out int goalIndex)) return false;
        if (!walkable[startIndex] || !walkable[goalIndex]) return false;

        Vector3 exactGoal = new Vector3(goal.x, goal.y, start.z);

        if (startIndex == goalIndex)
        {
            if (!CanMoveDirectly(start, exactGoal)) return false;

            result.Add(exactGoal);
            return true;
        }

        BeginSearch(goalIndex);

        gCost[startIndex] = 0;
        openedStamp[startIndex] = searchStamp;
        PushHeap(startIndex);

        while (heapCount > 0)
        {
            int current = PopHeap();

            if (current == goalIndex)
            {
                ReconstructPath(startIndex, goalIndex, start.z, result);

                Vector3 from = result[result.Count - 1];

                if (CanMoveDirectly(from, exactGoal))
                    result.Add(exactGoal);

                return true;
            }

            closedStamp[current] = searchStamp;

            int cx = current % width;
            int cy = current / width;

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = cx + dx;
                    int ny = cy + dy;

                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

                    int next = nx + ny * width;

                    if (!walkable[next] || closedStamp[next] == searchStamp) continue;

                    // 대각선에서 벽 모서리 통과 방지
                    if (dx != 0 && dy != 0)
                    {
                        if (!walkable[cx + dx + cy * width] || !walkable[cx + (cy + dy) * width])
                            continue;
                    }

                    int nextCost = gCost[current] + (dx == 0 || dy == 0 ? 10 : 14);

                    if (openedStamp[next] == searchStamp && nextCost >= gCost[next]) continue;

                    parent[next] = current;
                    gCost[next] = nextCost;

                    if (openedStamp[next] != searchStamp)
                    {
                        openedStamp[next] = searchStamp;
                        PushHeap(next);
                    }
                    else
                    {
                        SiftUp(heapPosition[next]);
                    }
                }
            }
        }

        return false;
    }

    private bool TryWorldToIndex(Vector3 world, out int index)
    {
        Vector3 offset = world - transform.position;

        int x = Mathf.FloorToInt(offset.x / cellSize);
        int y = Mathf.FloorToInt(offset.y / cellSize);

        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            index = -1;
            return false;
        }

        index = x + y * width;
        return true;
    }

    private Vector3 IndexToWorld(int index, float z)
    {
        int x = index % width;
        int y = index / width;

        return new Vector3(transform.position.x + (x + 0.5f) * cellSize, transform.position.y + (y + 0.5f) * cellSize, z);
    }

    private bool CanMoveDirectly(Vector3 from, Vector3 to)
    {
        if (Physics.CheckBox(to, agentHalfExtents, Quaternion.identity, staticObstacleLayers, QueryTriggerInteraction.Ignore))
            return false;

        Vector3 delta = to - from;
        delta.z = 0f;

        float distance = delta.magnitude;

        return distance <= 0.001f || !Physics.BoxCast(from, agentHalfExtents, delta / distance, Quaternion.identity, distance, staticObstacleLayers, QueryTriggerInteraction.Ignore);
    }

    private void ReconstructPath(int startIndex, int goalIndex, float z, List<Vector3> result)
    {
        int current = goalIndex;

        while (current != startIndex)
        {
            result.Add(IndexToWorld(current, z));
            current = parent[current];
        }

        result.Reverse();
    }

    private void BeginSearch(int goal)
    {
        if (searchStamp == int.MaxValue)
        {
            Array.Clear(openedStamp, 0, openedStamp.Length);
            Array.Clear(closedStamp, 0, closedStamp.Length);
            searchStamp = 0;
        }

        searchStamp++;
        searchGoal = goal;
        heapCount = 0;
    }

    // Octile Distance - 8방향 이동 기준
    private int Heuristic(int a)
    {
        int dx = Mathf.Abs(a % width - searchGoal % width);
        int dy = Mathf.Abs(a / width - searchGoal / width);

        return 10 * (dx + dy) - 6 * Mathf.Min(dx, dy);
    }

    private bool IsBetter(int a, int b)
    {
        int ha = Heuristic(a);
        int hb = Heuristic(b);

        int fa = gCost[a] + ha;
        int fb = gCost[b] + hb;

        return fa < fb || (fa == fb && ha < hb);
    }

    private void SwapHeap(int a, int b)
    {
        int first = heap[a];
        int second = heap[b];

        heap[a] = second;
        heap[b] = first;

        heapPosition[first] = b;
        heapPosition[second] = a;
    }

    private void PushHeap(int index)
    {
        heap[heapCount] = index;
        heapPosition[index] = heapCount;

        SiftUp(heapCount++);
    }

    private void SiftUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;

            if (!IsBetter(heap[index], heap[parentIndex])) break;

            SwapHeap(index, parentIndex);
            index = parentIndex;
        }
    }

    private int PopHeap()
    {
        int result = heap[0];

        heapCount--;
        heapPosition[result] = -1;

        if (heapCount == 0) return result;

        heap[0] = heap[heapCount];
        heapPosition[heap[0]] = 0;

        int index = 0;

        while (true)
        {
            int left = index * 2 + 1;
            int right = left + 1;

            if (left >= heapCount) break;

            int best = right < heapCount && IsBetter(heap[right], heap[left]) ? right : left;

            if (!IsBetter(heap[best], heap[index])) break;

            SwapHeap(index, best);
            index = best;
        }

        return result;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos || walkable == null) return;

        for (int i = 0; i < walkable.Length; i++)
        {
            Gizmos.color = walkable[i] ? new Color(0f, 0.8f, 0.2f, 0.2f) : new Color(1f, 0.1f, 0.1f, 0.55f);

            Gizmos.DrawCube(IndexToWorld(i, transform.position.z), new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0.02f));
        }
    }
}
