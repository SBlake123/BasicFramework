using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraLookAhead : MonoBehaviour
{
    public Player player;
    private float lookDistance = 0.8f;
    private float smoothTime = 0.6f;

    private Vector3 origin;
    private Vector3 smoothVelocity;

    private void Awake() => origin = transform.localPosition;

    private void LateUpdate()
    {
        if (player == null) return;

        Vector2 direction = player.lastLookDirection;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float snappedAngle = Mathf.Round(angle / 90f) * 90f * Mathf.Deg2Rad;
        Vector3 target = origin + new Vector3(Mathf.Cos(snappedAngle), Mathf.Sin(snappedAngle), 0f) * lookDistance;

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, target, ref smoothVelocity, smoothTime);

        //Vector2 direction = player.lastLookDirection.normalized;
        //Vector3 target = origin + new Vector3(direction.x, direction.y, 0f) * lookDistance;

        //transform.localPosition = Vector3.SmoothDamp(transform.localPosition, target, ref smoothVelocity, smoothTime);
    }
}
