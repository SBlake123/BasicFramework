using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : PersistentMonoSingleton<GameManager>
{
    public float systemInitPer { get; set; }

    public class InitStep
    {
        public string Name { get; }
        public Func<UniTask> Action { get; }
        public Func<UniTask> ErrorAction { get; }
        public bool IsEssential { get; }
        public int MaxRetryCount { get; }

        public InitStep(string name, Func<UniTask> action, Func<UniTask> errorAction = null, bool isEssential = true, int maxRetryCount = 3)
        {
            Name = name;
            ErrorAction = errorAction;
            Action = action;
            IsEssential = isEssential;
            MaxRetryCount = maxRetryCount;
        }
    }

    public async UniTask SystemInitialize(Func<UniTask> onInitialize = null)
    {
        Application.targetFrameRate = 60;

        var initSteps = new List<InitStep>
        {
            new InitStep("ResourceManager", () => ResourceManager.Instance.OnInitialize(), isEssential: true, maxRetryCount: 3),
            new InitStep("LanguageManager", () => LanguageManager.Instance.OnInitialize(), isEssential: true, maxRetryCount: 3),
            new InitStep("SaveLoadManager", () => SaveLoadManager.Instance.OnInitialize(), isEssential: true, maxRetryCount: 3),
            new InitStep("DataManager", () => DataManager.Instance.OnInitialize(), isEssential: true, maxRetryCount: 3),

            new InitStep("SoundManager", () => { SoundManager.Instance.SoundInit(); return UniTask.CompletedTask; }, isEssential: false),
        };

        bool isSuccess = await SystemLoading();

        if (onInitialize != null)
        {
            await onInitialize();
        }

        async UniTask<bool> SystemLoading()
        {
            for (int i = 0; i < initSteps.Count; i++)
            {
                var step = initSteps[i];

                bool success = false;

                for (int retry = 0; retry <= step.MaxRetryCount; retry++)
                {
                    try
                    {
                        await step.Action();
                        success = true;
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }

                if (!success && step.IsEssential)
                {
                    await EssentialFailed();
                    return false;
                }

                systemInitPer = (float)(i + 1) / initSteps.Count;
            }

            return true;
        }

        //async UniTask SystemLoading()
        //{
        //    int loadStep = 0;

        //    for (int i = 0; i < initSteps.Count; i++)
        //    {
        //        var step = initSteps[i];

        //        bool success = false;

        //        for (int retry = 0; retry <= step.MaxRetryCount; retry++)
        //        {
        //            try
        //            {
        //                await step.Action.Invoke();
        //                success = true;
        //                break;
        //            }
        //            catch (Exception e)
        //            {

        //            }
        //        }

        //        if (!success)
        //        {
        //            if (step.IsEssential)
        //            {
        //                await EssentialFailed();
        //            }
        //        }

        //        loadStep++;
        //        systemInitPer = (float)loadStep / initSteps.Count;

        //        await UniTask.Delay(1000);
        //    }
        //}
    }

    public async UniTask EssentialFailed()
    {
        PopupManager.Instance.setPopUpCode(false, "ERROR", "QUIT");
        PopupManager.Instance.AddMethodToBtn(() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
        });
    }

    public void SetGameplayPaused(bool isPaused, Action callback = null)
    {
        Time.timeScale = isPaused ? 0f : 1f;
        callback?.Invoke();
    }

    public async UniTask SetGameplayPaused(bool isPaused, Func<UniTask> callback = null)
    {
        Time.timeScale = isPaused ? 0f : 1f;

        if (callback != null)
        {
            await callback.Invoke();
        }
    }
}
