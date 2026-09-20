using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSkinSkeleton : MonsterSkinBase
{
    public override bool needSprRelocation => true;
    public override async UniTask PlayIdle()
    {
        anim.Play(GSkinSprName.IDLE);
        Debug.Log("NEW IDLE");
    }
    public override async UniTask PlayAttack()
    {
      
    }

    public override async UniTask PlayDeath()
    {

    }

    public override async UniTask PlayHit()
    {
        //±ôºý±ôºý
    }

    public override async UniTask PlayMove()
    {
        anim.Play("SkeletonMove");
    }


}
