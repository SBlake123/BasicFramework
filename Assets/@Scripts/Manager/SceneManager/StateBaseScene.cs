using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateBaseScene : MonoBehaviour
{
    public GameObject loading;

    public abstract UniTask SceneAllocate();

    public abstract UniTask ChangeState(int _state);

    public abstract UniTask OnStateChange();
}
