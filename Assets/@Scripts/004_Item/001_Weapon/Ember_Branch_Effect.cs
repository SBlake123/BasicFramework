using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ember_Branch_Effect : WeaponHitBox
{
    public BoxCollider boxCollider;

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
