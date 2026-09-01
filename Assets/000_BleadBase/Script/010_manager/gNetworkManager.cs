/// [스크립트 명세]------------------------------------------------------------------------------------------------------------------------
///  WAS와의 네트워크 접속을 처리하는 매니저
///  로고 씬에서 GameObject 컴포넌트에 등록
/// --------------------------------------------------------------------------------------------------------------------------------------

//#define USE_EscapeURL
//#define USE_TEST_VIEW

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LitJson;
using System;
using UnityEngine.Networking;
using System.IO;
using System.Security.Cryptography;
using Cysharp.Threading.Tasks;
using static gBase;
using Unity.VisualScripting;

public partial class gNetworkManager : MonoBehaviour
{
    public GameObject panelNetLoading;

    private static int nowState;         //현재 상태
    private static bool bActive = false;
    float netTimer;

    bool bRotate = false;

    // [넷스테이트 상태 확인]
    public enum NET_STATE
    {
        READY = 0,  //준비 
        REQUEST,    //요청 
        NETWORKING, //네트워킹
        DONE,       //종료
        CLOSE,      //닫는다
    }

    // [넷스테이트 상태 확인]
    public enum SEND_TYPE
    {
        PARAMETER = 0,  //파라메터 
        JSON,           //Json
        FULL_JSON,       //완성된 JSON데이터
    }

    //리턴코드
    public struct returnStr
    {
        public int code;
        public bool bSuccess;
    }

    //요청 시 사용하는 변수
    private static string api;
    private static string[] sendPost;
    private static string[] sendData;
    private static string sendAllData;
    private static int sendType;            //0:파라메터    1:json
    private static bool bUseDummyData;      //해당 네트워킹에 더미데이터를 사용할 것인지?
    private static bool bSuccessEndNetPanel;//네트워킹 성공 시 넷로딩 패널을 지울지 여부. 연속 네트워킹을 하는 경우 false값이 들어와야 끊어짐이 없다.
    private static bool bRequestOnly;       //true = 응답 받기 위해 대기하지 않는다. false = 대기한다.

    //응답 시 사용하는 변수
    private static string errorMsg;         //에러 메시지
    private static string responseMsg;      //응답 메시지
    private static JsonData responseJson;   //응답 메시지 json변환

#if USE_TEST_VIEW
    //테스트 뷰 관련
    private static string testViewApi;
    private static float testViewSizeY;
#endif
    //[MonoBehaviour 메소드]========================================================================================

