using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackHitBox : MonoBehaviour
{
    public Monster_000_Base monster_000_Base;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Player>() is Player player)
        {
            Debug.Log("Damage");
            player.TakeDamage((int)monster_000_Base.monsterStats.attackDamage).Forget();
        }
    }
}
