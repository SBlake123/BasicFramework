/// [스크립트 명세]-----------------------------------------------------------------------------------------------------------------------
///  팝업 메시지를 처리하는 매니저
///  로고 씬에서 GameObject 컴포넌트에 등록
/// --------------------------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LitJson;
using System;

public class gPopMsgManager : MonoBehaviour
{
    //[변수 및 선언]==================================================================================

    // [모듈 상태 확인]
    private enum MODULE_STATE
    {
        SET = 0,    //셋
        READY,      //레디 
        REQUEST,    //팝업 요청 
        SHOWING,    //팝업 보여주는 중
        CLOSE,      //닫기
    }

    //[팝업 메시지 등록]
    public enum POPMSG
    {
        NOT_FIND_WORD = 0,      //단어를 찾을 수 없습니다.
        ADD_WORD,               //단어를 추가했습니다.
        NOT_FIND_BOOK,          //연습장을 찾을 수 없습니다.
        FAVORITES_ADD,          //즐겨찾기를 추가했습니다.
        FAVORITES_DELETE,       //즐겨찾기를 삭제했습니다.
        NOT_ADD_WORD,            //연습장에 단어를 추가할 수 없습니다.
        REQUEST_MESSAGE,
        TYPING_GAME_DISABLE
    }

    //[제어 변수]
    private static int nowState = (int)MODULE_STATE.SET;
    private bool bMsgAction = false;
    private int msgActionStep = 0;
    private float msgActionTimer = 0.0f;
    private float msgActionTime = 0.25f;

    public GameObject pMsg;

    //[(정적) 변수]
    private static int inputMsg = (int)POPMSG.NOT_FIND_WORD;
    private static float inputTime = (int)POPMSG.NOT_FIND_WORD;
    private static int additionalMsg = 0;

    //-----------------------------------------------------------------------------------------------



    //[MonoBehaviour 제어 함수]======================================================================

