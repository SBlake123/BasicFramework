/// [스크립트 명세]-----------------------------------------------------------------------------------------------------------------------
/// ※ 정의 : 공통으로 사용할 수 있는 텍스트 관련 함수들 
/// --------------------------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using LitJson;
using System;
using UnityEngine.UI;
using TMPro;


//언어팩 Enum 셋팅
public enum LANG_PACK
{
    BASE = 0,       //베이스
    CONTENTS,       //컨텐츠
    UI,             //UI
    CHARACTER,      //캐릭터
    SCENARIO,       //시나리오
}

public class gText
{
    //[변수]==================================================================================

    //로드할 언어팩
    public static class loadLangPack
    {
        public static UnityEngine.TextAsset basePack = (UnityEngine.TextAsset)Resources.Load("B010_Text/basePack");
        public static JsonData jBase = JsonMapper.ToObject(basePack.text);
    }

    //폰트
    private static Font gFont;

    //기타 설정
    private static int language = (int)ENUM_TEXT_TYPE.KR;        //언어 셋팅
    private static int voice = (int)ENUM_TEXT_TYPE.KR;          //음성 언어 셋팅
    private const int langCount = 1;                             //언어의 개수

    //------------------------------------------------------------------------------------------



    //[Setting]==================================================================================

    //언어 설정
    public static void setLanguage()
    {
        if (gBase.gameVersion.multiLang == true)
        {
            if (EncryptedPlayerPrefs.HasKey(gData.myPrefs.uLang) == true)
            {
                language = EncryptedPlayerPrefs.GetInt(gData.myPrefs.uLang, (int)ENUM_TEXT_TYPE.EN);
                voice = EncryptedPlayerPrefs.GetInt(gData.myPrefs.uVoice, (int)ENUM_TEXT_TYPE.EN);
            }
            else
            {
                language = (int)ENUM_TEXT_TYPE.EN;
                voice = (int)ENUM_TEXT_TYPE.EN;
            }
        }
        else
        {
            language = (int)ENUM_TEXT_TYPE.KR;
            voice = (int)ENUM_TEXT_TYPE.KR;
        }
        setFont();
    }

    //폰트 설정
    public static void setFont()
    {
        /*
        if (language == (int)ENUM_TEXT_TYPE.KR || language == (int)ENUM_TEXT_TYPE.EN)
            gFont = Resources.Load<Font>("B020_Font/ggSans_M");
        else
            gFont = Resources.Load<Font>("B020_Font/NotoSans");
        */
    }

    //------------------------------------------------------------------------------------------



    //[GetText]==================================================================================

    //[Base Text return]
    public static string getBaseText(int val)
    {
        JsonData text = loadLangPack.jBase["base"];
        return getStrData(text, val);
    }
    public static string getBaseText(int val, int lang)
    {
        JsonData text = loadLangPack.jBase["base"];
        return getStrData(text, val, lang);
    }

    //[All Text Return]
    public static string getText(int lPack, int val)
    {   
        JsonData text = getLangPackData(lPack);
        return getStrData(text, val);
    }
    public static string getText(int lPack, int val, int lang)
    {
        JsonData text = getLangPackData(lPack);
        return getStrData(text, val, lang);
    }

    //--------------------------------------------------------------------------------------



    //[GetText 보조 함수]===================================================================

    //언어팩
    private static JsonData getLangPackData(int lPack)
    {
        JsonData text = new JsonData();
        switch (lPack)
        {
            case (int)LANG_PACK.BASE:          //베이스
                text = loadLangPack.jBase["base"];
                break;
            case (int)LANG_PACK.CONTENTS:      //컨텐츠
                text = "";
                break;
            case (int)LANG_PACK.UI:            //UI
                text = "";
                break;
            case (int)LANG_PACK.CHARACTER:     //캐릭터
                text = "";
                break;
            case (int)LANG_PACK.SCENARIO:       //시나리오
                text = "";
                break;
        }
        return text;
    }

    //언어팩에서 언어 셋팅에 따라 해당 val값을 바탕으로 리턴
    private static string getStrData(JsonData text, int val)
    {   //언어값을 리턴받는 곳에서 사용
        int lang = language;
        int textCount = 0;
        int textIdx = 0;
        string str = "NULL";

        textCount = text.Count;
        for (int i = 0; i < textCount; i++)
        {
            textIdx = Convert.ToInt32(text[i]["idx"].ToString());
            if (textIdx == val)
            {
                switch (lang)
                {
                    case (int)ENUM_TEXT_TYPE.KR:
                        str = text[i]["kr"].ToString();
                        break;
                    case (int)ENUM_TEXT_TYPE.JP:
                        str = text[i]["jp"].ToString();
                        break;
                    default:
                        str = text[i]["en"].ToString();
                        break;
                }
                break;
            }
        }
        str = str.Replace("\\n", "\n");
        return str;
    }

