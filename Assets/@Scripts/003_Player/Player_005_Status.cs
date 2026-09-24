using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;


public partial class Player : MonoBehaviour
{
    public async UniTask TakeDamage(int attackDamage)
    {
        CalculateDamage(attackDamage);

        GameObject obj = await ObjectPool.Instance.PopFromPool(GPrefabName.ATTACK_EFFECT_BASE, (RectTransform)IngameUIManager.Instance.hudCanvas.transform, false);

        obj.transform.position = Camera.main.WorldToScreenPoint(damageTrf.transform.position);

        DamageVal damageVal = obj.GetComponent<DamageVal>();

        damageVal.SetDamage(attackDamage);
        damageVal.Play();

        //몬스터의 방어력 값 계산 후 적용
    }

    public void CalculateDamage(int attackDamage)
    {
        //int calculateDamage = attackDamage;

        //방어력 계산 적용 후 빼야 함

        IngameSessionManager.Instance.playerStats.currentHp -= attackDamage;

        //체력 UI 표시하기
        IngameUIManager.Instance.SetHpSliderText();
        //죽음판정
        if (IngameSessionManager.Instance.playerStats.currentHp <= 0)
            ChangeState(PlayerState.DEAD).Forget();
    }

    public async UniTask OnDeath()
    {
        transform.DOShakePosition(0.5f, 0.2f).SetEase(Ease.Linear).OnComplete(() => Destroy(gameObject));

        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: destroyCancellationToken);
    }
}
