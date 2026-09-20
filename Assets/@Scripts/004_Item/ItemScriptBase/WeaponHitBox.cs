using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHitBox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"TRIGGER ENTER : {other.name}");

        if (other.GetComponentInParent<Monster_000_Base>() is Monster_000_Base monster)
        {
            monster.TakeDamage((int)IngameSessionManager.Instance.playerStats.attackDamage);
        }
        //monster.TakeDamage(damage);
    }
}
