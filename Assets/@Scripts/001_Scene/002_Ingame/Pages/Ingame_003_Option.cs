using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingame_003_Option : StateBasePage
{
    private IngameSceneManager IngameSceneManager;
    protected override async UniTask OnFirstSetting()
    {
        IngameSceneManager = (IngameSceneManager)stateBaseSceneManager;
    }

}
