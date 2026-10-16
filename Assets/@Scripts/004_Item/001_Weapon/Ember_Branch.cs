using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Ember_Branch : WeaponBase
{
    public override async UniTask AttackAsync(Player player, CancellationToken token)
    {
        AttackEffectActive(player, destroyCancellationToken).Forget();
    }

    public async UniTask AttackEffectActive(Player player, CancellationToken token)
    {
        var effect = await ObjectPool.Instance.PopFromPool("Ember_Branch_Effect", parent: null, false);
        Vector3 direction = new Vector3(player.lastLookDirection.x, player.lastLookDirection.y, 0f).normalized;

        effect.transform.position = player.transform.position + direction * 1.2f;
        UpdateEffectDirection(effect, player);
        effect.SetActive(true);
    }

    public override void UpdateEffectDirection(GameObject gameObject, Player player)
    {
        //Vector2 dir = player.moveInput.sqrMagnitude > 0.001f ? player.moveInput.normalized : player.lastMoveDirection;
        Vector2 dir = player.lastLookDirection;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (player.isPlayerFacingRight)
            gameObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        else
            gameObject.transform.rotation = Quaternion.Euler(0f, 180f, 180f - angle);
    }
}
