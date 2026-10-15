using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrawlingFlameTail : MonoBehaviour
{
    public Transform head;
    public Transform tail1;
    public Transform tail2;

    [SerializeField] private float followDelay = 0.08f;

    private readonly List<Vector3> positions = new();
    private readonly List<float> times = new();

    private Vector3 tail1Scale;
    private Vector3 tail2Scale;

    private void Awake()
    {
        tail1Scale = tail1.localScale;
        tail2Scale = tail2.localScale;
    }

    private void OnEnable()
    {
        positions.Clear();
        times.Clear();

        tail1.GetComponent<Animator>().Play("Burn");
        tail2.GetComponent<Animator>().Play("Burn");

        tail1.position = head.position;
        tail2.position = head.position;
    }

    private void LateUpdate()
    {
        float now = Time.time;
        float delay = Mathf.Max(0f, followDelay);

        positions.Add(head.position);
        times.Add(now);

        tail1.position = GetPosition(now - delay);
        tail2.position = GetPosition(now - delay * 1.6f);

        // 두 번째 꼬리에 필요한 기록까지만 유지
        while (times.Count > 2 && times[1] <= now - delay * 1.6f)
        {
            positions.RemoveAt(0);
            times.RemoveAt(0);
        }

        float pulse1 = Mathf.Sin(Time.time * 7f) * 0.1f;
        float pulse2 = Mathf.Sin(Time.time * 8.3f + 1.5f) * 0.15f;

        tail1.localScale = Vector3.Scale(tail1Scale, new Vector3(1f + pulse1, 1f - pulse1, 1f));
        tail2.localScale = Vector3.Scale(tail2Scale, new Vector3(1f + pulse2, 1f - pulse2, 1f));
    }

    private Vector3 GetPosition(float targetTime)
    {
        if (targetTime <= times[0]) return positions[0];

        for (int i = 1; i < times.Count; i++)
        {
            if (times[i] < targetTime) continue;

            float t = Mathf.InverseLerp(times[i - 1], times[i], targetTime);
            return Vector3.Lerp(positions[i - 1], positions[i], t);
        }

        return positions[positions.Count - 1];
    }
}
