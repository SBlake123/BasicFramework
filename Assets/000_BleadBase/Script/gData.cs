/// [스크립트 명세]-----------------------------------------------------------------------------------------------------------------------
///  ※ 정의 : 게임에 들어있는 베이스 데이터를 불러오고 관리 
/// --------------------------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LitJson;
using System;
using UnityEngine.Networking;
using System.IO;


public class gData
{
    //더미데이터를 얻는다.
    public static string getDummyData(string fileName)
    {
        string reStr = "";
        fileName = fileName.Replace("/", ".");
        TextAsset tMaster = (TextAsset)Resources.Load(string.Format("000_TestData/{0}", fileName));
        if (tMaster != null)
        {
            reStr = tMaster.text;
        }
        else
        {
            reStr = string.Format("DummyApi:<b><i>{0}</i></b> Not Found", fileName);
            Debug.Log(reStr);
        }

        return reStr;
    }

    /// ============================================================================================================
    /// <summary>
    /// [플레이어프렙스 관련]
    /// myPrefs 클래스       : 사용하는 플레이어프렙스 저장값 정의
    /// setEnKey()          : 암호화 키값 셋팅. 씬 awake때 불러줄 것.
    /// </summary>
    public static class myPrefs
    {
        //유저정보---------------------------------------------------
        public const string udId        = "udId";
        //public const string uId       = "uId";
        //public const string uPass     = "uPass";
        //public const string pId       = "pId";
        //public const string loginType = "loginType";
        //셋팅-------------------------------------------------------
        public const string uLang       = "uLang";
        public const string uVoice      = "uVoice";
        public const string vMusic      = "musicVolume";
        public const string vSoundFx    = "soundFxVolume";
        //인앱 관련
        //public const string uIndex      = "uIndex";
        //public const string token       = "token";
        //public const string server      = "server";
        //public const string market      = "market";
        //public const string sku         = "sku";
        //public const string txid        = "txid";
        //public const string sign        = "sign";
    }

    /// ----------------------------------------------------------------------------------------------------------
    /// 

    //인 앱 결제 관련=============================================================================================

    //[구글 안드로이드 구매 키값 정의]
    public static class purchaseKey
    {
        public static string key = "";
    }
    public static void setIAPKey()
    {

    }
    // ----------------------------------------------------------------------------------------------------------



}