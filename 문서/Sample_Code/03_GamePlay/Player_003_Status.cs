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
    }

    public void CalculateDamage(int attackDamage)
    {
        if (isDead || playerState == PlayerState.DEAD)
            return;

        var stats = IngameSessionManager.Instance.playerStats;

        stats.currentHp = Mathf.Max(0, stats.currentHp - attackDamage);

        IngameUIManager.Instance.SetHpSlider();

        if (stats.currentHp == 0)
        {
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

        GameManager.Instance.SetGameplayPaused(true, (Action)null);
    }
}