    //언어값을 직접 지정해서 val값에 따라 리턴
    public static string getStrData(JsonData text, int val, int selLang)
    {   //언어값을 직접 지정
        int textCount = 0;
        int textIdx = 0;
        string str = "NULL";

        textCount = text.Count;
        for (int i = 0; i < textCount; i++)
        {
            textIdx = Convert.ToInt32(text[i]["idx"].ToString());
            if (textIdx == val)
            {
                switch (selLang)
                {
                    case (int)ENUM_TEXT_TYPE.KR:
                        str = text[i]["kr"].ToString();
                        break;
                    case (int)ENUM_TEXT_TYPE.JP:
                        str = text[i]["jp"].ToString();
                        break;
                    default:
                        str = text[i]["en"].ToString();
                        break;
                }
                break;
            }
        }
        str = str.Replace("\\n", "\n");

        return str;
    }

    //---------------------------------------------------------------------------------------------------------



    //[텍스트 출력 관련]=======================================================================================

    //텍스트 셋팅
    public static void setText(GameObject textObj, string text = "")
    {
        if (textObj != null)
        {
            //줄바꿈 예쁘게 개선      
            if (text != "" || text != null) text = text.Replace(' ', '\u00A0');

            //textObj.GetComponent<Text>().font = gFont;
            float ly = 1.3f;
            switch (language)
            {
                case (int)ENUM_TEXT_TYPE.KR:
                    ly = 1.3f;
                    break;
                case (int)ENUM_TEXT_TYPE.JP:
                    ly = 1.2f;
                    break;
                default:
                    ly = 1.3f;
                    break;
            }
            if (textObj.GetComponent<TMP_Text>() != null)
            {
                textObj.GetComponent<TMP_Text>().lineSpacing = ly;
                textObj.GetComponent<TMP_Text>().text = text;
            }
            else if (textObj.GetComponent<Text>() != null)
            {
                textObj.GetComponent<Text>().lineSpacing = ly;
                textObj.GetComponent<Text>().text = text;
            }
        }
    }
    //텍스트 셋팅 (라인 스페이스 지정)
    public static void setText(GameObject textObj, string text, float space)
    {
        //줄바꿈 예쁘게 개선
        text = text.Replace(' ', '\u00A0');

        //textObj.GetComponent<Text>().font = gFont;
        textObj.GetComponent<Text>().lineSpacing = space;
        textObj.GetComponent<Text>().text = text;
    }
    //인풋 텍스트 셋팅
    public static void setInputText(GameObject textObj, string text)
    {
        //textObj.transform.Find("Placeholder").GetComponent<Text>().font = gFont;
        //textObj.transform.Find("Text").GetComponent<Text>().font = gFont;
        setText(textObj.transform.Find("Placeholder").gameObject, "");
        textObj.GetComponent<InputField>().text = text;
    }

    public static void setInputTextAndPlaceholder(GameObject textObj, string text, string pText)
    {
        //textObj.transform.Find("Placeholder").GetComponent<Text>().font = gFont;
        //textObj.transform.Find("Text").GetComponent<Text>().font = gFont;
        setText(textObj.transform.Find("Placeholder").gameObject, pText);
        textObj.GetComponent<InputField>().text = text;
    }

    public static void setTMPText(GameObject textObj, string text = "")
    {
        //줄바꿈 예쁘게 개선      
        if (text != "" || text != null) text = text.Replace(' ', '\u00A0');

        //textObj.GetComponent<Text>().font = gFont;
        float ly = 1.3f;
        switch (language)
        {
            case (int)ENUM_TEXT_TYPE.KR:
                ly = 1.3f;
                break;
            case (int)ENUM_TEXT_TYPE.JP:
                ly = 1.2f;
                break;
            default:
                ly = 1.3f;
                break;
        }
        textObj.GetComponent<TMP_Text>().lineSpacing = ly;
        textObj.GetComponent<TMP_Text>().text = text;
    }


    //-----------------------------------------------------------------------------------------------------------


