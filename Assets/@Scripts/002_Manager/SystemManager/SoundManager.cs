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

    public Transform bgmSoundParent, fxSoundParent, soundDiskPool;
    public List<SoundDisk> bgmSoundList { get; set; } = new List<SoundDisk>();
    public List<SoundDisk> fxSoundList { get; set; } = new List<SoundDisk>();

    public void SoundInit()
    {
        if (!PlayerPrefs.HasKey("FXVOL")) PlayerPrefs.SetFloat("FXVOL", 0.5f);
        if (!PlayerPrefs.HasKey("BGMVOL")) PlayerPrefs.SetFloat("BGMVOL", 0.5f);
    }

    private readonly Queue<SoundDisk> soundPool = new();
    private GameObject PopSoundDisk()
    {
        if (soundPool.Count > 0)
        {
            SoundDisk disk = soundPool.Dequeue();
            disk.gameObject.SetActive(true);
            return disk.gameObject;
        }

        return Instantiate(soundDiskPrefab, soundDiskPool);
    }

    private void PushSoundDisk(GameObject disk)
    {
        SoundDisk soundDisk = disk.GetComponent<SoundDisk>();

        disk.gameObject.SetActive(false);
        disk.transform.SetParent(soundDiskPool);
        soundDisk.audioSource.clip = null;
        soundPool.Enqueue(soundDisk);
    }

    public async UniTask SetSound(string soundName, SoundCategory category, GameObject owner = null)
    {
        switch (category)
        {
            case SoundCategory.BGM:
                {
                    if (bgmSoundList.Count > 0)
                    {
                        foreach (Transform item in bgmSoundParent)
                        {
                            item.GetComponent<SoundDisk>().audioSource.Stop();
                            PushSoundDisk(item.gameObject);
                            //Destroy(item.gameObject);
                        }

                        //Destroy(bgmSoundParent.GetChild(0).gameObject);
                        bgmSoundList.Clear();
                    }

                    GameObject _soundDiskObj = PopSoundDisk();
                    _soundDiskObj.transform.SetParent(bgmSoundParent);
                    SoundDisk soundDisk = _soundDiskObj.GetComponent<SoundDisk>();

                    Debug.Log($"soundName : {soundName}");
                    soundDisk.audioSource.clip = await ResourceManager.Instance.LoadAsset<AudioClip>(soundName); //어드레서블에서 바꿀 곳
                    soundDisk.category = category;
                    soundDisk.audioSource.volume = PlayerPrefs.GetFloat("BGMVOL");
                    soundDisk.audioSource.loop = true;
                    bgmSoundList.Add(soundDisk);

                    soundDisk.audioSource.Play();
                }
                break;

            case SoundCategory.FX:
                {
                    GameObject _soundDiskObj = PopSoundDisk();
                    _soundDiskObj.transform.SetParent(fxSoundParent);
                    SoundDisk soundDisk = _soundDiskObj.GetComponent<SoundDisk>();
                    soundDisk.audioSource.clip = await ResourceManager.Instance.LoadAsset<AudioClip>(soundName);//Resources.Load<AudioClip>(soundName); //어드레서블에서 바꿀 곳
                    fxSoundList.Add(soundDisk);

                    if (soundDisk.audioSource.clip != null)
                    {
                        soundDisk.category = category;
                        soundDisk.owner = owner;
                        soundDisk.audioSource.volume = PlayerPrefs.GetFloat("FXVOL");
                        soundDisk.audioSource.loop = false;

                        soundDisk.audioSource.Play();

                        StopSound(soundDisk, SoundCategory.FX, soundDisk.audioSource.clip.length).Forget();

                        //Destroy(_soundDiskObj, soundDisk.audioSource.clip.length);
                    }

                }
                break;
        }
    }

    public async UniTask StopSound(string soundName, SoundCategory category, GameObject owner = null)
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

                        if (fxSoundList[i].owner == owner)
                        {
                            SoundDisk soundDisk = fxSoundList[i];

                            soundDisk.audioSource.Stop();
                            fxSoundList.Remove(soundDisk);
                            PushSoundDisk(soundDisk.gameObject);
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
                            PushSoundDisk(soundDisk.gameObject);
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
            PushSoundDisk(fxSoundList[i].gameObject);
            fxSoundList.RemoveAt(i);
        }

        await UniTask.WaitForFixedUpdate();
    }

    public void PauseSound(SoundCategory soundCategory)
    {
        switch (soundCategory)
        {
            case SoundCategory.BGM:
                {
                    foreach (var disk in bgmSoundList)
                    {
                        disk.audioSource.Pause();
                    }
                }
                break;

            case SoundCategory.FX:
                {
                    foreach (var disk in fxSoundList)
                    {
                        disk.audioSource.Pause();
                    }
                }
                break;
        }
    }

    public void UnPauseSound(SoundCategory soundCategory)
    {
        switch (soundCategory)
        {
            case SoundCategory.BGM:
                {
                    foreach (var disk in bgmSoundList)
                    {
                        disk.audioSource.UnPause();
                    }
                }
                break;

            case SoundCategory.FX:
                {
                    foreach (var disk in fxSoundList)
                    {
                        disk.audioSource.UnPause();
                    }
                }
                break;
        }
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

                    GameObject _soundDiskObj = Instantiate(soundDiskPrefab.gameObject, bgmSoundParent);
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
                    GameObject _soundDiskObj = Instantiate(soundDiskPrefab.gameObject, fxSoundParent);
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
