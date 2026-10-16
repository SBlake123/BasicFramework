using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrawlingFlameHitBox : MonoBehaviour
{
    public CrawlingFlame crawlingFlame;

    public CrawlingFlameBoom boom;

    public LayerMask layerMask;

    private void OnTriggerEnter(Collider other)
    {
        if (!crawlingFlame.isMoving) return;

        var monster = other.GetComponentInParent<Monster_000_Base>();
        bool isGround = (layerMask.value & (1 << other.gameObject.layer)) != 0;

        if (monster == null && !isGround) return;

        crawlingFlame.isMoving = false;

        crawlingFlame.crawlingFlameTail.SetActive(false);

        if (crawlingFlame.moveDirection.x > 0) boom.sprRenderer.flipX = false;

        else boom.sprRenderer.flipX = true;

        boom.gameObject.SetActive(true);

        //if (other.GetComponentInParent<Monster_000_Base>() is Monster_000_Base monster)
        //{
        //    crawlingFlame.isMoving = false;
        //    boom.gameObject.SetActive(true);
        //}

        //if (other.GetComponentInParent<Monster_000_Base>() is Monster_000_Base monster)
        //{
        //    monster.TakeDamage(IngameSessionManager.Instance.CalculateDamageDealt());
        //}
    }    
}
