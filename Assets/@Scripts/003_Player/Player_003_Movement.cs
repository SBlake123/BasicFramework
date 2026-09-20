using UnityEngine;

// Both characters stop their own movement before contact; neither applies impulses.
public static class CharacterContactMovement
{
    private const float ContactGap = 0.02f;

    //public static void Move(Rigidbody body, Vector3 velocity)
    //{
    //    if (body == null) return;
    //    Vector3 displacement = velocity * Time.fixedDeltaTime;
    //    displacement.z = 0f;
    //    float distance = displacement.magnitude;
    //    if (distance < 0.00001f) return;

    //    Vector3 direction = displacement / distance;
    //    // Reserve half the free distance so two approaching characters cannot
    //    // both consume the same gap during one physics step.
    //    RaycastHit[] hits = body.SweepTestAll(direction, distance * 2f + ContactGap,
    //        QueryTriggerInteraction.Ignore);
    //    foreach (RaycastHit hit in hits)
    //    {
    //        Collider obstacle = hit.collider;
    //        if (obstacle == null || obstacle.attachedRigidbody == body ||
    //            Physics.GetIgnoreLayerCollision(body.gameObject.layer, obstacle.gameObject.layer)) continue;
    //        if (hit.normal.sqrMagnitude > 0f && Vector3.Dot(direction, hit.normal) >= 0f) continue;
    //        distance = Mathf.Min(distance, Mathf.Max(0f, (hit.distance - ContactGap) * 0.5f));
    //    }
    //    body.MovePosition(body.position + direction * distance);
    //}

    // 목표 속도까지 도달하는 속도
    public static void Move(Rigidbody body, Vector3 desiredVelocity)
    {
        if (body == null) return;

        //Vector3 displacement = desiredVelocity * Time.fixedDeltaTime;
        //displacement.z = 0f;
        //float distance = displacement.magnitude;

        //if (body == null) return;

        body.velocity = desiredVelocity;

        //Vector3 direction = displacement / distance;

        //body.MovePosition(body.position + direction * distance);
    }

    public static void Stop(Rigidbody body)
    {
        if (body == null) return;

        Vector3 velocity = body.velocity;
        velocity.x = 0f;
        velocity.y = 0f;

        body.velocity = velocity;
    }
}
