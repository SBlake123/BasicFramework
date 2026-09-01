/// [스크립트 명세]-----------------------------------------------------------------------------------------------------
/// ※ 정의 : 프로그램 전체에서 범용/필수적으로 사용하는 정적 클래스 및 함수들을 정리 
/// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using LitJson;
using System;
using UnityEngine.UI;
using System.Net;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

//[ENUM]=========================================================================================================

//마켓 타입
public enum ENUM_MARKET_TYPE
{
    GOOGLE = 0,     //구글
    APPLE,          //애플
    ONESTORE,       //원스토어
    WEB,            //웹
    ALL,            //무관
    SINGLE          //싱글
}
//빌드 타입
public enum ENUM_BUILD_TYPE
{
    DEVELOP = 0,     //개발
    RELEASE          //릴리즈
}
//언어
public enum ENUM_TEXT_TYPE
{
    KR = 0,
    EN,
    JP
}

//플랫폼 로그인 타입
enum PLATFORM_TYPE
{
    GAMEID = -1,
    GOOGLEPLAY = 0,
    GAMECENTER,
    FACEBOOK,
    NONE = -99,
}
//씬
public enum ENUM_SCENE
{
    LOGO = 0,       //로고
    TITLE,          //타이틀
    OPENING,        //오프닝
    MAIN,           //로비
    API_CHECKER,
}

//-------------------------------------------------------------------------------------------------------------------



public class gBase
{
    //[빌드, 마켓 타입 및 버전 정의]=================================================================================
    public static class gameVersion
    {
        //=========================마켓타입===========================
        //[구글]
        public const int marketType = (int)ENUM_MARKET_TYPE.GOOGLE;
        //[IOS]
        //public const int marketType = (int)ENUM_MARKET_TYPE.APPLE;
        //------------------------------------------------------------

        //=========================빌드타입===========================
        //[Dev]
        //public const int buildType = (int)ENUM_BUILD_TYPE.DEVELOP;
        //[Release]
        public const int buildType = (int)ENUM_BUILD_TYPE.RELEASE;
        //------------------------------------------------------------

        //멀티랭퀴지 지원 여부
        public const bool multiLang = false;
        public static string buildVer = string.Format("ver {0}", Application.version);
        public static int wasVer = 1410;//1300;//1000;
    }
    // -------------------------------------------------------------------------------------------------------------



    //[씬 제어값 정의 및 함수]=======================================================================================
    public static class sceneState
    {
        public static int nowScene = (int)ENUM_SCENE.LOGO;
        public static int beforeScene = (int)ENUM_SCENE.LOGO;
    }
    //씬 변경시 식별자 반영
    public static void sceneChange(int val)
    {
        sceneState.beforeScene = sceneState.nowScene;
        sceneState.nowScene = val;
    }
    // ------------------------------------------------------------------------------------------------------------



    //[서버 접속 관련]==============================================================================================
    //[네트워크 값 정의]
    public static class netState
    {
        public const int waitingNetTime = 120;   //서버 응답시간
        public static string dns = "";
        public static string wasName = "";

        public static string privacy_kr = "";
        public static string privacy_en = "";
        public static string term_kr = "";
        public static string term_en = "";
    }
    //[타입에 따라 접근할 서버 셋팅]
    public static void setServerDNSAbsolute(string str)
    {
        gBase.netState.dns = str;
    }

    public static void setServerDNS()
    {
        if (gBase.netState.dns == "")
        {
            string sDns = "";
            switch (gameVersion.buildType)
            {
                case (int)ENUM_BUILD_TYPE.DEVELOP:
                    sDns = string.Format("https://dev.owl-logics.com/000_hht20/dev/d{0}/", gameVersion.wasVer);
                    break;
                case (int)ENUM_BUILD_TYPE.RELEASE:
                    sDns = string.Format("https://hht20.owl-logics.com/000_hht20/release/r{0}/", gameVersion.wasVer);
                    break;
            }
            //string[] verDirectory = string.Format("{0}", Application.version).Split('.');
            //sSub = string.Format("/v{0}{1}/", verDirectory[0], verDirectory[1]);

            gBase.netState.dns = sDns;
        }
    }
    // --------------------------------------------------------------------------------------------------------------



    //[OS의 언어 설정을 받아온다]=================================================================================
    public static int checkOSLanguage()
    {
        if (gBase.gameVersion.multiLang == true)
        {
            string code = Application.systemLanguage.ToString();
            if (code == "Korean")
            {
                return (int)ENUM_TEXT_TYPE.KR;
            }
            else if (code == "Japanese")
            {
                return (int)ENUM_TEXT_TYPE.JP;
            }
            else
            {
                return (int)ENUM_TEXT_TYPE.EN;
            }
        }
        else
        {
            return (int)ENUM_TEXT_TYPE.KR;
        }
    }

    public static string getCountry()
    {
        //string rName = System.Globalization.RegionInfo.CurrentRegion.Name;
        //string cName = System.Globalization.CultureInfo.CurrentCulture.Name;
        //string cname = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
        string cName = System.Globalization.RegionInfo.CurrentRegion.Name;
        if (cName != "KR" && cName != "US" && cName != "VN" && cName != "JP")
        {
            cName = "US";
        }
        return cName;
    }

    public static string getTimeZone()
    {
        string str = "";
        str = string.Format("+{0:D2}:{1:D2}", System.TimeZoneInfo.Local.BaseUtcOffset.Hours, System.TimeZoneInfo.Local.BaseUtcOffset.Minutes);
        //Debug.Log(str);
        return str;
    }

    public static int getUserLang()
    {
        int reVal = EncryptedPlayerPrefs.GetInt(gData.myPrefs.uLang, -1);
        return reVal;
    }

