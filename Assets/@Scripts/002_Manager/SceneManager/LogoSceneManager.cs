using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogoSceneManager : MonoSingleton<LogoSceneManager>
{
    public TextMeshProUGUI loadingPercentTMP;

    public class InitStep
    {
        public string Name { get; }
        public Func<UniTask> Action { get; }
        public bool IsEssential { get; } // true면 실패 시 멈춤/재시도, false면 로그 찍고 건너뜀
        public int MaxRetryCount { get; }

        public InitStep(string name, Func<UniTask> action, bool isEssential = true, int maxRetryCount = 3)
        {
            Name = name;
            Action = action;
            IsEssential = isEssential;
            MaxRetryCount = maxRetryCount;
        }
    }

    private void Start()
    {
        LogoSceneStart().Forget();
    }

    private async UniTask LogoSceneStart()
    {
        Application.targetFrameRate = 60;

        var initSteps = new List<InitStep>
        {
        // 필수 모듈 (실패 시 재시도 후 팝업 띄우고 중단)
        new InitStep("ResourceManager", () => ResourceManager.Instance.OnInitialize(), isEssential: true, maxRetryCount: 3),
        new InitStep("LanguageManager", () => LanguageManager.Instance.OnInitialize(), isEssential: true),

        // 선택 모듈 (실패해도 게임 진입에는 지장 없으므로 스킵 가능)
        new InitStep("SoundManager", () => { SoundManager.Instance.SoundInit(); return UniTask.CompletedTask; }, isEssential: false),
        };


        SoundManager.Instance.SoundInit();
        await UniTask.Delay(2000);
        loadingPercentTMP.text = $"{100}%";
        await UniTask.Delay(2000);
        await SceneLoadManager.Instance.LoadScene(GSceneName.TITLE_SCENE);
    }

    //게임에 필요한 친구들 다 로딩 되었는가?
    //SoundMAnager
    //다 되면 넘어가기 Title로
}