    void Awake()
    {
        var obj = FindObjectsOfType<gPopMsgManager>();
        if (obj.Length <= 1)
        {
            gBase.setEnKey();
            DontDestroyOnLoad(gameObject);
            initPopMsg();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        //상태에 따라 처리
        switch (nowState)
        {
            case (int)MODULE_STATE.REQUEST:
                setPopMsgAction(inputMsg, inputTime, additionalMsg);
                break;
            case (int)MODULE_STATE.SHOWING:
                if(bMsgAction == false)
                {
                    nowState = (int)MODULE_STATE.READY;
                }
                break;

        }
    }

    //-----------------------------------------------------------------------------------------------


    //[요청 메소드]==================================================================================

    public static void requestPopMsg(int msg, float actionTime = 0.4f, int addMsg = 0)
    {
        if (nowState == (int)MODULE_STATE.READY)
        {
            inputMsg = msg;
            inputTime = actionTime;
            additionalMsg = addMsg;
            nowState = (int)MODULE_STATE.REQUEST;
        }
        else
        {
            if (nowState == (int)MODULE_STATE.SET)
            {
                Debug.Log("<b><i>Please add [BleadPopMsgManager] to the Scene.</i></b>");
            }
        }
    }

    public static bool checkClear()
    {
        if (nowState == (int)MODULE_STATE.SET || nowState == (int)MODULE_STATE.READY)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //-----------------------------------------------------------------------------------------------



    //[내부 로직]===================================================================================

    //이니셜라이즈
    private void initPopMsg()
    {
        //GameObject pMsg = this.transform.Find("PopMsg").gameObject;

        //초기화
        bMsgAction = false;
        msgActionStep = 0;
        msgActionTimer = 0.0f;
        pMsg.SetActive(false);

        nowState = (int)MODULE_STATE.READY;
    }

    //액션 셋팅
    private void setPopMsgAction(int wType = 1, float actionTime = 0.4f, int additionalMsg = 0)
    {
        if (bMsgAction == false && nowState == (int)MODULE_STATE.REQUEST)
        {
            bMsgAction = true;
            nowState = (int)MODULE_STATE.SHOWING;

            //GameObject pMsg = this.transform.Find("PopMsg").gameObject;

            switch (wType)
            {
                case (int)POPMSG.NOT_FIND_WORD:
                    gText.setText(pMsg.transform.Find("Pop/T_Text").gameObject, getText(170200));
                    break;
                case (int)POPMSG.ADD_WORD:
                    gText.setText(pMsg.transform.Find("Pop/T_Text").gameObject, getText(92091));
                    break;
                case (int)POPMSG.NOT_FIND_BOOK:
                    gText.setText(pMsg.transform.Find("Pop/T_Text").gameObject, getText(150200));
                    break;
                case (int)POPMSG.FAVORITES_ADD:
                    gText.setText(pMsg.transform.Find("Pop/T_Text").gameObject, getText(90455));
                    break;
                case (int)POPMSG.FAVORITES_DELETE:
                    gText.setText(pMsg.transform.Find("Pop/T_Text").gameObject, getText(90456));
                    break;
                case (int)POPMSG.NOT_ADD_WORD:
                    gText.setText(pMsg.transform.Find("Pop/T_Text").gameObject, getText(90479));
                    break;
                case (int)POPMSG.REQUEST_MESSAGE:
                    gText.setText(pMsg.transform.Find("Pop/T_Text").gameObject, string.Format(LanguageManager.Instance.GetLangScript(92225), additionalMsg));
                    break;
                case (int)POPMSG.TYPING_GAME_DISABLE:
                    gText.setText(pMsg.transform.Find("Pop/T_Text").gameObject, getText(92229));
                    break;

                case 900:
                    {
                        string _body = string.Format(LanguageManager.Instance.GetLangScript(92226), 50);

                        gText.setText(pMsg.transform.Find("Pop/T_Text").gameObject, _body);
                    }
                    break;

                case 999:
                    gText.setText(pMsg.transform.Find("Pop/T_Text").gameObject, "Ad Loading...");
                    break;

                default:
                    gText.setText(pMsg.transform.Find("Pop/T_Text").gameObject, getText(92091));
                    break;
            }
            msgActionStep = 0;
            msgActionTimer = 0.0f;
            msgActionTime = actionTime;
            StartCoroutine("popMsgAction");
        }
    }

    //액션 처리
    private IEnumerator popMsgAction()
    {
        //GameObject pMsg = this.transform.Find("PopMsg").gameObject;
        RectTransform pRect = pMsg.transform.Find("Pop").GetComponent<RectTransform>();

        float msgTimeVal = 0.0f;

        while (bMsgAction == true)
        {
            switch (msgActionStep)
            {
                case 0:
                    pRect.anchoredPosition3D = new Vector3(pRect.anchoredPosition3D.x, -100.0f, 0.0f);
                    msgActionStep = 1;
                    msgActionTimer = 0.0f;
                    pMsg.SetActive(true);
                    break;
                case 1:
                    msgActionTimer += Time.smoothDeltaTime;
                    msgTimeVal = msgActionTime / 2.0f;
                    if (msgActionTimer > msgTimeVal)
                    {
                        pRect.anchoredPosition3D = new Vector3(pRect.anchoredPosition3D.x, 0.0f, 0.0f);
                        msgActionStep = 2;
                        msgActionTimer = 0.0f;
                    }
                    else
                    {
                        float pY = -100.0f + (100.0f / msgTimeVal * msgActionTimer);
                        pRect.anchoredPosition3D = new Vector3(pRect.anchoredPosition3D.x, pY, 0.0f);
                    }
                    break;
                case 2:
                    msgActionTimer += Time.smoothDeltaTime;
                    if (msgActionTimer > msgActionTime)
                    {
                        msgActionStep = 3;
                        msgActionTimer = 0.0f;
                    }
                    break;
                case 3:
                    msgActionTimer += Time.smoothDeltaTime;
                    msgTimeVal = msgActionTime / 2.0f;
                    if (msgActionTimer > msgTimeVal)
                    {
                        pRect.anchoredPosition3D = new Vector3(pRect.anchoredPosition3D.x, 100.0f, 0.0f);
                        msgActionStep = 4;
                        msgActionTimer = 0.0f;
                    }
                    else
                    {
                        float pY = 0.0f + (100.0f / msgTimeVal * msgActionTimer);
                        pRect.anchoredPosition3D = new Vector3(pRect.anchoredPosition3D.x, pY, 0.0f);
                    }
                    break;
                case 4:
                    bMsgAction = false;
                    pMsg.SetActive(false);
                    break;
            }
            yield return null;
        }
    }

    //Text를 얻어온다.
    private static string getText(int code)
    {
        string str = "";

        str = LanguageManager.Instance.GetLangScript(code, LanguageManager.Instance.languageScriptDic);

        return str;
    }

    //------------------------------------------------------------------------------------------------------------



}
