using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DG.Tweening;

public partial class Monster_001_Skeleton : Monster_000_Base
{
    public override async UniTask TakeDamage(int attackDamage)
    {
        GameObject obj = await ObjectPool.Instance.PopFromPool(GPrefabName.ATTACK_EFFECT_BASE, (RectTransform)IngameUIManager.Instance.hudCanvas.transform, false);

        obj.transform.position = Camera.main.WorldToScreenPoint(damageTrf.transform.position);

        DamageVal damageVal = obj.GetComponent<DamageVal>();

        damageVal.SetDamage(attackDamage);
        damageVal.Play();

        CalculateDamage(attackDamage);
        //몬스터의 방어력 값 계산 후 적용
    }

    public void CalculateDamage(int attackDamage)
    {
        monsterStats.currentHp -= attackDamage;

        //죽음판정
        if (monsterStats.currentHp <= 0)
            ChangeState((int)MonsterState.DEAD).Forget();
    }

    public async UniTask AttackAsync(Player player, CancellationToken token)
    {
        attackHitBoxTrf.gameObject.SetActive(true);
        AttackEffectActive(player, destroyCancellationToken).Forget();

        await UniTask.Delay(System.TimeSpan.FromSeconds(0.12f), cancellationToken: token);

        attackHitBoxTrf.gameObject.SetActive(false);
    }

    public async UniTask AttackEffectActive(Player player, CancellationToken token)
    {
        //var effect = Instantiate(attackEffect, attackEffect.transform.position, Quaternion.identity);
        //UpdateEffectDirection(attackHitBoxTrf.gameObject, player);
        //effect.SetActive(true);

        //try
        //{
        //    await UniTask.Delay(TimeSpan.FromSeconds(AttackSpeed), cancellationToken: token);
        //}
        //finally
        //{
        //    if (effect != null)
        //        Destroy(effect);
        //}
    }

    public void UpdateEffectDirection(GameObject gameObject, Player player)
    {
        if (isAttack || target == null || facingObjectParentTrf == null)
        {
            return;
        }

        //Vector2 dir = player.moveInput.sqrMagnitude > 0.001f ? player.moveInput.normalized : player.lastMoveDirection;
        Vector2 dir = (player.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (player.isPlayerFacingRight)
            gameObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        else
            gameObject.transform.rotation = Quaternion.Euler(0f, 180f, 180f - angle);
    }
}
