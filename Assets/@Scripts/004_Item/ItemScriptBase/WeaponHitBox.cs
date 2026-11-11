using Cysharp.Threading.Tasks;
using UnityEngine;

public class WeaponHitBox : MonoBehaviour
{
    private const float KnockbackDistance = 0.5f;

    private Vector3? impactOrigin;
    private Vector3 attackDirection;

    public void SetImpactOrigin(Vector3 origin, Vector3 direction)
    {
        impactOrigin = origin;
        attackDirection = direction;
    }

    internal void ClearImpactOrigin()
    {
        impactOrigin = null;
        attackDirection = Vector3.zero;
    }

    protected void OnDisable() => ClearImpactOrigin();

    protected void OnTriggerEnter(Collider other)
    {
        Debug.Log($"TRIGGER ENTER : {other.name}");

        if (other.GetComponentInParent<Monster_000_Base>() is Monster_000_Base monster)
        {
            if (impactOrigin.HasValue)
                PlayImpact(monster, other, impactOrigin.Value, attackDirection);
            monster.TakeDamage(IngameSessionManager.Instance.CalculateDamageDealt());
        }
        //monster.TakeDamage(damage);
    }

    private void PlayImpact(Monster_000_Base monster, Collider hitCollider, Vector3 origin, Vector3 attackDirection)
    {
        GameJuiceManager manager = GameJuiceManager.Instance;
        monster.PlayHitFlash();

        Vector3 direction = monster.transform.position - origin;
        direction.z = 0f;
        if (direction.sqrMagnitude < 0.0001f)
            direction = attackDirection;
        manager.PlayKnockback(direction, KnockbackDistance, monster.QueueWeaponKnockback);
        manager.PlayShockwave(hitCollider.ClosestPoint(origin));
        manager.PlayShake();
        SoundManager.Instance.SetSound(GSoundName.SWORD_ATTACK_HIT, SoundCategory.FX, monster.gameObject).Forget();
    }

}
