using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DG.Tweening;

public partial class Monster_101_Fallen : Monster_000_Base
{
    public async UniTask AttackAsync(Player player, CancellationToken token)
    {
        attackHitBoxTrf.gameObject.SetActive(true);

        await UniTask.Delay(System.TimeSpan.FromSeconds(0.12f), cancellationToken: token);

        attackHitBoxTrf.gameObject.SetActive(false);
    }
}
