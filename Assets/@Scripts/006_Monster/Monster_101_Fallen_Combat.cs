using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DG.Tweening;

public partial class Monster_101_Fallen : Monster_000_Base
{
    public async UniTask AttackAsync(CancellationToken token)
    {
        attackHitBoxTrf.gameObject.SetActive(true);

        await UniTask.Delay(System.TimeSpan.FromSeconds(0.12f), cancellationToken: token);

        attackHitBoxTrf.gameObject.SetActive(false);
    }

    public async UniTask RollingAttackAsync(CancellationToken token)
    {

        //target.position 도착했을 때....

        Vector2 forward = target.position - transform.position;
        if (forward.sqrMagnitude < 0.0001f) return;

        float baseAngle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;

        for (int i = 0; i < 8; i++)
        {
            float angle = baseAngle + i * 45f;
            float radian = angle * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0f);
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            GameObject poisonNeedle = await ObjectPool.Instance.PopFromPool("PoisonNeedle");

            poisonNeedle.transform.position = transform.position;
            poisonNeedle.transform.rotation = rotation;
            // 발사체를 firePoint.position에 생성.
            // 발사체 회전에는 rotation, 이동 방향에는 direction 사용.
        }


        //8방향 가시쏘기 
        //만들기
        //쏘기
        //부딫히면 펑펑 터짐
        //총알에 만들기..
    }


    public override async UniTask TakeDamage(int attackDamage)
    {
        GameObject obj = await ObjectPool.Instance.PopFromPool(GPrefabName.ATTACK_EFFECT_BASE, (RectTransform)IngameUIManager.Instance.hudCanvas.transform, false);

        obj.transform.position = Camera.main.WorldToScreenPoint(damageTrf.transform.position);

        DamageVal damageVal = obj.GetComponent<DamageVal>();

        damageVal.SetDamage(attackDamage);
        damageVal.Play();

        CalculateDamage(attackDamage);
    }

    public void CalculateDamage(int attackDamage)
    {
        monsterStats.currentHp -= attackDamage;

        //죽음판정
        if (monsterStats.currentHp <= 0)
            ChangeState((int)MonsterState.DEAD).Forget();
    }
}
