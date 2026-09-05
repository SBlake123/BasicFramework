using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    private string savePath => Path.Combine(Application.persistentDataPath, "SaveData.dat");

    public async UniTask OnInitialize()
    {
        await Initialize();
    }

    protected override async UniTask Initialize()
    {
        Load();
        await UniTask.WaitForFixedUpdate();
    }

    public void Save(string jsonString)
    {
        try
        {
            if (string.IsNullOrEmpty(jsonString)) return;

            string encrypted = GSecurity.EncryptString(jsonString);
            File.WriteAllText(savePath, encrypted);
        }

        catch (Exception ex)
        {
            Debug.LogError($"[SaveLoadManager] 저장 실패: {ex.Message}");
        }
    }

    public string Load()
    {
        Debug.Log("Load");

        if (!File.Exists(savePath))
        {
            return null;
        }

        try
        {
            string encrypted = File.ReadAllText(savePath);
            if (string.IsNullOrEmpty(encrypted)) return null;

            return GSecurity.DecryptString(encrypted);
        }
        catch (Exception ex)
        {
            // 파일이 깨졌거나 위변조되어 복호화 에러가 날 경우 대비
            Debug.LogError($"[SaveLoadManager] 불러오기 실패 / 파일 손상: {ex.Message}");
            return null;
        }
    }

}
