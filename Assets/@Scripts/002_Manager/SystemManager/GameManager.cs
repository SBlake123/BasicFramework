using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : PersistentMonoSingleton<GameManager>
{
    public float systemInitPer { get; set; }

    public class InitStep
    {
        public string Name { get; }
        public Func<UniTask> Action { get; }
        public Func<UniTask> ErrorAction { get; }
        public bool IsEssential { get; } // true면 실패 시 멈춤/재시도, false면 로그 찍고 건너뜀
        public int MaxRetryCount { get; }

        public InitStep(string name, Func<UniTask> action, Func<UniTask> errorAction = null, bool isEssential = true,  int maxRetryCount = 3)
        {
            Name = name;
            ErrorAction = errorAction;
            Action = action;
            IsEssential = isEssential;
            MaxRetryCount = maxRetryCount;
        }
    }

    public async UniTask SystemInitialize(Func<UniTask> onInitialize)
    {
        Application.targetFrameRate = 60;

        var initSteps = new List<InitStep>
        {
        // 필수 모듈 (실패 시 재시도 후 팝업 띄우고 중단)
        new InitStep("ResourceManager", () => ResourceManager.Instance.OnInitialize(), isEssential: true, maxRetryCount: 3),
        new InitStep("LanguageManager", () => LanguageManager.Instance.OnInitialize(), isEssential: true, maxRetryCount: 3),
        new InitStep("SaveLoadManager", () => SaveLoadManager.Instance.OnInitialize(), isEssential: true, maxRetryCount: 3),

        // 선택 모듈 (실패해도 게임 진입에는 지장 없으므로 스킵 가능)
        new InitStep("SoundManager", () => { SoundManager.Instance.SoundInit(); return UniTask.CompletedTask; }, isEssential: false),
        };

        await SystemLoading();

        onInitialize?.Invoke();

        async UniTask SystemLoading()
        {
            int loadStep = 0;

            for (int i = 0; i < initSteps.Count; i++)
            {
                var step = initSteps[i];

                bool success = false;

                for (int retry = 0; retry <= step.MaxRetryCount; retry++)
                {
                    try
                    {
                        await step.Action.Invoke();
                        success = true;
                        break;
                    }
                    catch (Exception e)
                    {

                    }
                }

                if (!success)
                {
                    if (step.IsEssential)
                    {
                        await EssentialFailed();
                    }
                }

                loadStep++;
                systemInitPer = (float) loadStep / initSteps.Count;

                await UniTask.Delay(1000);
            }
        }
    }

    public async UniTask EssentialFailed()
    {
        PopupManager.Instance.setPopUpCode(false, "Error", "Yes");
    }

    //public async UniTask KK()
    //{
    //    Application.targetFrameRate = 60;

    //    var initSteps = new List<InitStep>
    //    {
    //    // 필수 모듈 (실패 시 재시도 후 팝업 띄우고 중단)
    //    new InitStep("ResourceManager", () => ResourceManager.Instance.OnInitialize(), isEssential: true, maxRetryCount: 3),
    //    new InitStep("LanguageManager", () => LanguageManager.Instance.OnInitialize(), isEssential: true),

    //    // 선택 모듈 (실패해도 게임 진입에는 지장 없으므로 스킵 가능)
    //    new InitStep("SoundManager", () => { SoundManager.Instance.SoundInit(); return UniTask.CompletedTask; }, isEssential: false),
    //    };

    //    await UniTask.Delay(2000);

    //    await UniTask.Delay(2000);
    //    await SceneLoadManager.Instance.LoadScene(GSceneName.TITLE_SCENE);
    //}
}
