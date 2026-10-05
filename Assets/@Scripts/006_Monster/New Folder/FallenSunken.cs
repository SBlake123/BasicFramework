using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PooledObject))]
public class FallenSunken : MonoBehaviour
{
    public LayerMask hitLayers;

    public Animator anim;

    public BoxCollider col;

    public void Init()
    {
        col.enabled = false;
    }

    private void OnEnable()
    {
        Init();
        ActivateSunken();
    }

    private void ActivateSunken()
    {
        anim.Play("FallenSunken");
        //애니메이션 실행
    }

    private void OnTriggerEnter(Collider other)
    {
        int damage = 15;

        if (other.GetComponentInParent<Player>() is Player player)
        {
            col.enabled = false;
            player.TakeDamage(damage).Forget();

            //폭발 이펙트
        }
    }

    //event

    public void OnHitBox()
    {
        col.enabled = true;
    }

    public void EndSunken()
    {
        ObjectPool.Instance.PushToPool(gameObject);
    }
}
