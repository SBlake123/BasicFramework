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

    }

    //게임에 필요한 친구들 다 로딩 되었는가?
    //SoundMAnager
    //다 되면 넘어가기 Title로
}
