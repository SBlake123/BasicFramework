using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinNewbie : PlayerSkinBase
{
    public override bool needSprRelocation => true;
    public override Vector3 weaponPosition => new Vector3(0.32f, -0.25f);
    public override Vector3 shieldPosition => new Vector3(-0.36f, -0.2f);

    public override void PlayIdle()
    {
        anim.Play(GSkinSprName.IDLE);
    }

    public override void PlayMove()
    {
        anim.Play(GSkinSprName.MOVE);
    }


    public override void PlayAttack()
    {
        anim.Play(GSkinSprName.ATTACK);
    }

    public override void PlayHit()
    {
        anim.Play(GSkinSprName.HIT);

    }

    public override void PlayDie()
    {
    }

    public override void PlayDodge()
    {
        anim.Play("Dodge", 0, 0f);
    }
}
