using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinNewbie : PlayerSkinBase
{
    public override bool needSprRelocation => true;

    public override void PlayIdle()
    {
        anim.Play(GSkinSprName.IDLE);
        Debug.Log("NEW IDLE");
    }

    public override void PlayMove()
    {
        anim.Play(GSkinSprName.MOVE);
        Debug.Log("NEW MOVE");
    }


    public override void PlayAttack()
    {
        anim.Play(GSkinSprName.ATTACK);
        Debug.Log("NEW ATTACK");
    }

    public override void PlayHit()
    {
        anim.Play(GSkinSprName.HIT);

        Debug.Log("NEW HIT");
    }

    public override void PlayDie()
    {
        Debug.Log("NEW DIE");
    }

    public override void PlayDodge()
    {
        
    }
}
