using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogoSceneManager : MonoSingleton<LogoSceneManager>
{
    public TextMeshProUGUI loadingPercentTMP;

    private void Start()
    {
        LogoSceneStart().Forget();
    }

    private async UniTask LogoSceneStart()
    {
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
