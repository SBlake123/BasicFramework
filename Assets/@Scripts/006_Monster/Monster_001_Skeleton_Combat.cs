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
        monsterData.currentHp -= attackDamage;

        //죽음판정
        if (monsterData.currentHp <= 0)
            ChangeState((int)MonsterState.DEAD).Forget();
    }
}