    //[초를 받아 시간 문자열을 리턴하는 함수들]==================================================================
    //초를 받아 시, 분, 초를 나눈 문자열을 리턴하는 함수
    public static string getTimeString(double time)
    {
        int hour = Convert.ToInt32((time / 3600) - 0.5f);
        int min = Convert.ToInt32(((time - (hour * 3600)) / 60) - 0.5f);
        int sec = Convert.ToInt32(((time - (hour * 3600) - (min * 60))) - 0.5f);

        if (sec >= 60)
        {
            sec = sec - 60;
            min = min + 1;
        }
        if (min >= 60)
        {
            min = min - 60;
            hour = hour + 1;
        }

        string str = "";
        if (hour > 0)
        {
            str += string.Format(gText.getBaseText((int)ENUM_BASE.HOUR), hour);
        }
        if (min > 0)
        {
            if (hour > 0)
                str += " ";

            str += string.Format(gText.getBaseText((int)ENUM_BASE.MIN), min);
        }
        if (sec > 0)
        {
            if (hour > 0 || min > 0)
                str += " ";

            str += string.Format(gText.getBaseText((int)ENUM_BASE.SEC), sec);
        }
        return str;
    }
    //날짜까지 표시
    public static string getTimeStringDay(double time)
    {
        int day = Convert.ToInt32((time / 86400) - 0.5f);
        int hour = Convert.ToInt32(((time - (day * 86400)) / 3600) - 0.5f);
        int min = Convert.ToInt32(((time - (day * 86400) - (hour * 3600)) / 60) - 0.5f);
        int sec = Convert.ToInt32(((time - (day * 86400) - (hour * 3600) - (min * 60))) - 0.5f);

        if (sec >= 60)
        {
            sec = sec - 60;
            min = min + 1;
        }
        if (min >= 60)
        {
            min = min - 60;
            hour = hour + 1;
        }
        if (hour >= 24)
        {
            hour = hour - 24;
            day = day + 1;
        }

        string str = "";
        if (day > 0)
        {
            str += string.Format(gText.getBaseText((int)ENUM_BASE.DAY), day);
        }
        if (hour > 0)
        {
            if (day > 0)
                str += " ";

            str += string.Format(gText.getBaseText((int)ENUM_BASE.HOUR), hour);
        }
        if (min > 0)
        {
            if (day > 0 || hour > 0)
                str += " ";

            str += string.Format(gText.getBaseText((int)ENUM_BASE.MIN), min);
        }
        if (sec > 0)
        {
            if (day > 0 || hour > 0 || min > 0)
                str += " ";

            str += string.Format(gText.getBaseText((int)ENUM_BASE.SEC), sec);
        }
        return str;
    }
    //간략하게 :로 표시
    public static string getTimeStringSimple(double time)
    {
        if (time < 0.0f)
            time = 0.0f;

        int hour = Convert.ToInt32((time / 3600) - 0.5f);
        int min = Convert.ToInt32(((time - (hour * 3600)) / 60) - 0.5f);
        int sec = Convert.ToInt32(((time - (hour * 3600) - (min * 60))) - 0.5f);

        if (sec >= 60)
        {
            sec = sec - 60;
            min = min + 1;
        }
        if (min >= 60)
        {
            min = min - 60;
            hour = hour + 1;
        }

        string str = "";
        if (hour > 24)
        {
            int day = Convert.ToInt32((time / 86400) - 0.5f);
            str += string.Format(gText.getBaseText((int)ENUM_BASE.DAY), day + 1);
        }
        else
        {
            if (hour > 0)
            {
                str += string.Format("{0}", hour);
            }
            if (min > 0)
            {
                if (hour > 0)
                    str += ":";

                if (min >= 10)
                    str += string.Format("{0}", min);
                else
                    str += string.Format("0{0}", min);
            }
            else
            {
                if (hour > 0)
                    str += ":";
                str += string.Format("00");
            }
            if (sec > 0)
            {
                //if (hour > 0 || min > 0)
                str += ":";

                if (sec >= 10)
                    str += string.Format("{0}", sec);
                else
                    str += string.Format("0{0}", sec);
            }
            else
            {
                str += ":";
                str += string.Format("00");
            }
        }
        return str;
    }
    //날짜 없이 간략하게 :로 표시
    public static string getTimeStringSimpleNoDay(double time)
    {
        int hour = Convert.ToInt32((time / 3600) - 0.5f);
        int min = Convert.ToInt32(((time - (hour * 3600)) / 60) - 0.5f);
        int sec = Convert.ToInt32(((time - (hour * 3600) - (min * 60))) - 0.5f);

        if (sec >= 60)
        {
            sec = sec - 60;
            min = min + 1;
        }
        if (min >= 60)
        {
            min = min - 60;
            hour = hour + 1;
        }

        string str = "";
        if (hour > 0)
        {
            str += string.Format("{0}", hour);
        }
        if (min > 0)
        {
            if (hour > 0)
                str += ":";

            if (min >= 10)
                str += string.Format("{0}", min);
            else
                str += string.Format("0{0}", min);
        }
        else
        {
            if (hour > 0)
                str += ":";
            str += string.Format("00");
        }
        if (sec > 0)
        {
            //if (hour > 0 || min > 0)
            str += ":";

            if (sec >= 10)
                str += string.Format("{0}", sec);
            else
                str += string.Format("0{0}", sec);
        }
        else
        {
            str += ":";
            str += string.Format("00");
        }
        return str;
    }
    //----------------------------------------------------------------------------------------------------------



    //[숫자값 리턴]=============================================================================================

    public static string convertKMBNumber(float val)
    {
        string reVal = "";
        if (val >= 1000000000)
        {
            reVal = string.Format("{0}B", (int)(val / 10000000) / 100.0f); 
        }
        else if (val >= 10000000)
        {
            reVal = string.Format("{0}M", (int)(val / 1000000));
        }
        else if(val >= 1000000)
        {
            reVal = string.Format("{0}M", (int)(val / 100000) / 10.0f);
        }
        else if (val >= 100000)
        {
            reVal = string.Format("{0}K", (int)(val / 1000.0f));
        }
        else if (val >= 1000)
        {
            reVal = string.Format("{0}K", (int)(val / 100) / 10.0f);
        }
        else
        {
            reVal = string.Format("{0}", (int)(val));
        }

        return reVal;
    }



    //----------------------------------------------------------------------------------------------------------
}
