/// [스크립트 명세]-----------------------------------------------------------------------------------------------------------------------
///  사운드를 처리하는 매니저
///  로고 씬에서 GameObject 컴포넌트에 등록
/// --------------------------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LitJson;
using System;

public class gSoundManager : MonoBehaviour
{
    public GameObject player;
    public GameObject disk;

    private static int nowState = (int)STATE.READY;
    private static bool bVolumeChange;
    private static bool bInit = false;

    public enum STATE
    {
        READY = 0,      //준비 
        CLEAR,          //정리 
        CLEAR_FX,       //이펙트 정리 
        ONLIVE,         //사운드 송출 가능 
    }
    public enum SOUND_TYPE
    {
        MUSIC = 0,      //음악 
        FX,             //효과 
    }

    //사운드 재생 대기 리스트
    public struct soundStr
    {
        public int soundType;
        public string prefabRoot;
        public bool loop;
        public float volume;
        public float pitch;
    }
    private static List<soundStr> waitSoundList;

    // Awake is called before the first frame update
    void Awake()
    {
        var obj = FindObjectsOfType<gSoundManager>();
        if (obj.Length <= 1)
        {
            gBase.setEnKey();
            DontDestroyOnLoad(gameObject);
            initManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //현재 스테이트 상태에 따라 업데이트 처리한다.
        switch (nowState)
        {
            case (int)STATE.CLEAR:
                clearSound();
                break;
            case (int)STATE.CLEAR_FX:
                clearFx();
                break;
            case (int)STATE.ONLIVE:
                updateSound();
                break;
        }
    }

    //스테이트를 변경한다.
    public static void setState(int state)
    {
        nowState = state;
    }

    //사운드를 등록한다.
    public static void setSound(int soundType, string prefabRoot, bool loop)
    {
        if (bInit == true)
        {
            soundStr newSound = new soundStr();
            //float volume = 1.0f;
            newSound.soundType = soundType;
            newSound.prefabRoot = prefabRoot;
            newSound.loop = loop;
            newSound.volume = getVolume(newSound.soundType);
            newSound.pitch = 1.0f;

            waitSoundList.Add(newSound);
        }
    }

    //볼륨을 변경한다.
    public static void setVolume()
    {
        bVolumeChange = true;
    }

    //매니저 초기화
    void initManager()
    {
        waitSoundList = new List<soundStr>();
        setState((int)STATE.CLEAR);
        bInit = true;
    }

    //사운드 정리
    void clearSound()
    {
        //음악 소스를 삭제한다.
        foreach (Transform child in player.transform.Find("Music").transform)
        {
            Destroy(child.gameObject);
        }
        //효과음 소스를 삭제한다.
        foreach (Transform child in player.transform.Find("Fx").transform)
        {
            Destroy(child.gameObject);
        }
        if(player.transform.Find("Music").transform.childCount <= 0 && player.transform.Find("Fx").transform.childCount <= 0)
        {
            //gameObject.SetActive(false);
            setState((int)STATE.ONLIVE);
        }
    }

    //사운드 정리
    public void clearFx()
    {
        //효과음 소스를 삭제한다.
        foreach (Transform child in player.transform.Find("Fx").transform)
        {
            Destroy(child.gameObject);
        }
        if (player.transform.Find("Fx").transform.childCount <= 0)
        {
            setState((int)STATE.ONLIVE);
        }
    }

    void updateSound()
    {
        //음악 소스를 삭제한다.
        foreach (Transform child in player.transform.Find("Music").transform)
        {
            if (child.GetComponent<AudioSource>().isPlaying == false)
            {
                Destroy(child.gameObject, 0.1f);
            }
            else
            {
                if (bVolumeChange == true)
                {
                    child.GetComponent<AudioSource>().volume = getVolume((int)SOUND_TYPE.MUSIC);
                    bVolumeChange = false;
                }
            }
        }
        //효과음 소스를 삭제한다.
        foreach (Transform child in player.transform.Find("Fx").transform)
        {
            if (child.GetComponent<AudioSource>().isPlaying == false)
            {
                Destroy(child.gameObject, 0.1f);
            }
            else
            {
                if (bVolumeChange == true)
                {
                    child.GetComponent<AudioSource>().volume = getVolume((int)SOUND_TYPE.FX);
                    bVolumeChange = false;
                }
            }
        }
        if (waitSoundList.Count > 0)
        {
            for (int i = waitSoundList.Count - 1; i >= 0; i--)
            {
                if (checkAlreadyMusic(waitSoundList[i].soundType, waitSoundList[i].prefabRoot) == false)
                {
                    createAudioDisk(waitSoundList[i].soundType, waitSoundList[i].prefabRoot, waitSoundList[i].loop, waitSoundList[i].volume, waitSoundList[i].pitch);
                }
                waitSoundList.Remove(waitSoundList[i]);
            }
        }
    }

    void createAudioDisk(int soundType, string prefabRoot, bool loop, float volume, float pitch)
    {
        if (Resources.Load<GameObject>(string.Format("100_Prefabs/000_Sound/{0}", prefabRoot)) != null)
        {
            GameObject nItem = Instantiate(Resources.Load<GameObject>(string.Format("100_Prefabs/000_Sound/{0}", prefabRoot))) as GameObject;
            AudioSource source = nItem.GetComponent<AudioSource>();
            source.loop = loop;
            source.volume = volume;
            source.pitch = pitch;
            if (soundType == (int)SOUND_TYPE.MUSIC)
            {
                nItem.transform.SetParent(player.transform.Find("Music").transform);
            }
            else
            {
                nItem.transform.SetParent(player.transform.Find("Fx").transform);
            }
            source.dopplerLevel = 0;
            source.Play();
            //if(gameObject.activeInHierarchy == false)
            //{
            //   gameObject.SetActive(true);
            //}
        }
    }

    //설정된 볼륨을 가져온다.
    //음악 볼륨 가져오기    (1)리턴값: 0.0f~1.0f. (2)호출방법: gSoundManager.getVolume((int) gSoundManager.SOUND_TYPE.MUSIC);
    //효과음 볼륨 가져오기  (1)리턴값: 0.0f~1.0f. (2)호출방법: gSoundManager.getVolume((int) gSoundManager.SOUND_TYPE.FX);
    public static float getVolume(int soundType)
    {
        float volume = 0.0f;
        if (soundType == (int)SOUND_TYPE.MUSIC)
        {
            volume = (float)EncryptedPlayerPrefs.GetInt(gData.myPrefs.vMusic, 100) / 100.0f;
        }
        else
        {
            volume = (float)EncryptedPlayerPrefs.GetInt(gData.myPrefs.vSoundFx, 100) / 100.0f;
        }
        if (volume < 0.0f)
        {
            volume = 0.0f;
        }
        else if (volume > 1.0f)
        {
            volume = 1.0f;
        }
        return volume;
    }

    //현재 해당 음악을 재생하고 있는가?
    bool checkAlreadyMusic(int soundType, string soundName)
    {
        bool reVal = false;
        if (soundType == (int)SOUND_TYPE.MUSIC)
        {
            foreach (Transform child in player.transform.Find("Music").transform)
            {
                if (child.name == soundName+"(Clone)" && child.GetComponent<AudioSource>().isPlaying)
                {
                    reVal = true;
                    break;
                }
            }
        }
        return reVal;
    }

}
