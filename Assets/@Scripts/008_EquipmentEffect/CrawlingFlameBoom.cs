using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrawlingFlameBoom : MonoBehaviour
{
    public CrawlingFlame crawlingFlame;

    public SphereCollider sphereColider;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Monster_000_Base>() is Monster_000_Base monster)
        {
            monster.TakeDamage(20);
        }
    }

    public void BoomEnd()
    {
        ObjectPool.Instance.PushToPool(crawlingFlame.gameObject);
    }

    public void HitBoxOn()
    {
        sphereColider.enabled = true;
    }

    public void HitBoxOff()
    {
        sphereColider.enabled = false;
    }
}
