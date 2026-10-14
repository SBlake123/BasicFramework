using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PooledObject))]
public class BurningAura : MonoBehaviour
{
    public LayerMask hitLayers;

    public Animator anim;

    public BoxCollider col;
}
