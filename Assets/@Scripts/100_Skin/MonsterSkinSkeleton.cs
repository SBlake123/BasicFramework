using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSkinSkeleton : MonsterSkinBase
{
    public Sprite hitSprite;
    public Sprite deadSprite;
    public Sprite[] runSpriteArr;

    public override bool needSprRelocation => true;
    public override async UniTask PlayIdle()
    {
        anim.Play(GSkinSprName.IDLE);
        mainSpr.sprite = runSpriteArr[0];
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
