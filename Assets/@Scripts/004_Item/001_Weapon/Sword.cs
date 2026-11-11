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
        

        //transform.localRotation = Quaternion.Euler(0f, 0f, -35f);
        //transform.DOLocalRotate(new Vector3(0f, 0f, 55f), 0.12f).SetEase(Ease.OutQuad).OnComplete(() => transform.localRotation = Quaternion.Euler(0f, 0f, 0f));
    }

    public async UniTask AttackEffectActive(Player player, CancellationToken token)
    {
        SoundManager.Instance.SetSound(GSoundName.SWORD_SWING, SoundCategory.FX, gameObject).Forget();

        var effect = await ObjectPool.Instance.PopFromPool(GPrefabName.SWORD_ATTACK_EFFECT, parent: null, isActive: false);
        var hitBox = effect.GetComponentInChildren<WeaponHitBox>(true);
        var hitCollider = hitBox != null ? hitBox.GetComponent<Collider>() : null;

        try
        {
            if (hitCollider != null)
                hitCollider.enabled = false;
            hitBox?.ClearImpactOrigin();
            token.ThrowIfCancellationRequested();

            Vector3 origin = player.transform.position;
            Vector3 direction = new Vector3(player.lastLookDirection.x, player.lastLookDirection.y, 0f).normalized;
            effect.transform.position = origin + direction;
            UpdateEffectDirection(effect, player);
            hitBox?.SetImpactOrigin(origin, direction);
            effect.SetActive(true);

            double visibleUntil = Time.timeAsDouble + 0.3f;
            if (hitCollider != null)
            {
                hitCollider.enabled = true;
                await UniTask.WaitForFixedUpdate(token);
                if (hitCollider != null)
                    hitCollider.enabled = false;
                hitBox?.ClearImpactOrigin();
            }

            double remaining = visibleUntil - Time.timeAsDouble;
            if (remaining > 0)
                await UniTask.Delay(TimeSpan.FromSeconds(remaining), cancellationToken: token);
        }
        finally
        {
            if (hitCollider != null)
                hitCollider.enabled = false;
            if (effect != null)
                ObjectPool.Instance.PushToPool(effect);
        }
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
