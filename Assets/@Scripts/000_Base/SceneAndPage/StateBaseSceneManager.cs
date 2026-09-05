using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateBaseSceneManager  :MonoBehaviour 
{
    public abstract void SceneAllocate();

    public abstract UniTask ChangeState(int _state);

    public abstract UniTask OnStateChange();
}
