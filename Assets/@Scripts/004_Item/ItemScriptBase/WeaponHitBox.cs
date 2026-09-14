using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHitBox : MonoBehaviour
{
    private float damage;

    public void Init(float damage)
    {
        this.damage = damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"TRIGGER ENTER : {other.name}");


        if (other.GetComponentInParent<Monster_000_Base>() is Monster_000_Base monster)
            Debug.Log("MONSTER_HIT");
        //monster.TakeDamage(damage);
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log($"TRIGGER STAY : {other.name}");
    }
}
