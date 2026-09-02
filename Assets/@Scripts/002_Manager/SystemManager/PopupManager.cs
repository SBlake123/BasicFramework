using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LitJson;
using System;
using Cysharp.Threading.Tasks;

public class PopupManager : PersistentMonoSingleton<PopupManager>
{
    public GameObject panelPopUp;
    private static Button popup1Btn;
    private static Button popup2BtnYes;
    private static Button popup2BtnNo;
    private static Button popup1Back;
    private static Button popup2Back;

    private static int nowState = (int)POPUP_STATE.READY;    //현재 상태
    private static bool bChoosePopup;                        //false:1버튼 팝업 true:2버튼 팝업
    //private static bool bSelect;                             //선택하였는가?
    //private static bool bOk;                                 //true = Yes, false = no
    private static string strHeader;                         //Text Header
    private static string strBody;                           //Text Body;
    private static string strYes = "";                            //Text BtnYes and confirm
    private static string strNo = "";                             //Text btnNo;

    //private static int returnSceneStateYes;
    //private static int returnSceneStateNo;
    private static Action backKeyAction;

    //241023 threeBtnPopup Add by Seunghwan
    private static Button popup3Btn1;
    private static Button popup3Btn2;
    private static Button popup3Btn3;
    private static Button popup3Back;

    private static string strFirst;
    private static string strSecond;
    private static string strThird;



    bool bInputHold = false;    //키 입력 시 1프레임 지연 처리를 위한 변수값 (이 값이 없으면 메인 모듈의 update와 중복 처리가 발생할 수 있다.)

    // [팝업 상태 확인]
    public enum POPUP_STATE
    {
        READY = 0,    //준비 
        REQUEST,      //팝업 요청 
        SHOWING,      //팝업 보여주는 중
        SELECT,       //선택
        INPUT_KEY,    //백 키 누름
        CLOSE,        //닫기
        THREEBTNPOPUP //3버튼팝업
    }

