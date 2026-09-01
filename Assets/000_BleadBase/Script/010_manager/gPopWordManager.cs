/// [스크립트 명세]-----------------------------------------------------------------------------------------------------------------------
///  단어 정보 UI를 처리하는 매니저
///  로고 씬에서 GameObject 컴포넌트에 등록
/// --------------------------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using System;
using TMPro;

public class gPopWordManager : MonoBehaviour
{
    public GameObject popWord;  //단어 정보 팝업

    private static int mainState = (int)MAIN_STATE.INIT;
    private static int subState = (int)MAIN_STATE.READY;

    private static Action backKeyAction;

    bool bInputHold = false;    //키 입력 시 1프레임 지연 처리를 위한 변수값 (이 값이 없으면 메인 모듈의 update와 중복 처리가 발생할 수 있다.)

    // [mainState: 매니저 상태]
    public enum MAIN_STATE
    {
        INIT = 0,   //초기화
        READY,      //준비됨
        POP_WORD    //단어 정보 팝업 상태
    }

    // [subState: 단어 정보 팝업의 상태]
    public enum SUB_STATE
    {
        READY = 0,      //준비 
        REQUEST,        //요청 
        SHOWING,        //보여주기
        INPUT_KEY,      //키 입력
        //SELECT,         //선택
        CLOSE,          //닫기
    }

    // Awake is called before the first frame update
    void Awake()
    {
        var obj = FindObjectsOfType<gPopWordManager>();
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

    void initManager()
    {
        if (popWord.activeSelf == true)
        {
            popWord.SetActive(false);
        }
        mainState = (int)MAIN_STATE.INIT;
        subState = (int)SUB_STATE.READY;
        initPopWord();
    }



    void Update()
    {
        bInputHold = false;
        //현재 스테이트 상태에 따라 업데이트 처리한다.
        switch (mainState)
        {
            case (int)MAIN_STATE.INIT:
                mainState = (int)MAIN_STATE.READY;
                break;
            case (int)MAIN_STATE.POP_WORD:
                updatePopWord();
                break;
        }
    }

    //[단어 상세 팝업]=================================================================================

    private static string popWordStr = "";
    private static string popWordMeaning = "";
    private static string popWordSentences = "";
    private static Action popWordCloseAction;
    private static Action<long> popWordListenAction;
    private static long popWordIdx = -1;

    void initPopWord()
    {
        popWord.SetActive(false);
        popWordStr = "";
        popWordMeaning = "";
        popWordSentences = "";
        popWordCloseAction = null;
        popWordListenAction = null;
        popWordIdx = -1;
    }

    void updatePopWord()
    {
        //인풋 처리한다.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            switch (subState)
            {
                case (int)SUB_STATE.SHOWING:
                    //setPopWordBackKeyAction();
                    setPopWordBackKeySelect();
                    bInputHold = true;
                    break;
            }
        }
        if(bInputHold == false)
        {
            //현재 스테이트 상태에 따라 업데이트 처리한다.
            switch (subState)
            {
                case (int)SUB_STATE.REQUEST:
                    showPopWord();
                    break;
                case (int)SUB_STATE.INPUT_KEY:
                    setPopWordBackKeyAction();
                    break;
            }
        }
    }

    void setPopWordBackKeySelect()
    {
       if(subState != (int)SUB_STATE.INPUT_KEY)
       {
            subState = (int)SUB_STATE.INPUT_KEY;
       }
    }

    //백 키 터치 시 해당하는 액션을 처리한다.
    void setPopWordBackKeyAction()
    {
        if (subState == (int)SUB_STATE.INPUT_KEY)
        {
            subState = (int)SUB_STATE.CLOSE;
            if (backKeyAction != null)
            {
                backKeyAction.Invoke();
                backKeyAction = null;
            }
            //클릭 사운드 등록
            setPopSoundNegative();
            //팝업 종료
            setPopWordClose();
        }
    }

    //단어 팝업을 종료한다.
    void setPopWordClose()
    {
        initPopWord();
        mainState = (int)MAIN_STATE.READY;
        subState = (int)SUB_STATE.READY;
    }

