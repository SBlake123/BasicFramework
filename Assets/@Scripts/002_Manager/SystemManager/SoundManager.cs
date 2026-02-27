using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LitJson;
using System;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;

public enum SoundCategory
{
    BGM,
    FX
}
public class SoundManager : PersistentMonoSingleton<SoundManager>
{
    public GameObject soundDiskPrefab;

    public Transform bgmSoundParent, fxSoundParent;
    public List<SoundDisk> bgmSoundList { get; set; } = new List<SoundDisk>();
    public List<SoundDisk> fxSoundList { get; set; } = new List<SoundDisk>();

    public void SoundInit()
    {
        if (!PlayerPrefs.HasKey("FXVOL")) PlayerPrefs.SetFloat("FXVOL", 0.5f);
        if (!PlayerPrefs.HasKey("BGMVOL")) PlayerPrefs.SetFloat("BGMVOL", 0.5f);
    }

    //public async UniTask SetSound(string soundName, SoundCategory category)
    //{
    //    switch (category)
    //    {
    //        case SoundCategory.BGM:
    //            {
    //                if (bgmSoundList.Count > 0)
    //                {
    //                    foreach (Transform item in bgmSoundParent)
    //                    {
    //                        Destroy(item.gameObject);
    //                    }

    //                    //Destroy(bgmSoundParent.GetChild(0).gameObject);
    //                    bgmSoundList.Clear();
    //                }

    //                GameObject _soundDiskObj = Instantiate(soundDiskPrefab, bgmSoundParent);
    //                SoundDisk soundDisk = _soundDiskObj.GetComponent<SoundDisk>();

    //                Debug.Log($"soundName : {soundName}");
    //                soundDisk.audioSource.clip = await ResourceManager.Instance.LoadAsset<AudioClip>(soundName); //어드레서블에서 바꿀 곳
    //                soundDisk.category = category;
    //                soundDisk.audioSource.volume = PlayerPrefs.GetFloat("BGMVOL");
    //                soundDisk.audioSource.loop = true;
    //                bgmSoundList.Add(soundDisk);

    //                soundDisk.audioSource.Play();
    //            }
    //            break;

    //        case SoundCategory.FX:
    //            {
    //                GameObject _soundDiskObj = Instantiate(soundDiskPrefab, fxSoundParent);
    //                SoundDisk soundDisk = _soundDiskObj.GetComponent<SoundDisk>();
    //                soundDisk.audioSource.clip = await ResourceManager.Instance.LoadAsset<AudioClip>(soundName);//Resources.Load<AudioClip>(soundName); //어드레서블에서 바꿀 곳
    //                fxSoundList.Add(soundDisk);

    //                if (soundDisk.audioSource.clip != null)
    //                {
    //                    soundDisk.category = category;
    //                    soundDisk.audioSource.volume = PlayerPrefs.GetFloat("FXVOL");
    //                    soundDisk.audioSource.loop = false;

    //                    soundDisk.audioSource.Play();

    //                    StopSound(soundDisk, SoundCategory.FX, soundDisk.audioSource.clip.length).Forget();

    //                    //Destroy(_soundDiskObj, soundDisk.audioSource.clip.length);
    //                }

    //            }
    //            break;
    //    }
    //}

    public async UniTask StopSound(string soundName, SoundCategory category)
    {
        await UniTask.WaitForFixedUpdate();

        switch (category)
        {
            case SoundCategory.BGM:
                {
                    for (int i = 0; i < bgmSoundList.Count; i++)
                    {
                        if (bgmSoundList[i].audioSource.name == soundName)
                        {
                            bgmSoundList[i].audioSource.Stop();
                        }
                    }
                }
                break;

            case SoundCategory.FX:
                {
                    for (int i = 0; i < fxSoundList.Count; i++)
                    {
                        Debug.Log(fxSoundList[i].audioSource.clip.name);

                        if (fxSoundList[i].audioSource.clip.name == soundName)
                        {
                            SoundDisk _soundDisk = fxSoundList[i];

                            _soundDisk.audioSource.Stop();
                            fxSoundList.Remove(_soundDisk);
                            Destroy(_soundDisk.gameObject);
                        }
                    }
                }
                break;
        }
    }
    public async UniTask StopSound(SoundDisk soundDisk, SoundCategory category, float duration)
    {
        await UniTask.WaitForSeconds(duration);

        switch (category)
        {
            case SoundCategory.FX:
                {
                    for (int i = 0; i < fxSoundList.Count; i++)
                    {
                        if (fxSoundList[i] == soundDisk)
                        {
                            fxSoundList[i].audioSource.Stop();
                            fxSoundList.Remove(soundDisk);
                            Destroy(soundDisk.gameObject);
                        }
                    }
                }
                break;
        }
    }

    public async UniTask StopAllFXSounds()
    {
        for (int i = fxSoundList.Count - 1; i >= 0; i--)
        {
            fxSoundList[i].audioSource.Stop();
            Destroy(fxSoundList[i].gameObject);
            fxSoundList.RemoveAt(i);
        }

        await UniTask.WaitForFixedUpdate();
    }

    public async UniTask SetSoundFromResources(string soundName, SoundCategory category)
    {
        switch (category)
        {
            case SoundCategory.BGM:
                {
                    if (bgmSoundList.Count > 0)
                    {
                        Destroy(bgmSoundParent.GetChild(0).gameObject);
                        bgmSoundList.Clear();
                    }

                    GameObject _soundDiskObj = Instantiate(soundDiskPrefab, bgmSoundParent);
                    SoundDisk soundDisk = _soundDiskObj.GetComponent<SoundDisk>();
                    soundDisk.audioSource.clip = Resources.Load<AudioClip>(soundName); //await ResourceManager.Instance.LoadAsset<AudioClip>(soundName); //어드레서블에서 바꿀 곳
                    soundDisk.category = category;
                    soundDisk.audioSource.volume = PlayerPrefs.GetFloat("BGMVOL");
                    soundDisk.audioSource.loop = true;
                    bgmSoundList.Add(soundDisk);

                    soundDisk.audioSource.Play();
                }
                break;

            case SoundCategory.FX:
                {
                    GameObject _soundDiskObj = Instantiate(soundDiskPrefab, fxSoundParent);
                    SoundDisk soundDisk = _soundDiskObj.GetComponent<SoundDisk>();
                    soundDisk.audioSource.clip = Resources.Load<AudioClip>(soundName); //await ResourceManager.Instance.LoadAsset<AudioClip>(soundName);//Resources.Load<AudioClip>(soundName); //어드레서블에서 바꿀 곳
                    soundDisk.category = category;
                    soundDisk.audioSource.volume = PlayerPrefs.GetFloat("FXVOL");
                    soundDisk.audioSource.loop = false;

                    soundDisk.audioSource.Play();

                    Destroy(_soundDiskObj, soundDisk.audioSource.clip.length);
                }
                break;
        }

        await UniTask.WaitForFixedUpdate();
    }
}
