using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class PlayerSkinBase : MonoBehaviour, IPlayerAttacker
{
    public Image mainImg;

    public Sprite[] skinSpriteArr;

    public Dictionary<string, Sprite> skinSpriteDic = new Dictionary<string, Sprite>();

    public void Idle()
    {
        mainImg.sprite = skinSpriteArr[(int)PlayerSkinBaseIdx.IDLE];
    }

    public void Attack()
    {
        mainImg.sprite = skinSpriteArr[(int)PlayerSkinBaseIdx.ATTACK];
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
