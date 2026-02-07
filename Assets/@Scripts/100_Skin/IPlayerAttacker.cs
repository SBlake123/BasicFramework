using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerAttacker
{
    void PlayAttack();

    void PlayIdle();

    void PlayHit();

    void PlayDie();

}
