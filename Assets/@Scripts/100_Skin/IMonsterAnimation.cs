using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMonsterAnimation
{
    UniTask PlayAttack();

    UniTask PlayIdle();

    UniTask PlayHit();

    UniTask PlayDeath();

    UniTask PlayMove();
}
