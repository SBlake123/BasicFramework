using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
public class ShieldData
{
    public float defense;
    public float guardDuration;
    public float cooldown;
}

public abstract class ShieldBase : MonoBehaviour
{
    public ShieldData shieldData { get; set; }

    public GameObject guardEffect;

    public float Defense => shieldData.defense;
    public float GuardDuration => shieldData.guardDuration;
    public float Cooldown => shieldData.cooldown;

    public abstract UniTask GuardAsync(Player player, CancellationToken token);
}