    //플레이어프렙스 암호화 키값 셋팅
    public static void setEnKey()
    {
        //암호화에 쓸 값을 자유로이 셋팅합니다.
        EncryptedPlayerPrefs.keys = new string[5];
        EncryptedPlayerPrefs.keys[0] = "123456";
        EncryptedPlayerPrefs.keys[1] = "ytrewq";
        EncryptedPlayerPrefs.keys[2] = "asdfgh";
        EncryptedPlayerPrefs.keys[3] = "Fucking";
        EncryptedPlayerPrefs.keys[4] = "Blead";
    }

    //----------------------------------------------------------------------------------------------------------



    //[씬 로딩 시 기본 설정]=====================================================================================
    public static void InitScene(int scene)
    {
        sceneChange(scene);
        setServerDNS();
        gBase.setEnKey();
        gText.setLanguage();
        //UDID 등록
#if UNITY_ANDROID
        EncryptedPlayerPrefs.SetString(gData.myPrefs.udId, SystemInfo.deviceUniqueIdentifier);
#elif UNITY_IOS
        EncryptedPlayerPrefs.SetString(gData.myPrefs.udId, Device.vendorIdentifier);
#endif
        PlayerPrefs.Save();

        //사운드 플레이어 초기화
        //gSoundManager.setState((int)gSoundManager.STATE.CLEAR);

        setTimeScale(1.0f);

        //프레임 최적화
        //2019부터 셋팅하지 않으면 프레임을 30으로 최적화시킨다고 한다. (왜째서?)
#if UNITY_IOS || UNITY_ANDROID
        Application.targetFrameRate = 60; 
#else
        QualitySettings.vSyncCount = 1;
#endif

    }

    //----------------------------------------------------------------------------------------------------------



    //[마켓 URL을 받아온다]=====================================================================================
    public static string getMarketURL()
    {
        string url = "";
#if UNITY_ANDROID
        //url = "market://details?id=";
        if (gBase.gameVersion.marketType == (int)ENUM_MARKET_TYPE.GOOGLE)
        {
            url = "market://details?id=com.OwlLogicsKorea.HooHoot20";
        }
        else
        {
            url = "onestore://common/product/";
        }
#elif UNITY_IOS
        url = "itms-apps://itunes.apple.com/app/6737188918";
#endif
        return url;
    }
    //----------------------------------------------------------------------------------------------------------



    //[쿠폰 URL을 받아온다]=====================================================================================

    public static string getCouponURL()
    {
        string url = "";
        if (gBase.gameVersion.buildType == (int)ENUM_BUILD_TYPE.DEVELOP)
        {
            url = string.Format("https://dev.owl-logics.com/000_hht20/coupon?uIndex={0}&token={1}&server={2}", Convert.ToString(gBase.getUserIndex()), gBase.getLoginToken(), Convert.ToString(gBase.getServerNum()));
        }
        else
        {
            url = string.Format("https://hht20.owl-logics.com/000_hht20/coupon?uIndex={0}&token={1}&server={2}", Convert.ToString(gBase.getUserIndex()), gBase.getLoginToken(), Convert.ToString(gBase.getServerNum()));
        }

        return url;
    }


    //----------------------------------------------------------------------------------------------------------



    //[빌드타입 체크]==========================================================================================
    public static string getBuildType()
    {
        string str = "";
        return str;
    }
    //----------------------------------------------------------------------------------------------------------



    //[타임스케일 설정]==========================================================================================
    public static void setTimeScale(float i)
    {
        Time.timeScale = i;
    }
    //----------------------------------------------------------------------------------------------------------



    //[인앱 결제 관련 함수]: 결제하다 앱이 종료되었을 때 복구하기 위한 변수 및 함수 처리=========================
    //[iap 처리를 위한 전역변수]--------------------------------------------------
    private static bool iapBuyItem;
    private static string iapTxId;
    private static string iapSign;
    private static string iapSku;
    //구매 관련 전역변수 초기화
    public static void setIapInit()
    {
        iapBuyItem = false;
        iapTxId = "";
        iapSign = "";
        iapSku = "";
    }
    //현재 구매처리할 데이터를 전역변수에 등록
    public static void setIapPurchase(string txid, string sign, string sku)
    {
        iapBuyItem = true;
        iapTxId = txid;
        iapSign = sign;
        iapSku = sku;
    }
    //--------------------------------------------------------------------------------------------------------



    //[게임 종료]============================================================================================

    public static void Quit()
    {
#if !UNITY_EDITOR
            AndroidJavaClass ajc = new AndroidJavaClass("com.lancekun.quit_helper.AN_QuitHelper");
            AndroidJavaObject UnityInstance = ajc.CallStatic<AndroidJavaObject>("Instance");
            UnityInstance.Call("AN_Exit");
#endif
    }

    //--------------------------------------------------------------------------------------------------------



    //[기타 기본 설정값]======================================================================================
    //[스크롤 민감도 조정]
    public const float scrollSensitive = 0.5f;
    //스크롤 위치 밸류
    public static float posValue = 1.0f;
    //스크롤 체크
    public static bool checkScroll = true;
    //--------------------------------------------------------------------------------------------------------



    //[로그인 정보 저장]======================================================================================
    private static long uIndex = -1;
    private static string loginToken = "";
    private static int serverNum = -1;
    public static void setUserIndex(long input)
    {
        uIndex = input;
    }
    public static long getUserIndex()
    {
        return uIndex;
    }
    public static void setLoginToken(string input)
    {
        loginToken = input;
    }
    public static string getLoginToken()
    {
        return loginToken;
    }
    public static void setServerNum(int input)
    {
        serverNum = input;
    }
    public static int getServerNum()
    {
        return serverNum;
    }

    //--------------------------------------------------------------------------------------------------------

}