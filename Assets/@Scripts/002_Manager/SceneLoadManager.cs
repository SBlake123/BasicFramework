using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : Singleton<SceneLoadManager>
{
    public async UniTask LoadScene(string sceneName, Func<UniTask> onComplete = null)
    {
        SceneTracker.Instance.SceneNameSave(sceneName);

        if (onComplete != null) await onComplete.Invoke();

        var handle = SceneManager.LoadSceneAsync(sceneName);

        await UniTask.WaitUntil(() => handle.isDone);
    }
}
