using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class PlayerSkinBase : MonoBehaviour, IPlayerAttacker
{
    public Image mainImg;

    public Animator anim;
    
    public Sprite[] skinSpriteArr;

    public Dictionary<string, Sprite> skinSpriteDic = new Dictionary<string, Sprite>();

    public void Idle()
    {
        anim.Play(GSkinSprName.IDLE);
    }

    public void Attack()
    {
        Debug.Log("ATTACK");

        mainImg.sprite = skinSpriteArr[(int)PlayerSkinBaseIdx.ATTACK];
        anim.Play(GSkinSprName.ATTACK);
    }

    public void Hit()
    {
        mainImg.sprite = skinSpriteArr[(int)PlayerSkinBaseIdx.HIT];
    }

    public void Die()
    {
        mainImg.sprite = skinSpriteArr[(int)PlayerSkinBaseIdx.DIE];
    }
}
