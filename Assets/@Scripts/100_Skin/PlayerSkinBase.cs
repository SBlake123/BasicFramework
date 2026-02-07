using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class PlayerSkinBase : MonoBehaviour, IPlayerAttacker
{
    public RectTransform mainRect;

    public Image mainImg;

    public Animator anim;

    public Sprite[] skinSpriteArr;

    public virtual bool needSprRelocation => false;

    public virtual void PlayerSprRelocationLeft()
    {
        mainRect.anchorMin = mainRect.anchorMax = new Vector2(1, 0.5f);
        mainRect.anchoredPosition = Vector2.zero;
        mainRect.localRotation = Quaternion.Euler(0f, 180f, 0f);
    }

    public virtual void PlayerSprRelocationRight()
    {
        mainRect.anchorMin = mainRect.anchorMax = new Vector2(0, 0.5f);
        mainRect.anchoredPosition = Vector2.zero;
        mainRect.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }


    public abstract void PlayIdle();

    public abstract void PlayAttack();

    public abstract void PlayHit();

    public abstract void PlayDie();
}

    //public abstract void Idle()
    //{
    //    anim.Play(GSkinSprName.IDLE);
    //}

    //public void Attack()
    //{
    //    Debug.Log("ATTACK");

    //    mainImg.sprite = skinSpriteArr[(int)PlayerSkinBaseIdx.ATTACK];
    //    anim.Play(GSkinSprName.ATTACK);
    //}

    //public void Hit()
    //{
    //    mainImg.sprite = skinSpriteArr[(int)PlayerSkinBaseIdx.HIT];
    //}

    //public void Die()
    //{
    //    mainImg.sprite = skinSpriteArr[(int)PlayerSkinBaseIdx.DIE];
    //}

