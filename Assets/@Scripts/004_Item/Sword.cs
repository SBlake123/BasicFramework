using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Sword : WeaponBase
{
    public override async UniTask AttackAsync(Player player, CancellationToken token)
    {
        AttackEffectActive(player, destroyCancellationToken).Forget();

        transform.localRotation = Quaternion.Euler(0f, 0f, -35f);
        transform.DOLocalRotate(new Vector3(0f, 0f, 55f), 0.12f).SetEase(Ease.OutQuad).OnComplete(() => transform.localRotation = Quaternion.Euler(0f, 0f, 0f));
    }

    public async UniTask AttackEffectActive(Player player, CancellationToken token)
    {
        var effect = Instantiate(attackEffect, attackEffect.transform.position, Quaternion.identity);
        UpdateEffectDirection(effect, player);
        effect.SetActive(true);

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(AttackSpeed), cancellationToken: token);
        }
        finally
        {
            if (effect != null)
                Destroy(effect);
        }
    }

    public override void UpdateEffectDirection(GameObject gameObject, Player player)
    {
        Vector2 dir = player.moveInput.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (player.isPlayerFacingRight)
            gameObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        else
            gameObject.transform.rotation = Quaternion.Euler(0f, 180f, 180f - angle);
    }
}