    //초기화
    void Awake()
    {
        var obj = FindObjectsOfType<gNetworkManager>();
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

    //상태에 따른 요청 처리
    void Update()
    {
        switch (nowState)
        {
            case (int)NET_STATE.REQUEST:
                if (bUseDummyData == true)
                {
                    dataRequest();
                }
                else
                {
                    if (sendType == (int)SEND_TYPE.JSON)
                    {
                        wwwJsonRequest();
                    }
                    else if (sendType == (int)SEND_TYPE.FULL_JSON)
                    {
                        wwwJsonDataRequest();
                    }
                    else
                    {
                        wwwRequest();
                    }
                }
                break;
            case (int)NET_STATE.NETWORKING:
                break;
            case (int)NET_STATE.CLOSE:
                endNetLoading(false);
                setState((int)NET_STATE.READY);
                break;
        }
        showTestView();
    }

    IEnumerator backRotate()
    {
        while (bRotate == true)// && WWWHelper.coroutine != null)
        {
            netTimer += Time.deltaTime;
            if (netTimer > gBase.netState.waitingNetTime)
            {
                networkTimeOut();
                bRotate = false;
                break;
            }
            else if (netTimer > 0.25f)
            {
                panelNetLoading.transform.Find("Back").gameObject.SetActive(true);
                RectTransform bRect = panelNetLoading.transform.Find("Back").gameObject.GetComponent<RectTransform>();
                float rZ = bRect.localEulerAngles.z;
                rZ -= Time.smoothDeltaTime * 100.0f;
                if (rZ < -360.0f)
                {
                    rZ = 0.0f;
                }
                bRect.localEulerAngles = new Vector3(0, 0, rZ);
            }
            else
            {
                panelNetLoading.transform.Find("Back").gameObject.SetActive(false);
            }
            //yield return new WaitForSeconds(0.1f);
            yield return null;
        }
    }


    //---------------------------------------------------------------------------------------------------------------------



    //[다른 클래스의 요청을 받는 함수]====================================================================================

    //현재 네트워크 메니저를 사용할 수 있는가?
    public static bool bCheckActivate()
    {
        return bActive;
    }

    public static bool bCheckUse()
    {
        if (api.Length <= 0 && nowState == (int)NET_STATE.READY)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //네트워킹 요청
    public static void setRequest(string apiName, string[] sPost, string[] sData, bool bNetworking, bool bNetFinish)
    {
        if (nowState == (int)NET_STATE.READY)
        {
            api = apiName;
#if USE_TEST_VIEW
            testViewApi = apiName;
            testViewSizeY = 0.0f;
#endif
            //Debug.Log(api);
            sendData = new string[sData.Length];
            sendPost = new string[sPost.Length];
            sendPost = sPost;
            sendData = sData;
            //네트워킹을 사용할 수 없는 경우, 더미데이터를 사용한다.
            bUseDummyData = !bNetworking;
            //리퀘스트 성공 시 넷 로딩 페이지를 닫을지 판단한다.
            bSuccessEndNetPanel = bNetFinish;
            //응답 대기한다.
            bRequestOnly = false;
            sendType = (int)SEND_TYPE.JSON;         //JSON으로 요청
            setState((int)NET_STATE.REQUEST);
        }
        else
        {
            Debug.Log("이전 요청이 처리되지 않아 현재 접속 요청을 받을 수 없는 상태입니다. 이전의 요청을 처리하세요.");
        }
    }

    //Json데이터로 네트워킹 요청
    public static void setRequestData(string apiName, string sData, bool bNetworking, bool bNetFinish)
    {
        if (nowState == (int)NET_STATE.READY)
        {
            api = apiName;
#if USE_TEST_VIEW
            testViewApi = apiName;
            testViewSizeY = 0.0f;
#endif
            //Debug.Log(api);
            sendAllData = sData;
            //네트워킹을 사용할 수 없는 경우, 더미데이터를 사용한다.
            bUseDummyData = !bNetworking;
            //리퀘스트 성공 시 넷 로딩 페이지를 닫을지 판단한다.
            bSuccessEndNetPanel = bNetFinish;
            //응답 대기한다.
            bRequestOnly = false;
            //JSON 통짜로 요청
            sendType = (int)SEND_TYPE.FULL_JSON;
            setState((int)NET_STATE.REQUEST);
        }
        else
        {
            Debug.Log("이전 요청이 처리되지 않아 현재 접속 요청을 받을 수 없는 상태입니다. 이전의 요청을 처리하세요.");
        }
    }

    //파라메터로 요청
    public static void setRequestParam(string apiName, string[] sPost, string[] sData, bool bNetworking, bool bNetFinish)
    {
        if (nowState == (int)NET_STATE.READY)
        {
            api = apiName;
#if USE_TEST_VIEW
            testViewApi = apiName;
            testViewSizeY = 0.0f;
#endif
            //Debug.Log(api);
            sendData = new string[sData.Length];
            sendPost = new string[sPost.Length];
            sendPost = sPost;
            sendData = sData;
            //네트워킹을 사용할 수 없는 경우, 더미데이터를 사용한다.
            bUseDummyData = !bNetworking;
            //리퀘스트 성공 시 넷 로딩 페이지를 닫을지 판단한다.
            bSuccessEndNetPanel = bNetFinish;
            //응답 대기한다.
            bRequestOnly = false;
            sendType = (int)SEND_TYPE.PARAMETER;   //파라메터로 요청
            setState((int)NET_STATE.REQUEST);
        }
        else
        {
            Debug.Log("이전 요청이 처리되지 않아 현재 접속 요청을 받을 수 없는 상태입니다. 이전의 요청을 처리하세요.");
        }
    }

    //응답을 받지 않는 요청
    public static void setRequestOnly(string apiName, string[] sPost, string[] sData, bool bNetworking)
    {
        if (nowState == (int)NET_STATE.READY)
        {
            api = apiName;
#if USE_TEST_VIEW
            testViewApi = apiName;
            testViewSizeY = 0.0f;
#endif
            //Debug.Log(api);
            sendData = new string[sData.Length];
            sendPost = new string[sPost.Length];
            sendPost = sPost;
            sendData = sData;
            //네트워킹을 사용할 수 없는 경우, 더미데이터를 사용한다.
            bUseDummyData = !bNetworking;
            //응답 대기하지 않는다
            bRequestOnly = true;
            sendType = (int)SEND_TYPE.JSON;         //JSON으로 요청
            setState((int)NET_STATE.REQUEST);
        }
        else
        {
            Debug.Log("이전 요청이 처리되지 않아 현재 접속 요청을 받을 수 없는 상태입니다. 이전의 요청을 처리하세요.");
        }
    }

    //응답을 받지 않는 Json데이터로 네트워킹 요청
    public static void setRequestOnlyData(string apiName, string sData, bool bNetworking)
    {
        if (nowState == (int)NET_STATE.READY)
        {
            api = apiName;
#if USE_TEST_VIEW
            testViewApi = apiName;
            testViewSizeY = 0.0f;
#endif
            sendAllData = sData;
            //네트워킹을 사용할 수 없는 경우, 더미데이터를 사용한다.
            bUseDummyData = !bNetworking;
            //응답 대기하지 않는다
            bRequestOnly = true;
            //JSON 통짜로 요청
            sendType = (int)SEND_TYPE.FULL_JSON;
            setState((int)NET_STATE.REQUEST);
        }
        else
        {
            Debug.Log("이전 요청이 처리되지 않아 현재 접속 요청을 받을 수 없는 상태입니다. 이전의 요청을 처리하세요.");
        }
    }

    //응답을 받지 않는 파라메터 요청
    public static void setRequestOnlyParam(string apiName, string[] sPost, string[] sData, bool bNetworking)
    {
        if (nowState == (int)NET_STATE.READY)
        {
            api = apiName;
#if USE_TEST_VIEW
            testViewApi = apiName;
            testViewSizeY = 0.0f;
#endif
            sendData = new string[sData.Length];
            sendPost = new string[sPost.Length];
            sendPost = sPost;
            sendData = sData;
            bUseDummyData = !bNetworking;
            bRequestOnly = true;
            sendType = (int)SEND_TYPE.PARAMETER;   //파라메터로 요청
            setState((int)NET_STATE.REQUEST);
        }
        else
        {
            Debug.Log("이전 요청이 처리되지 않아 현재 접속 요청을 받을 수 없는 상태입니다. 이전의 요청을 처리하세요.");
        }
    }

    //네트워킹 결과 확인하고 다시 요청 받을 수 있게 변경한다.
    public static bool getResult()
    {
        if (nowState == (int)NET_STATE.DONE)
        {
            setState((int)NET_STATE.READY);
            return true;
        }
        else
        {
            return false;
        }
    }

    //에러 메시지를 받는다.
    public static string getErrorMsg()
    {
        return errorMsg;
    }

    //리턴한 Json데이터를 받는다.
    public static JsonData getResponseJson()
    {
        return responseJson;
    }

    //리턴한 MSG데이터를 받는다
    public static string getResponseMsg()
    {
        return responseMsg;
    }

    //현재 요청한 API를 받는다.
    public static string getApi()
    {
        return api;
    }

    //API를 클리어한다.
    public static void clearApi()
    {
        Debug.Log("Clear Api");
        api = "";
    }

    public static void networkInit()
    {
        setState((int)NET_STATE.READY);
        clearApi();
    }

    public static void networkClose()
    {
        setState((int)NET_STATE.CLOSE);
        clearApi();
    }

    //---------------------------------------------------------------------------------------------------------------------



    //[메인 처리 함수]=====================================================================================================

    //상태를 초기화한다.
    void initManager()
    {
        //이니셜라이즈
        //if (panelNetLoading.activeSelf == true)
        //{
        panelNetLoading.SetActive(false);
        panelNetLoading.transform.Find("Text").gameObject.SetActive(false);
        //}
        netTimer = 0.0f;
        setState((int)NET_STATE.READY);
        sendType = 0;
        sendAllData = "";
#if USE_TEST_VIEW
        testViewApi = "";
        testViewSizeY = 0.0f;
#endif
        bUseDummyData = false;
        bSuccessEndNetPanel = true;
        bRequestOnly = false;
        bActive = true;
    }

    //매니저 안의 스테이트를 변경합니다
    public static void setState(int val)
    {
        //Debug.Log(string.Format("SetState: {0}", val));
        nowState = val;
    }

    //(int)NET_STATE.REQUEST 상태에서 네트워크 접속을 Json으로 요청
    void wwwJsonRequest()
    {
        if (nowState == (int)NET_STATE.REQUEST)
        {
            string url = "";

            startNetLoading();

            errorMsg = "";
            responseMsg = "";
            responseJson = null;

            //Post Data
            Dictionary<string, string> postData = new Dictionary<string, string>();
            for (int i = 0; i < sendData.Length; i++)
            {
                //Debug.Log(string.Format("{0}, {1}", sendPost[i], sendData[i]));
#if USE_EscapeURL
                postData.Add(sendPost[i], UnityWebRequest.EscapeURL(sendData[i]));
#else
                postData.Add(sendPost[i], sendData[i]);
#endif
            }
            sendAllData = (string)JsonMapper.ToJson(postData);
            //Debug.Log(api);
            Debug.Log(sendAllData);
            //json AES-256 암호화
            sendAllData = gSecurity.EncryptString(sendAllData);

            //URL
            if (gBase.netState.dns == "")
            {
                gBase.setServerDNS();
            }
            url = string.Format("{0}{1}", gBase.netState.dns, api);
            Debug.Log(url);
            if (bUseDummyData == false)
            {
                UnityWebRequest request = UnityWebRequest.PostWwwForm(url, sendAllData);
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(sendAllData);
                request.uploadHandler = new UploadHandlerRaw(jsonToSend);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
                //헤더에 iv값 추가
                request.SetRequestHeader("iv", gSecurity.getIv());
                if (bRequestOnly == true)
                {
                    StartCoroutine(WWWHelper.Instance.sendRequestOnly(666, request));
                    endNetLoading(false);
                    getResult();
                    clearApi();
                }
                else
                {
                    WWWHelper.Instance.OnHttpRequest += wwwResponse;
                    WWWHelper.Instance.sendRequest(100, request);
                }
            }
            else
            {
                dataRequest();
            }
        }
    }

    //(int)NET_STATE.REQUEST 상태에서 네트워크 접속을 JsonAllData로 요청
    void wwwJsonDataRequest()
    {
        if (nowState == (int)NET_STATE.REQUEST)
        {
            string url = "";

            startNetLoading();

            errorMsg = "";
            responseMsg = "";
            responseJson = null;
            //Debug.Log(api);
            Debug.Log(sendAllData);
            //json AES-256 암호화
            sendAllData = gSecurity.EncryptString(sendAllData);

            //URL
            if (gBase.netState.dns == "")
            {
                gBase.setServerDNS();
            }
            url = string.Format("{0}{1}", gBase.netState.dns, api);
            Debug.Log(url);
            if (bUseDummyData == false)
            {
                UnityWebRequest request = UnityWebRequest.PostWwwForm(url, sendAllData);
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(sendAllData);
                request.uploadHandler = new UploadHandlerRaw(jsonToSend);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
                //헤더에 iv값 추가
                request.SetRequestHeader("iv", gSecurity.getIv());
                if (bRequestOnly == true)
                {
                    StartCoroutine(WWWHelper.Instance.sendRequestOnly(666, request));
                    endNetLoading(false);
                    getResult();
                    clearApi();
                    Debug.Log("getResult(); activate");
                }
                else
                {
                    WWWHelper.Instance.OnHttpRequest += wwwResponse;
                    WWWHelper.Instance.sendRequest(100, request);

                }
            }
            else
            {
                dataRequest();
            }
        }
    }

    //(int)NET_STATE.REQUEST 상태에서 네트워크 접속을 파라메터로 요청
    void wwwRequest()
    {
        if (nowState == (int)NET_STATE.REQUEST)
        {
            string url = "";

            startNetLoading();

            errorMsg = "";
            responseMsg = "";
            responseJson = null;

            //Post Data
            Dictionary<string, string> postData = new Dictionary<string, string>();
            for (int i = 0; i < sendData.Length; i++)
            {
                //파라메터 요청 시에는 UnityWebRequest.EscapeURL을 필수적으로 처리한다.
                postData.Add(sendPost[i], UnityWebRequest.EscapeURL(sendData[i]));
            }
            //URL
            if (gBase.netState.dns == "")
            {
                gBase.setServerDNS();
            }
            url = string.Format("{0}{1}", gBase.netState.dns, api);
            Debug.Log(url);
            if (bUseDummyData == false)
            {
                //서버에 POST
                WWWHelper.Instance.OnHttpRequest += wwwResponse;
                //헤더에 iv값 추가해서 post
                WWWHelper.Instance.post(100, url, postData, "iv", gSecurity.getIv());
            }
            else
            {
                dataRequest();
            }

        }
    }

    //네트워크 응답을 받아 해당 리턴 값을 json으로 저장합니다.
    void wwwResponse(int id, UnityWebRequest www)
    {
        WWWHelper.Instance.OnHttpRequest -= wwwResponse;
        responseMsg = www.downloadHandler.text;
        if (www.error != null)
        {
            //Debug.Log(string.Format("[Error {1}] {0}", www.error, www.responseCode));
            try
            {
                responseMsg = gSecurity.DecryptString(responseMsg);
                responseJson = JsonMapper.ToObject(responseMsg);
            }
            catch
            {
                responseMsg = string.Format("<color=red>[Error {1}] {0}</color>", www.error, www.responseCode);
                Debug.Log(responseMsg);
                string _yesStr = LanguageManager.Instance.GetLangScript(90026, LanguageManager.Instance.languageScriptDic);
                string _noStr = LanguageManager.Instance.GetLangScript(90013, LanguageManager.Instance.languageScriptDic);
                string _body = LanguageManager.Instance.GetLangScript(99007, LanguageManager.Instance.languageScriptDic);

                gPopUpManager.setPopUpCode(true, _yesStr, _noStr, "", _body);
                gPopUpManager.AddMethodToBtn(async () =>
                {
                    setState((int)NET_STATE.CLOSE);

                    await UniTask.WaitUntil(() => ReturnNetState() == NET_STATE.READY);

                    setState((int)NET_STATE.REQUEST);
                },

                async () =>
                {
                    if (SceneTracker.Instance.currentScene == GSceneName.TITLE_SCENE || Application.platform == RuntimePlatform.Android)
                    {
                        Application.Quit();
                    }
                    else
                    {
                        await LoadSceneManager.Instance.LoadScene(GSceneName.TITLE_SCENE, async () =>
                        {
                            gNetworkManager.networkInit();
                            await UniTask.WaitUntil(() => ReturnNetState() == NET_STATE.READY);
                        });
                    }
                });
            }
            errorMsg = www.error;
        }
        else
        {
            responseMsg = gSecurity.DecryptString(responseMsg);
            Debug.Log(responseMsg);
            responseJson = JsonMapper.ToObject(responseMsg);
            if (checkReturnSuccessCode(responseJson).bSuccess == true)
            {
                saveReceiveData(responseMsg);
            }
        }
        if (www.error == null && bSuccessEndNetPanel == false)
        {
            //성공한 응답인 경우, 응답 넷로딩 패널을 비활성화시키면 안되는 경우를 처리한다.
            returnStr reVal = checkReturnSuccessCode(responseJson);
            if (reVal.bSuccess == true)
            {
                endNetLoading(true);
            }
            else
            {
                endNetLoading(false);
            }

        }
        else
        {
            endNetLoading(false);
        }
    }

    //----------------------------------------------------------------------------------------------------------------------



    //[이하 UI에서 보여지는 상태 변경]=====================================================================================

    //서버 접속 전에 씬 UI를 네트워크 대기 상태로 만듭니다.
    void startNetLoading()
    {
        netTimer = 0.0f;
        //bRotate = false;
        panelNetLoading.transform.Find("Back").gameObject.SetActive(false);
        panelNetLoading.transform.Find("Text").gameObject.SetActive(false);
        panelNetLoading.GetComponent<Image>().color = new Color32(0, 0, 0, 1);
        setState((int)NET_STATE.NETWORKING);
        panelNetLoading.SetActive(true);
        //gText.setText(panelNetLoading.transform.Find("Text").gameObject, gText.getBaseText((int)ENUM_BASE.NETWORKING));
        RectTransform bRect = panelNetLoading.transform.Find("Back").GetComponent<RectTransform>();
        gUi.setAnchor(bRect, gUi.AnchorPresets.MiddleCenter);
        bRect.sizeDelta = new Vector2(160.0f, 160.0f);
        bRotate = true;
        StartCoroutine(backRotate());
    }

    //서버에서 데이터를 송신받은 다음 씬 UI를 네트워크 상태를 종료 처리합니다.
    void endNetLoading(bool bPanelSetActive)
    {
        netTimer = 0.0f;
        bRotate = false;
        panelNetLoading.transform.Find("Back").gameObject.SetActive(false);
        panelNetLoading.transform.Find("Text").gameObject.SetActive(false);
        panelNetLoading.SetActive(bPanelSetActive);

        setState((int)NET_STATE.DONE);
    }

    //----------------------------------------------------------------------------------------------------------------------



    //[기타 처리]===========================================================================================================

    //시간 지연으로 인해 서버 접속 연결을 종료합니다.
    void networkTimeOut()
    {
        if (WWWHelper.coroutine != null)
        {
            StopCoroutine(WWWHelper.coroutine);
            WWWHelper.Instance.OnHttpRequest -= wwwResponse;
        }
        endNetLoading(false);
        //팝업 처리합니다.
        setNetworkTimeoutPopUp();
    }

    //성공적인 코드 리턴인지 확인
    public static returnStr checkReturnSuccessCode(JsonData json)//, int yesState, int noState)
    {
        returnStr reVal = new returnStr();
        reVal.code = 0;
        reVal.bSuccess = false;

        if (json != null)
        {
            if (json.Keys.Contains("rCode") == true)
            {
                JsonData rCode = json["rCode"];
                reVal.code = Convert.ToInt32(rCode.ToString());
                if (reVal.code < 100000)
                {
                    reVal.bSuccess = true;
                    getAccessInfo(json);
                }
            }
            else
            {
                reVal.bSuccess = true;
                getAccessInfo(json);
            }
        }
        return reVal;
    }

    public static int CheckReturnCode(JsonData json)
    {
        int _rCode = -1;

        if (json != null)
        {
            if (json.Keys.Contains("rCode") == true)
            {
                JsonData rCode = json["rCode"];
                _rCode = Convert.ToInt32(rCode.ToString());
            }
        }

        return _rCode;
    }

    //엑세스 정보를 받아서 저장한다.
    public static void getAccessInfo(JsonData json)
    {
        returnStr reVal = new returnStr();
        reVal.code = 0;
        reVal.bSuccess = false;

        if (json != null)
        {
            if (json.Keys.Contains("accessInfo") == true)
            {
                JsonData aInfo = json["accessInfo"];
                gBase.setUserIndex(gSystem.getInt64FromJson(aInfo, "uIndex"));
                gBase.setLoginToken(gSystem.getStringFromJson(aInfo, "token"));
                gBase.setServerNum(gSystem.getInt32FromJson(aInfo, "server"));
            }
        }

        //[행렬값 받기 테스트]=======================================================
        /*
        if (json != null)
        {
            if (json.Keys.Contains("shop") == true)
            {
                JsonData jShop = json["shop"];
                int[] sArray = gSystem.getintArrayFromJson(jShop, "pageName");
                //'for (int i = 0; i < sArray.Length; i++)
                //{
                //    Debug.Log(sArray[i]);
                //}
            }
        }
        */
        //--------------------------------------------------------------------------
    }

    //----------------------------------------------------------------------------------------------------------------------



    //[더미 데이터 처리 관련]===============================================================================================

    void dataRequest()
    {
        if (api != "")
        {
            gData.getDummyData(api);
            responseMsg = gData.getDummyData(api);
            try
            {
                responseJson = JsonMapper.ToObject(responseMsg);
                Debug.Log(string.Format("API: <b><i>{0}</i></b> <color=red>Warning! This is <b>DUMMY</b> data Request</color>", api));
            }
            catch
            {
                errorMsg = "<color=red>Dummy data fucking error!</color>";
                Debug.Log(errorMsg);
            }
        }
        Debug.Log(responseMsg);
        endNetLoading(false);
    }

    //리스폰스 데이터를 저장한다
    void saveReceiveData(string jsonData)
    {
#if UNITY_EDITOR
        //추후 파일 IO 관련해서는 추가적으로 문제 없게 구현할 것. 필요하면 매니저를 추가로 구현한다.
        /*
        string fPath; 
        //폴더를 생성한다.
        fPath = Path.Combine(Application.dataPath + "/NetData");
        if (Directory.Exists(fPath) == false)
        {
            Directory.CreateDirectory(Application.dataPath + "/NetData");
        }
        fPath = Path.Combine(Application.dataPath + "/NetData/", string.Format("receive_{0}.json", api.Replace("/", "_")));
        if (File.Exists(fPath) == false)
        {
            //파일이 없으면 파일을 생성한다
            File.Create(fPath);
        }
        //파일 깨끗히 정리
        //File.WriteAllText(fPath, String.Empty);
        //데이터 저장
        File.WriteAllText(fPath, jsonData);
        */
#else

#endif
    }

    //----------------------------------------------------------------------------------------------------------------------



    //[네트워킹 테스트 뷰]===============================================================================================

    void showTestView()
    {
        GameObject tView = panelNetLoading.transform.parent.transform.Find("TestView").gameObject;
#if USE_TEST_VIEW
        if (sendAllData != "")
        {
            string str = "";
            RectTransform tTextRect = tView.transform.Find("ScrollView").transform.Find("ScrollPanel").transform.Find("Text").transform.GetComponent<RectTransform>();

            tView.SetActive(true);
            str += string.Format("<size=28>Request API: {0}</size>", testViewApi);
            str += string.Format("\n{0}", sendAllData);
            if (errorMsg != "" || responseMsg != "")
            {
                str += string.Format("\n\n<size=28>Response Data</size>");
            }
            if (errorMsg != "")
            {
                str += string.Format("\nERROR: {0}", errorMsg);
            }
            if (responseMsg != "")
            {
                str += string.Format("\nJson: {0}", responseMsg);
            }
            //str = str.Replace("\\n", "\n");
            if(str.Length > 15000)
            {
                str = str.Substring(0, 15000);
            }
            gText.setText(tView.transform.Find("ScrollView").transform.Find("ScrollPanel").transform.Find("Text").gameObject, str);
            if (testViewSizeY != tTextRect.sizeDelta.y)
            {
                RectTransform sViewRect = tView.transform.Find("ScrollView").transform.GetComponent<RectTransform>();
                RectTransform sPanelRect = tView.transform.Find("ScrollView").transform.Find("ScrollPanel").transform.GetComponent<RectTransform>();
                GameObject sBar = tView.transform.Find("ScrollView").transform.Find("ScrollBar").gameObject;
                //버티컬 페이지 스크롤 처리
                //gSystem.setDynamicVerticalScrollPage(sViewRect, sPanelRect, tTextRect, sBar, 25.0f, 5.0f);
                //testViewSizeY = tTextRect.sizeDelta.y;
                testViewSizeY = gUi.setDynamicVerticalScrollPage(sViewRect, sPanelRect, tTextRect, sBar, 25.0f, 5.0f);
            }
        }
        else
        {
            tView.SetActive(false);
            gText.setText(tView.transform.Find("ScrollView").transform.Find("ScrollPanel").transform.Find("Text").gameObject, "");
        }
#else
        tView.SetActive(false);
#endif
    }

    public void clickBtnHideTestView()
    {
        GameObject tView = panelNetLoading.transform.parent.transform.Find("TestView").gameObject;
        if (nowState == (int)NET_STATE.READY)
        {
            sendAllData = "";
            gText.setText(tView.transform.Find("ScrollView").transform.Find("ScrollPanel").transform.Find("Text").gameObject, "");
        }
    }

    public static NET_STATE ReturnNetState()
    {
        //Debug.Log(nowState);
        return (NET_STATE)nowState;
    }
    //----------------------------------------------------------------------------------------------------------------------



    //[이하 팝업 및 씬 이동 처리]===========================================================================================

    void setNetworkTimeoutPopUp()
    {
        //팝업으로 처리한다.
        string bodyStr = "";
        string yesStr = "";
        yesStr = LanguageManager.Instance.GetLangScript(90013, LanguageManager.Instance.languageScriptDic);
        bodyStr = LanguageManager.Instance.GetLangScript(99007, LanguageManager.Instance.languageScriptDic);
        //팝업을 표시한다.
        gPopUpManager.setPopUpCode(false, yesStr, "", "", bodyStr);
        //메소드를 등록한다.
        gPopUpManager.AddMethodToBtn(setNetworkErrorCloseBtn, null);
    }

    async void setNetworkErrorCloseBtn()
    {
        if (SceneManager.GetActiveScene().name != "01_Title" && SceneManager.GetActiveScene().name != "BleadDev")
        {
            await LoadSceneManager.Instance.LoadScene(GSceneName.TITLE_SCENE, async () =>
            {
                gNetworkManager.networkInit();
                await UniTask.WaitUntil(() => gNetworkManager.ReturnNetState() == gNetworkManager.NET_STATE.READY);
            });
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            Debug.Log("Exit Game!");
#else
            Application.Quit(); // 어플리케이션 종료
#endif
        }
    }



    //----------------------------------------------------------------------------------------------------------------------


}
