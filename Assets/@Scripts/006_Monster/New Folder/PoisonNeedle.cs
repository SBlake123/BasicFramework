using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PooledObject))]
public class PoisonNeedle : MonoBehaviour
{
    public LayerMask hitLayers;

    bool hasExploded = false;

    public void Init()
    {
        hasExploded = false;
    }

    private void OnEnable()
    {
        Init();
    }

    private float moveSpeed = 13f;
    private void Update()
    {
        transform.position += transform.right * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        int damage = 15;

        if (hasExploded) return;

        if (other.GetComponentInParent<Player>() is Player player)
        {
            player.TakeDamage(damage).Forget();

            //Æø¹ß ÀÌÆåÆ®
            ObjectPool.Instance.PushToPool(gameObject);
            hasExploded = true;

        }

        if ((hitLayers.value & (1 << other.gameObject.layer)) != 0)
        {
            //Æø¹ß ÀÌÆåÆ®
            ObjectPool.Instance.PushToPool(gameObject);
            hasExploded = true;

        }

    }
}
