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
        var effect = Instantiate(attackEffect, attackEffect.transform.position, attackEffect.transform.rotation);
        UpdateEffectDirection(effect, player);
        effect.SetActive(true);

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.4f), cancellationToken: token);
        }
        finally
        {
            if (effect != null)
                Destroy(effect);
        }
    }

    public override void UpdateEffectDirection(GameObject gameObject, Player player)
    {
        Vector3 angle = attackEffect.transform.eulerAngles;

        if (player.isPlayerFacingRight)
            gameObject.transform.rotation = Quaternion.Euler(angle.x, 0f, angle.z);
        else
            gameObject.transform.rotation = Quaternion.Euler(angle.x, 180f, angle.z + 180f);
    }
}
