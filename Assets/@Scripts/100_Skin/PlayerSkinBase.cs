using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class PlayerSkinBase : MonoBehaviour, IPlayerAttacker
{
    //public RectTransform mainRect;
    public Transform mainTrf;
    public SpriteRenderer mainSpr;
    public Transform weaponTrf;
    public Transform shieldTrf;

    //public Image mainImg;

    public Animator anim;

    public virtual Vector3 weaponPosition => Vector3.zero;// = new Vector3(0.35f, -0.25f);
    public virtual Vector3 shieldPosition => Vector3.zero;//= new Vector3(-0.36f, -0.2f);

    public virtual bool needSprRelocation => false;

    public virtual void PlayerSprRelocationLeft()
    {
        mainTrf.localPosition = Vector3.zero;
        weaponTrf.localPosition = new Vector3(-weaponPosition.x, weaponPosition.y, weaponPosition.z);
        shieldTrf.localPosition = new Vector3(-shieldPosition.x, shieldPosition.y, shieldPosition.z);

        mainSpr.flipX = true;
    }

    public virtual void PlayerSprRelocationRight()
    {
        mainTrf.localPosition = Vector3.zero;
        weaponTrf.localPosition = new Vector3(weaponPosition.x, weaponPosition.y, weaponPosition.z);
        shieldTrf.localPosition = new Vector3(shieldPosition.x, shieldPosition.y, shieldPosition.z);

        mainSpr.flipX = false;
    }


    public abstract void PlayIdle();

    public abstract void PlayMove();

    public abstract void PlayAttack();

    public abstract void PlayHit();

    public abstract void PlayDodge();

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

