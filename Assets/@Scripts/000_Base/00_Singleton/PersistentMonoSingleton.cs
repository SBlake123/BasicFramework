using Cysharp.Threading.Tasks;
using UnityEngine;

public class PersistentMonoSingleton<T> : MonoSingleton<T> where T : MonoSingleton<T>
{
    protected override async UniTask Initialize()
    {
        if (Application.isPlaying)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    //#region Protected Methods

    //protected override async UniTask OnInitializing()
    //{
    //    base.OnInitializing();

    //}

    //#endregion
}
