using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;


public partial class Player : MonoBehaviour
{
    public async UniTask TakeDamage(int attackDamage)
    {
        int calculatingDamage = IngameSessionManager.Instance.CalculateDamageAfterDefense(attackDamage);

        CalculateDamage(calculatingDamage);

        GameObject obj = await ObjectPool.Instance.PopFromPool(GPrefabName.ATTACK_EFFECT_BASE, (RectTransform)IngameUIManager.Instance.hudCanvas.transform, false);

        obj.transform.position = Camera.main.WorldToScreenPoint(damageTrf.transform.position);

        DamageVal damageVal = obj.GetComponent<DamageVal>();

        damageVal.SetDamage(calculatingDamage);
        damageVal.Play();

        //몬스터의 방어력 값 계산 후 적용
    }

    public void CalculateDamage(int attackDamage)
    {
        //int calculateDamage = attackDamage;

        //방어력 계산 적용 후 빼야 함

        IngameSessionManager.Instance.playerStats.currentHp -= attackDamage;

        //체력 UI 표시하기
        IngameUIManager.Instance.SetHpSlider();
        //죽음판정
        if (IngameSessionManager.Instance.playerStats.currentHp <= 0)
        {
            IngameSessionManager.Instance.playerStats.currentHp = 0;
            IngameUIManager.Instance.SetHpSlider();
            ChangeState(PlayerState.DEAD).Forget();
        }
    }

    public async UniTask OnDeath()
    {
        isDead = true;
        playerSkinBase.EquipItemActivate(false);
        playerSkinBase.PlayDie();
        OnOffHitBox(false);
        moveInput = Vector2.zero;
        aimInput = Vector2.zero;
        movingVirtualJoystickInput = Vector2.zero;
        aimingVirtualJoystickInput = Vector2.zero;
        desiredVelocity = Vector3.zero;
        CharacterContactMovement.Stop(movementBody);

        await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: destroyCancellationToken);

        GameManager.Instance.SetGameplayPaused(true, callback: null);
    }
}