    //단어 팝업을 요청한다.
    public static bool setPopWord(Action closeAction, Action<long> listenAction, long tIdx)
    {
        bool reVal = false;
        if (mainState == (int)MAIN_STATE.READY && subState == (int)SUB_STATE.READY)
        {
            popWordStr = getWordText((int)tIdx, WordInfoField.WORD);
            popWordMeaning = getWordText((int)tIdx, WordInfoField.KR);
            popWordSentences = getWordText((int)tIdx, WordInfoField.ENGHINT) + "\n" + getWordText((int)tIdx, WordInfoField.KORHINT);
            popWordSentences = popWordSentences.Replace("[", "<color=#fa7f35>");
            popWordSentences = popWordSentences.Replace("]", "</color>");
            popWordCloseAction = closeAction;
            popWordListenAction = listenAction;
            popWordIdx = tIdx;

            mainState = (int)MAIN_STATE.POP_WORD;
            subState = (int)SUB_STATE.REQUEST;
            reVal = true;

            //클릭 사운드 등록
            setPopSoundPositive();
        }
        return reVal;
    }

    async static void setPopSoundPositive()
    {
        await SoundManager.Instance.SetSound(GSoundScript.BUTTON_CLICK_POSITIVE, SoundCategory.FX);
    }

    async static void setPopSoundNegative()
    {
        await SoundManager.Instance.SetSound(GSoundScript.BUTTON_CLICK_NEGATIVE, SoundCategory.FX);
    }


    void showPopWord()
    {
        if (subState == (int)SUB_STATE.REQUEST)
        {
            GameObject body = popWord.transform.Find("Body").gameObject;
            GameObject btnBack = popWord.transform.Find("Back").gameObject;
            GameObject btnClose = body.transform.Find("BtnX").gameObject;
            GameObject btnListen = body.transform.Find("Hearing").gameObject;

            subState = (int)SUB_STATE.SHOWING;
            popWord.SetActive(true);

            gText.setTMPText(body.transform.Find("T_Word").gameObject, popWordStr);
            gText.setText(body.transform.Find("Meaning/T_Head").gameObject, getText(90464));
            gText.setText(body.transform.Find("Sentences/T_Head").gameObject, getText(90465));
            gText.setText(body.transform.Find("Meaning/T_Meaning").gameObject, popWordMeaning, 1.1f);
            gText.setText(body.transform.Find("Sentences/T_Sentences").gameObject, popWordSentences, 1.1f);

            //듣기 버튼 위치 조정
            TMP_Text a = body.transform.Find("T_Word").gameObject.GetComponent<TMP_Text>();
            float b = a.preferredWidth;

            RectTransform pRect = body.GetComponent<RectTransform>();
            RectTransform btnRect = body.transform.Find("Hearing").gameObject.GetComponent<RectTransform>();

            float pWidth = pRect.rect.width - 70.0f;

            float btnX = (btnRect.rect.width / 2.0f) + b + 50.0f;

            if (btnX > pWidth - (btnRect.rect.width / 2.0f))
            {
                btnX = pWidth - (btnRect.rect.width / 2.0f);
            }
            btnRect.anchoredPosition3D = new Vector3(btnX, btnRect.anchoredPosition3D.y, btnRect.anchoredPosition3D.z);

            //키 액션 등록
            if (btnBack.GetComponent<Button>() != null)
            {
                btnBack.GetComponent<Button>().onClick.RemoveAllListeners();
                btnBack.GetComponent<Button>().onClick.AddListener(() => { popWordCloseAction.Invoke(); setPopWordClose(); });
            }
            if (btnClose.GetComponent<Button>() != null)
            {
                btnClose.GetComponent<Button>().onClick.RemoveAllListeners();
                btnClose.GetComponent<Button>().onClick.AddListener(() => { popWordCloseAction.Invoke(); setPopWordClose(); });
            }
            if (btnListen.GetComponent<Button>() != null)
            {
                btnListen.GetComponent<Button>().onClick.RemoveAllListeners();
                btnListen.GetComponent<Button>().onClick.AddListener(() => { popWordListenAction.Invoke(popWordIdx); });
            }
            backKeyAction = popWordCloseAction;
        }
    }

    private static string getText(int code)
    {
        string str = "";

        str = LanguageManager.Instance.GetLangScript(code, LanguageManager.Instance.languageScriptDic);

        return str;
    }

    private static string getWordText(int code, WordInfoField enumVal)
    {
        string str = "";

        str = LanguageManager.Instance.GetWord(code, enumVal);

        return str;
    }

    //--------------------------------------------------------------------------------------------------------------------------------


}