    // Update is called once per frame
    void Update()
    {
        bInputHold = false;
        //인풋 처리한다.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            switch (nowState)
            {
                case (int)POPUP_STATE.SHOWING:
                case (int)POPUP_STATE.THREEBTNPOPUP:
                    //setBackKeyAction();
                    setBackKeySelect();
                    bInputHold = true;
                    break;
            }
        }
        if (bInputHold == false)
        {
            //현재 스테이트 상태에 따라 업데이트 처리한다.
            switch (nowState)
            {
                case (int)POPUP_STATE.REQUEST:
                    nowState = (int)POPUP_STATE.SHOWING;
                    //bSelect = false;
                    if (bChoosePopup == true)
                    {
                        //선택해야 하는 팝업의 경우 팝업2
                        showPopUp2();
                    }
                    else
                    {
                        showPopUp1();
                    }
                    break;
                case (int)POPUP_STATE.INPUT_KEY:
                    setBackKeyAction();
                    break;
                case (int)POPUP_STATE.CLOSE:
                    //RemoveBtnListener();
                    initManager();
                    break;
                case (int)POPUP_STATE.THREEBTNPOPUP:
                    shopPopUp3();
                    break;
            }
        }
    }

    void initBtn()
    {
        GameObject _popup1 = panelPopUp.transform.Find("PopUp1").transform.Find("Body").gameObject;
        GameObject _popup2 = panelPopUp.transform.Find("PopUp2").transform.Find("Body").transform.Find("BtnParent").gameObject;

        popup1Btn = _popup1.transform.Find("Btn").GetComponent<Button>();
        popup2BtnYes = _popup2.transform.Find("BtnYes").GetComponent<Button>();
        popup2BtnNo = _popup2.transform.Find("BtnNo").GetComponent<Button>();
        popup1Back = panelPopUp.transform.Find("PopUp1/Back").GetComponent<Button>();
        popup2Back = panelPopUp.transform.Find("PopUp2/Back").GetComponent<Button>();

        popup3Btn1 = panelPopUp.transform.Find("PopUp3/Body/BtnParent/BtnFirst").GetComponent<Button>();
        popup3Btn2 = panelPopUp.transform.Find("PopUp3/Body/BtnParent/BtnSecond").GetComponent<Button>();
        popup3Btn3 = panelPopUp.transform.Find("PopUp3/Body/BtnParent/BtnThird").GetComponent<Button>();
        popup3Back = panelPopUp.transform.Find("PopUp3/Back").GetComponent<Button>();
    }

    //매니저 초기화
    void initManager()
    {
        RemoveBtnListener();
        if (panelPopUp.activeSelf == true)
        {
            panelPopUp.SetActive(false);
        }
        nowState = (int)POPUP_STATE.READY;
        bChoosePopup = false;
        //bSelect = false;
        //bOk = false;


    }

    /*
    //팝업에서 유저가 선택했는가?
    public static bool checkSelectPopUp()
    {
        return bSelect;
    }

    //팝업 버튼 결과를 가져온다.
    public static bool getPopUpValue()
    {
        if (nowState == (int)POPUP_STATE.SELECT)
        {
            nowState = (int)POPUP_STATE.CLOSE;
        }
        return bOk;
    }
    */

    //팝업을 요청한다.
    public static bool setPopUpCode(bool bChoose, string yesStr = "", string noStr = "", string header = "", string body = "")
    {
        bool reVal = false;
        if (nowState == (int)POPUP_STATE.READY)
        {
            bChoosePopup = bChoose;
            strHeader = header;
            strBody = body;
            if (string.Equals(strBody, "NULL", StringComparison.OrdinalIgnoreCase) == true)
            {
                //strBody = gText.getBaseText((int)ENUM_BASE.UNKNOWN_ERROR);
            }
            if (bChoose == true)
            {
                strYes = yesStr;
                strNo = noStr;
            }
            else
            {
                strYes = yesStr;
            }
            nowState = (int)POPUP_STATE.REQUEST;
            reVal = true;
        }
        return reVal;
    }

    /*

public static void setPopUp(bool bChoose, string sHeader, string sBody, string sYes, string sNo, int yesSceneState = -1, int noSceneState = -1)
{
    if (nowState == (int)POPUP_STATE.READY)
    {
        bChoosePopup = bChoose;
        strHeader = sHeader;
        strBody = sBody;
        if (bChoose == true)
        {
            if (sYes != "")
            {
                strYes = sYes;
            }
            else
            {
                strYes = gText.getBaseText((int)ENUM_BASE.CONFIRM);
            }
            if (sNo != "")
            {
                strNo = sNo;
            }
            else
            {
                strNo = gText.getBaseText((int)ENUM_BASE.CANCEL);
            }
            returnSceneStateYes = yesSceneState;
            returnSceneStateNo = noSceneState;
        }
        else
        {
            if (sYes != "")
            {
                strYes = sYes;
            }
            else
            {
                strYes = gText.getBaseText((int)ENUM_BASE.CONFIRM);
            }
            strNo = "";
            returnSceneStateYes = yesSceneState;
            returnSceneStateNo = yesSceneState;
        }
        nowState = (int)POPUP_STATE.REQUEST;
    }
}
public static void setPopUpText(int yesSceneState, int noSceneState, bool bChoose, int header, string cText)
{
    if (nowState == (int)POPUP_STATE.READY)
    {
        bChoosePopup = bChoose;
        strHeader = gText.getUiText(header);
        strBody = cText;
        if (bChoose == true)
        {
            strYes = gText.getBaseText((int)ENUM_BASE.CONFIRM);
            strNo = gText.getBaseText((int)ENUM_BASE.CANCEL);
            returnSceneStateYes = yesSceneState;
            returnSceneStateNo = noSceneState;
        }
        else
        {
            strYes = gText.getBaseText((int)ENUM_BASE.CONFIRM);
            strNo = "";
            returnSceneStateYes = yesSceneState;
            returnSceneStateNo = yesSceneState;
        }
        nowState = (int)POPUP_STATE.REQUEST;
    }
}

public static void setPopUpCodeAndBtn(int yesSceneState, int noSceneState, bool bChoose, int header, int textCode, int yesCode, int noCode)
{
    if (nowState == (int)POPUP_STATE.READY)
    {
        bChoosePopup = bChoose;
        strHeader = gText.getUiText(header);
        strBody = gText.getUiText(textCode);
        if (bChoose == true)
        {
            strYes = gText.getUiText(yesCode);
            strNo = gText.getUiText(noCode);
            returnSceneStateYes = yesSceneState;
            returnSceneStateNo = noSceneState;
        }
        else
        {
            strYes = gText.getUiText(yesCode);
            strNo = "";
            returnSceneStateYes = yesSceneState;
            returnSceneStateNo = yesSceneState;
        }
        nowState = (int)POPUP_STATE.REQUEST;
    }
}
*/

    //백 키 터치 시 해당하는 액션을 처리하고 팝업을 종료한다.
    void setBackKeySelect()
    {
        if (nowState != (int)POPUP_STATE.INPUT_KEY)
        {
            nowState = (int)POPUP_STATE.INPUT_KEY;
        }
    }

    void setBackKeyAction()
    {
        if (nowState == (int)POPUP_STATE.INPUT_KEY)
        {
            nowState = (int)POPUP_STATE.SELECT;
            if (backKeyAction != null)
            {
                backKeyAction.Invoke();
                backKeyAction = null;
            }
            setPopUpClose();
        }
    }


    public static void setPopUpClose()
    {
        //if (nowState == (int)POPUP_STATE.SELECT)
        //{
        //    nowState = (int)POPUP_STATE.CLOSE;
        //}
        nowState = (int)POPUP_STATE.CLOSE;
        //if (nowState == (int)POPUP_STATE.SHOWING)
        //{
        //  nowState = (int)POPUP_STATE.SELECT;
        //  bSelect = true;
        //  bOk = bYes;
        //}
    }

    /*
    //돌아갈 상태
    public static int getYesState()
    {
        return returnSceneStateYes;
    }
    public static int getNoState()
    {
        return returnSceneStateNo;
    }
    */

    //원버튼 팝업
    void showPopUp1()
    {
        GameObject body = panelPopUp.transform.Find("PopUp1").transform.Find("Body").gameObject;
        panelPopUp.transform.Find("PopUp1").gameObject.SetActive(true);
        panelPopUp.transform.Find("PopUp2").gameObject.SetActive(false);
        panelPopUp.transform.Find("PopUp3").gameObject.SetActive(false);
        panelPopUp.SetActive(true);

        //gText.setText(body.transform.Find("TextHeader").gameObject, strHeader);
        //gText.setText(body.transform.Find("TextBodyBg").transform.Find("TextBody").gameObject, strBody);
        //gText.setText(body.transform.Find("Btn").transform.Find("Text").gameObject, strYes);

        body.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
        body.GetComponent<ContentSizeFitter>().SetLayoutVertical();
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(body.GetComponent<RectTransform>());
    }
    //투버튼 팝업
    void showPopUp2()
    {
        GameObject body = panelPopUp.transform.Find("PopUp2").transform.Find("Body").gameObject;
        panelPopUp.transform.Find("PopUp1").gameObject.SetActive(false);
        panelPopUp.transform.Find("PopUp2").gameObject.SetActive(true);
        panelPopUp.transform.Find("PopUp3").gameObject.SetActive(false);

        panelPopUp.SetActive(true);

        //gText.setText(body.transform.Find("TextHeader").gameObject, strHeader);
        //gText.setText(body.transform.Find("TextBodyBg").transform.Find("TextBody").gameObject, strBody);
        //gText.setText(body.transform.Find("BtnParent").transform.Find("BtnYes").transform.Find("Text").gameObject, strYes);
        //gText.setText(body.transform.Find("BtnParent").transform.Find("BtnNo").transform.Find("Text").gameObject, strNo);

        body.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
        body.GetComponent<ContentSizeFitter>().SetLayoutVertical();
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(body.GetComponent<RectTransform>());
    }



    //팝업1에서 confirm버튼 터치
    public void clickBtnOK()
    {
        /*
        if(nowState == (int)POPUP_STATE.SHOWING)
        {
            nowState = (int)POPUP_STATE.SELECT;
            bSelect = true;
            bOk = true;
        }
        */
    }

    //팝업2에서 yes버튼 터치
    public void clickBtnYes()
    {
        /*
        if (nowState == (int)POPUP_STATE.SHOWING)
        {
            nowState = (int)POPUP_STATE.SELECT;
            bSelect = true;
            bOk = true;
        }
        */
    }

    //팝업2에서 no버튼 터치
    public void clickBtnNo()
    {
        /*
        if (nowState == (int)POPUP_STATE.SHOWING)
        {
            nowState = (int)POPUP_STATE.SELECT;
            bSelect = true;
            bOk = false;
        }
        */
    }

    public static void AddMethodToBtn(Action yesAction, Action noAction = null)
    {
        //RemoveBtnListener();
        if (noAction == null)
        {
            popup1Btn.onClick.AddListener(async () => {
                nowState = (int)POPUP_STATE.SELECT;
                yesAction.Invoke(); setPopUpClose();
            });
            popup1Back.onClick.AddListener(async () => {
                nowState = (int)POPUP_STATE.SELECT;
                yesAction.Invoke(); setPopUpClose();
            });
            //백 키 액션 등록
            backKeyAction = yesAction;
        }
        else
        {
            popup2BtnYes.onClick.AddListener(() => {
                nowState = (int)POPUP_STATE.SELECT;
                ; yesAction.Invoke(); setPopUpClose();
            });
            popup2BtnNo.onClick.AddListener(() => {
                nowState = (int)POPUP_STATE.SELECT; 
                noAction.Invoke(); setPopUpClose();
            });
            popup2Back.onClick.AddListener(() => {
                nowState = (int)POPUP_STATE.SELECT; 
                noAction.Invoke(); setPopUpClose();
            });
            //백 키 액션 등록
            backKeyAction = noAction;
        }
    }

    public static void RemoveBtnListener()
    {
        popup1Btn.onClick.RemoveAllListeners();
        popup2BtnYes.onClick.RemoveAllListeners();
        popup2BtnNo.onClick.RemoveAllListeners();
        popup1Back.onClick.RemoveAllListeners();
        popup2Back.onClick.RemoveAllListeners();
    }

    //Method Add by SeungHwan 241023

    public static void setThreeBtnPopUp(string firstBtnStr = "", string secondBtnStr = "", string thirdBtnStr = "", string header = "", string body = "")
    {
        strHeader = header;
        strBody = body;
        strFirst = firstBtnStr;
        strSecond = secondBtnStr;
        strThird = thirdBtnStr;

        nowState = (int)POPUP_STATE.THREEBTNPOPUP;
    }

    public static void RemoveThreeBtnListener()
    {
        popup3Btn1.onClick.RemoveAllListeners();
        popup3Btn2.onClick.RemoveAllListeners();
        popup3Btn3.onClick.RemoveAllListeners();
        popup3Back.onClick.RemoveAllListeners();
    }

    public static void AddMethodToThreeBtn(Action firstAction, Action secondAction, Action thirdAction)
    {
        RemoveThreeBtnListener();

        popup3Btn1.onClick.AddListener(() => { nowState = (int)POPUP_STATE.SELECT; firstAction.Invoke(); setPopUpClose(); });
        popup3Btn2.onClick.AddListener(() => { nowState = (int)POPUP_STATE.SELECT; secondAction.Invoke(); setPopUpClose(); });
        popup3Btn3.onClick.AddListener(() => { nowState = (int)POPUP_STATE.SELECT; thirdAction.Invoke(); setPopUpClose(); });
        //popup3Back.onClick.AddListener(async () => { await SoundManager.Instance.SetSound(GSoundScript.BUTTON_CLICK_NEGATIVE, SoundCategory.FX); nowState = (int)POPUP_STATE.SELECT; secondAction.Invoke(); setPopUpClose(); });

        //백 키 액션 등록
        //backKeyAction = secondAction;
    }
    void shopPopUp3()
    {
        GameObject body = panelPopUp.transform.Find("PopUp3").transform.Find("Body").gameObject;
        panelPopUp.transform.Find("PopUp1").gameObject.SetActive(false);
        panelPopUp.transform.Find("PopUp2").gameObject.SetActive(false);
        panelPopUp.transform.Find("PopUp3").gameObject.SetActive(true);
        panelPopUp.SetActive(true);

        //gText.setText(body.transform.Find("TextHeader").gameObject, strHeader);
        //gText.setText(body.transform.Find("TextBodyBg").transform.Find("TextBody").gameObject, strBody);

        Transform btnParent = body.transform.Find("BtnParent");

        //gText.setText(btnParent.transform.Find("BtnFirst").transform.Find("Text").gameObject, strFirst);
        //gText.setText(btnParent.transform.Find("BtnSecond").transform.Find("Text").gameObject, strSecond);
        //gText.setText(btnParent.transform.Find("BtnThird").transform.Find("Text").gameObject, strThird);
    }
}

