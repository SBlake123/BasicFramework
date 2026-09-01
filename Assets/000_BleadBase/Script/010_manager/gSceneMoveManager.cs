/// [스크립트 명세]-----------------------------------------------------------------------------------------------------------------------
///  씬 이동을 처리하는 매니저
///  로고 씬에서 GameObject 컴포넌트에 등록
/// --------------------------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LitJson;
using System;


public class gSceneMoveManager : MonoBehaviour
{
    public GameObject panelSceneLoading;
    //AsyncOperation async;

    private static int nowState = (int)MOVE_STATE.READY;     //현재 상태
    private static string nextScene = "";                    //이동할 씬
    private static bool bReady = false;
    bool bComplete;

    // [씬 무브 상태 확인]
    public enum MOVE_STATE
    {
        READY = 0,  //준비 
        REQUEST,    //이동 요청 
        MOVING,     //이동 중
        //DONE,       //종료
    }

    // Awake is called before the first frame update
    void Awake()
    {
        var obj = FindObjectsOfType<gSceneMoveManager>();
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
            case (int)MOVE_STATE.REQUEST:
                nowState = (int)MOVE_STATE.MOVING;
                bComplete = false;
                StartCoroutine("sceneMove");
                break;
            case (int)MOVE_STATE.MOVING:
                if (bComplete == true)
                {
                    initManager();
                }
                break;
        }
    }

    //이동을 요청한다.
    public static void setMoveScene(string scene)
    {
        if (nowState == (int)MOVE_STATE.READY)
        {
            nextScene = scene;
            nowState = (int)MOVE_STATE.REQUEST;
        }
    }

    //매니저 초기화
    void initManager()
    {
        if (panelSceneLoading.activeSelf == true)
        {
            panelSceneLoading.SetActive(false);
        }
        nowState = (int)MOVE_STATE.READY;
        nextScene = "";
        bComplete = false;
        bReady = true;
    }

    //씬 이동 시 코루틴으로 호출
    IEnumerator sceneMove()
    {
        if (panelSceneLoading.activeSelf == false)
        {
            panelSceneLoading.SetActive(true);
        }
        AsyncOperation async = SceneManager.LoadSceneAsync(nextScene);
        while (!async.isDone)
        {
            float progress = async.progress * 100.0f;
            int pRounded = Mathf.RoundToInt(progress);
            gText.setText(panelSceneLoading.transform.Find("Text").gameObject, string.Format(gText.getBaseText((int)ENUM_BASE.SCENELOADING), pRounded.ToString()));
            yield return true;
        }
        bComplete = true;
    }
}
