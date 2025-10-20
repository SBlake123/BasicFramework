using Cysharp.Threading.Tasks;
using UnityEngine;

public class PersistentMonoSingleton<T> : MonoSingleton<T> where T : MonoSingleton<T>
{
    #region Protected Methods

    protected override async UniTask OnInitializing()
    {
        base.OnInitializing();
        if (Application.isPlaying)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    #endregion
}
