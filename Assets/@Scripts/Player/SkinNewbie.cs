using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinNewbie : PlayerSkinBase
{
    public new void Idle()
    {
        mainImg.sprite = skinSpriteArr[(int)PlayerSkinBaseIdx.IDLE];
        Debug.Log("NEW IDLE");
    }

    public new void Attack()
    {
        mainImg.sprite = skinSpriteArr[(int)PlayerSkinBaseIdx.ATTACK];
        Debug.Log("NEW ATTACK");
    }

    public new void Hit()
    {
        mainImg.sprite = skinSpriteArr[(int)PlayerSkinBaseIdx.HIT];
        Debug.Log("NEW HIT");
    }

    public new void Die()
    {
        mainImg.sprite = skinSpriteArr[(int)PlayerSkinBaseIdx.DIE];
        Debug.Log("NEW DIE");
    }
}
