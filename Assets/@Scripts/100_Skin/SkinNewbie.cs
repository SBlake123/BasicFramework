using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinNewbie : PlayerSkinBase
{
    public override bool needSprRelocation => true;

    public override void PlayIdle()
    {
        anim.Play(GSkinSprName.IDLE);
        mainSpr.sprite = skinSpriteArr[(int)PlayerSkinBaseIdx.IDLE];
        Debug.Log("NEW IDLE");
    }

    public override void PlayAttack()
    {
        anim.Play(GSkinSprName.ATTACK);

        mainSpr.sprite = skinSpriteArr[(int)PlayerSkinBaseIdx.ATTACK];
        Debug.Log("NEW ATTACK");
    }

    public override void PlayHit()
    {
        anim.Play(GSkinSprName.HIT);

        mainSpr.sprite = skinSpriteArr[(int)PlayerSkinBaseIdx.HIT];
        Debug.Log("NEW HIT");
    }

    public override void PlayDie()
    {
        mainSpr.sprite = skinSpriteArr[(int)PlayerSkinBaseIdx.DIE];
        Debug.Log("NEW DIE");
    }
}
