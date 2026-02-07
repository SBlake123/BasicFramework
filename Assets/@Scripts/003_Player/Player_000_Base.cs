using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player : MonoBehaviour
{
    void Start()
    {
        PlayerInit().Forget();
        PlayerActionCheck().Forget();

    }

    //void Update()
    //{
    //}
}
