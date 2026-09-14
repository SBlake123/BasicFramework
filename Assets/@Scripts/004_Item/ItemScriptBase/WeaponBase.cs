using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponData
{
    public float attackDamage = 1f;
    public float attackSpeed = 0.3f;
    public float attackRange = 1f;
}

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponData weaponData { get; set; } = new WeaponData();

    public GameObject attackEffect;
    public float AttackDamage => weaponData.attackDamage;
    public float AttackSpeed => weaponData.attackSpeed;
    public float AttackRange => weaponData.attackRange;

    //async
    public abstract UniTask AttackAsync(Player player, CancellationToken token);

    public abstract void UpdateEffectDirection(GameObject gameObject, Player player);
}
