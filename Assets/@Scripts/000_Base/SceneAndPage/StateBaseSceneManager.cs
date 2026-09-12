using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateBaseSceneManager  : MonoBehaviour 
{
    public abstract void SceneAllocate();

    //async
    public abstract UniTask ChangeState(int _state);

    //async
    public abstract UniTask OnStateChange();

    //async
    protected abstract UniTask BackKeySetting();
}
