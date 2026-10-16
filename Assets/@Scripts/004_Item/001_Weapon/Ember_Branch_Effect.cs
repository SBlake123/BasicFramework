using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ember_Branch_Effect : MonoBehaviour
{
    public BoxCollider boxCollider;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"TRIGGER ENTER : {other.name}");

        if (other.GetComponentInParent<Monster_000_Base>() is Monster_000_Base monster)
        {
            monster.TakeDamage(IngameSessionManager.Instance.CalculateDamageDealt());
        }
        //monster.TakeDamage(damage);
    }

    public void ColliderOn()
    {
        boxCollider.enabled = true;
    }

    public void ColliderOff()
    {
        boxCollider.enabled = false;

    }

    public void EndEffect()
    {
        ObjectPool.Instance.PushToPool(gameObject);
    }
}
