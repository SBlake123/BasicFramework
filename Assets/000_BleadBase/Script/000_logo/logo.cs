/// [스크립트 명세]=======================================================================================================================
/// 
///  1. 정의 : "000_logo" 씬의 GameObject에 연결된 MonoBehaviour 스크립트
///  2. 기능 : "000_logo" 씬의 기본 제어 기능을 처리
/// --------------------------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LitJson;
using System;
using UnityEngine.Video;
//using static System.Net.Mime.MediaTypeNames;

public class logo : MonoBehaviour
{
    // [씬스테이트 상태값 선언]===================================================================================================
    
    public enum SCENE_STATE     //씬의 현재 상태를 확인하기 위한 enum 값
    {
        READY = 0,      //준비 
        MAIN,           //시작 
        SCENE_MOVE,     //씬 이동
    }

    //----------------------------------------------------------------------------------------------------------------------------



    // [이하 변수]================================================================================================================

    public GameObject uiCanvas; //UI Canvas 게임 오브젝트

    int nowState = (int)SCENE_STATE.READY;      //현재 상태
    int beforeState = (int)SCENE_STATE.READY;   //이전 상태

    float logoTimer = 0.0f;         //로고 타임처리를 담당하는 타이머 변수
    const float logoTime = 3.0f;    //로고가 보여지는 시간

    bool bLocaleLoad = false;       //언어가 셋팅되었는지를 확인하는 변수
    bool bExitLogo = false;         //exitLogo가 호출되었는지를 체크하는 함수

    /// --------------------------------------------------------------------------------------------------------------------------



    /// [MonoBehaviour 함수]=======================================================================================================

    void Awake()
    {
        //씬 초기화
        gBase.InitScene((int)ENUM_SCENE.LOGO);
        //슬립 전환 안되게 변경
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //씬 이니셜라이즈
        initLogo();
    }
    void Update ()
    {
        //현재 스테이트 상태에 따라 업데이트 처리한다.
        switch (nowState)
        {
            case (int)SCENE_STATE.MAIN:
                logoTimer += Time.deltaTime;
                if (logoTimer >= logoTime * 2.0f && bLocaleLoad == true && bExitLogo == false)
                {   //로고가 3배수 이상 보여지고 언어가 셋팅되었으며 exitLogo가 호출되지 않았다면 exitLogo()를 호출합니다.
                    exitLogo();
                }
                else if (logoTimer >= logoTime && bLocaleLoad == true && bExitLogo == false && useFCM.checkInit() == true)
                {   //로고가 보여지고 언어가 셋팅되었으며 exitLogo가 호출되지 않았고 FCM이 활성화되었다면 exitLogo()를 호출합니다.
                    exitLogo();
                }
                else
                {
                    showLogo();
                }
                break;
        }
    }
    /// --------------------------------------------------------------------------------------------------------------------------



    /// [메인 처리 함수]=======================================================================================================

    void initLogo()
    {
        //씬 초기화. 굳이 할 필요가 없엉.
        //gBase.InitScene((int)ENUM_SCENE.LOGO);
        
        //로고 씬 초기값 셋팅
        logoTimer = 0.0f;
        bLocaleLoad = false;
        bExitLogo = false;
        
        //로케일 설정
        StartCoroutine("getLocale");
        
        //페이스북 초기화. 하지만 해당 프로젝트에서 페이스북은 로그인 수단으로 쓰지 않으니 초기화할 필요가 없다.
        //useFB.fInit();

        //FireBase 초기화
        useFCM.initFCM();

        //로고 비활성화
        uiCanvas.transform.Find("Logo").gameObject.SetActive(false);

        //상태를 메인으로 변경
        setState((int)SCENE_STATE.MAIN);

        //gSystem.getNowLocalDay();
        //
        //DateTime t = DateTime.Now;
        //gSystem.getCalendarDate(t.Year, t.Month);

        //볼륨
        setVolume();
    }

    //로고를 보여줍니다
    void showLogo()
    {
        GameObject logo = uiCanvas.transform.Find("Logo").gameObject;
        float lAlpha = 0.0f;
        if(logoTimer < logoTime / 3.0f)
        {
            //서서히 보여준다.
            lAlpha = gUi.setAlphaValueOverTime(0, logoTimer, (logoTime / 3.0f));
        }
        else if(logoTimer >= logoTime / 3.0f && logoTimer < logoTime / 3.0f * 2.0f)
        {
            //보여주는 것을 유지한다.
            lAlpha = 255.0f;
        }
        else
        {
            //서서히 사라진다.
            lAlpha = gUi.setAlphaValueOverTime(1, (logoTimer - (logoTime / 3.0f * 2.0f)), (logoTime / 3.0f));
        }
        if(lAlpha > 0.0f && logo.activeInHierarchy == false)
        {
            logo.SetActive(true);
        }
        logo.GetComponent<Image>().color = new Color32(255, 255, 255, Convert.ToByte(lAlpha));
    }

    //로고 씬을 종료하고 타이틀 씬으로 이동합니다.
    void exitLogo()
    {
        bExitLogo = true;
        //상태를 씬 무브로 변경
        setState((int)SCENE_STATE.SCENE_MOVE);
        //씬 무브 매니저에게 씬 이동을 요청
        //gSceneMoveManager.setMoveScene("010_main");
    }

    //스테이트를 변경하고 이전 스테이트를 기억한다.
    void setState(int val)
    {
        beforeState = nowState;
        nowState = val;
    }

    //저장된 언어 설정이 없을 경우, OS를 체크해서 플레이어프렙스를 저장한다음 언어를 셋팅합니다.
    IEnumerator getLocale()
    {
        int setLang = 0;
        if (EncryptedPlayerPrefs.HasKey(gData.myPrefs.uLang) == false)
        {
            if (gBase.gameVersion.multiLang == true)
            {
                setLang = gBase.checkOSLanguage();
            }
            else
            {
                setLang = (int)ENUM_TEXT_TYPE.KR;
            }
            EncryptedPlayerPrefs.SetInt(gData.myPrefs.uLang, setLang);
            EncryptedPlayerPrefs.SetInt(gData.myPrefs.uVoice, setLang);
            PlayerPrefs.Save();
        }
        gText.setLanguage();
        
        bLocaleLoad = true;
        yield return null;
    }

    //저장된 볼륨 설정이 없는 경우, 볼륨을 100으로 설정합니다.
    void setVolume()
    {
        if (EncryptedPlayerPrefs.HasKey(gData.myPrefs.vMusic) == false)
        {
            EncryptedPlayerPrefs.SetInt(gData.myPrefs.vMusic, 100);
            EncryptedPlayerPrefs.SetInt(gData.myPrefs.vSoundFx, 100);
            PlayerPrefs.Save();
        }
    }

    /// ---------------------------------------------------------------------------------------------------------------------------------
}
