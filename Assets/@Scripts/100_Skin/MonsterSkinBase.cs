using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonsterSkinBase : MonoBehaviour, IMonsterAnimation
{
    public Transform mainTrf;
    public SpriteRenderer mainSpr;

    //public Image mainImg;

    public Animator anim;

    public virtual bool needSprRelocation => false;

    public abstract UniTask PlayAttack();

    public abstract UniTask PlayDeath();

    public abstract UniTask PlayHit();

    public abstract UniTask PlayIdle();

    public abstract UniTask PlayMove();
}
