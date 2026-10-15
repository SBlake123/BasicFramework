using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PooledObject))]
public class BurningAura : MonoBehaviour
{
    public LayerMask hitLayers;

    public Animator anim;

    public SphereCollider col;

    public int damage { get; set; } = 3;

    private float damageInterval = 0.3f; // 데미지 간격(초)
    private void OnEnable()
    {
        anim.Play("Burning_Aura");
    }


    private readonly Dictionary<Monster_000_Base, float> nextDamageTimes = new();

    private void OnTriggerStay(Collider other)
    {
        var monster = other.GetComponentInParent<Monster_000_Base>();
        if (monster == null) return;

        if (nextDamageTimes.TryGetValue(monster, out float nextTime)
            && Time.time < nextTime)
            return;

        nextDamageTimes[monster] = Time.time + damageInterval;
        monster.TakeDamage(damage);
    }

    private void OnDisable()
    {
        nextDamageTimes.Clear();
    }
}
