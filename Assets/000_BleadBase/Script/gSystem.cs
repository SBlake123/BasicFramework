/// [스크립트 명세]-----------------------------------------------------------------------------------------------------------------------
/// ※ 정의 : 공통으로 사용할 수 있는 시스템 관련 함수들 
/// --------------------------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using LitJson;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

public class gSystem
{
    //[Json처리]=================================================================================================

    //json에서 하위 json을 찾는다.
    public static JsonData getJsonData(JsonData json, string key)
    {
        JsonData reJson = null;

        if (json != null && json.IsObject == true)
        {
            if (json.Keys.Contains(key) == true)
            {
                reJson = json[key];
            }
            else
            {
                string errorMsg = string.Format("<color=red>Can Not Find Json From This Json Key: {0}</color>", key);
                Debug.Log(errorMsg);
            }
        }
        else
        {
            string errorMsg = string.Format("<color=red>Json is empty</color>");
            Debug.Log(errorMsg);
        }

        return reJson;
    }

    //불리언 밸류에 따라 bool값을 리턴한다.
    public static bool getBooleanValue(string key)
    {
        bool reVal = false;
        if(key == "Y")
        {
            reVal = true;
        }
        return reVal;
    }

    //json에서 스트링 추출
    public static string getStringFromJson(JsonData jData, string sKey)
    {
        string reVal = "";
        if (jData.Keys.Contains(sKey) == true)
        {
            if (jData[sKey] != null)
            {
                reVal = jData[sKey].ToString();
            }
            else
            {
                string errorMsg = string.Format("<color=red>This Json Key Value is NULL: {0}</color>", sKey);
                Debug.Log(errorMsg);
            }
        }
        else
        {
            string errorMsg = string.Format("<color=red>Can Not Find String From This Json Key: {0}</color>", sKey);
            Debug.Log(errorMsg);
        }
        return reVal;
    }

    //json에서 인트 추출
    public static int getInt32FromJson(JsonData jData, string sKey)
    {
        int reVal = -1;
        if (jData.Keys.Contains(sKey) == true)
        {
            if (jData[sKey] != null)
            {
                reVal = Mathf.CeilToInt(Convert.ToSingle(jData[sKey].ToString()));
            }
            else
            {
                string errorMsg = string.Format("<color=red>This Json Key Value is NULL: {0}</color>", sKey);
                Debug.Log(errorMsg);
            }
        }
        else
        {
            string errorMsg = string.Format("<color=red>Can Not Find Int From This Json Key: {0}</color>", sKey);
            Debug.Log(errorMsg);
        }
        return reVal;
    }

    //json에서 long 인트 추출
    public static long getInt64FromJson(JsonData jData, string sKey)
    {
        long reVal = -1;
        if (jData.Keys.Contains(sKey) == true)
        {
            if (jData[sKey] != null)
            {
                reVal = Convert.ToInt64(jData[sKey].ToString());
            }
            else
            {
                string errorMsg = string.Format("<color=red>This Json Key Value is NULL: {0}</color>", sKey);
                Debug.Log(errorMsg);
            }
        }
        else
        {
            string errorMsg = string.Format("<color=red>Can Not Find Int From This Json Key: {0}</color>", sKey);
            Debug.Log(errorMsg);
        }
        return reVal;
    }

    //json에서 플로트 추출
    public static float getFloatFromJson(JsonData jData, string sKey)
    {
        float reVal = -1;
        if (jData.Keys.Contains(sKey) == true)
        {
            if (jData[sKey] != null)
            {
                reVal = Convert.ToSingle(jData[sKey].ToString());
            }
            else
            {
                string errorMsg = string.Format("<color=red>This Json Key Value is NULL: {0}</color>", sKey);
                Debug.Log(errorMsg);
            }
        }
        else
        {
            string errorMsg = string.Format("<color=red>Can Not Find Float From This Json Key: {0}</color>", sKey);
            Debug.Log(errorMsg);
        }
        return reVal;
    }

    //json에서 int행렬 추출
    public static int[] getintArrayFromJson(JsonData jData, string sKey)
    {
        JsonData arrData = gSystem.getJsonData(jData, sKey);
        int[] sArray = new int[arrData.Count];
        for (int i = 0; i < arrData.Count; i++)
        {
            sArray[i] = Convert.ToInt32(arrData[i].ToString());
            //Debug.Log(sArray[i]);
        }
        return sArray;
    }

    //json에서 long행렬 추출
    public static long[] getlongArrayFromJson(JsonData jData, string sKey)
    {
        JsonData arrData = gSystem.getJsonData(jData, sKey);
        long[] sArray = new long[arrData.Count];
        for (int i = 0; i < arrData.Count; i++)
        {
            sArray[i] = Convert.ToInt64(arrData[i].ToString());
            //Debug.Log(sArray[i]);
        }
        return sArray;
    }

    //json에서 string행렬 추출
    public static string[] getStringArrayFromJson(JsonData jData, string sKey)
    {
        JsonData arrData = gSystem.getJsonData(jData, sKey);
        string[] sArray = new string[arrData.Count];
        for (int i = 0; i < arrData.Count; i++)
        {
            sArray[i] = arrData[i].ToString();
            //Debug.Log(sArray[i]);
        }
        return sArray;
    }

    //json에서 UTC타임을 받아서 로컬 타임 추출
    public static DateTime getLocalTimeFromJsonUTCTime(JsonData jData, string sKey)
    {
        DateTime reVal = DateTime.Now;
        if (jData.Keys.Contains(sKey) == true)
        {
            if (jData[sKey] != null)
            {
                reVal = TimeZoneInfo.ConvertTimeFromUtc(System.DateTime.Parse(jData[sKey].ToString()), TimeZoneInfo.Local);
            }
            else
            {
                string errorMsg = string.Format("<color=red>This Json Key Value is NULL: {0}</color>", sKey);
                Debug.Log(errorMsg);
            }
        }
        else
        {
            string errorMsg = string.Format("<color=red>Can Not Find DateTime From This Json Key: {0}</color>", sKey);
            Debug.Log(errorMsg);
        }
        return reVal;
    }

    //json에서 UTC타임을 받아서 로컬 스트링 추출
    public static string getLocalTimeStringFromJson(JsonData jData, string sKey)
    {
        DateTime t = DateTime.Now;
        if (jData.Keys.Contains(sKey) == true)
        {
            if (jData[sKey] != null)
            {
                t = TimeZoneInfo.ConvertTimeFromUtc(System.DateTime.Parse(jData[sKey].ToString()), TimeZoneInfo.Local);
            }
            else
            {
                string errorMsg = string.Format("<color=red>This Json Key Value is NULL: {0}</color>", sKey);
                Debug.Log(errorMsg);
            }
        }
        else
        {
            string errorMsg = string.Format("<color=red>Can Not Find DateTime From This Json Key: {0}</color>", sKey);
            Debug.Log(errorMsg);
        }
        return t.ToString("yyyy-MM-dd HH:mm:ss");
    }

    //json에서 시간 추출
    public static DateTime getTimeFromJson(JsonData jData, string sKey)
    {
        DateTime reVal = DateTime.Now;
        if (jData.Keys.Contains(sKey) == true)
        {
            if (jData[sKey] != null)
            {
                reVal = System.DateTime.Parse(jData[sKey].ToString());
            }
            else
            {
                string errorMsg = string.Format("<color=red>This Json Key Value is NULL: {0}</color>", sKey);
                Debug.Log(errorMsg);
            }
        }
        else
        {
            string errorMsg = string.Format("<color=red>Can Not Find DateTime From This Json Key: {0}</color>", sKey);
            Debug.Log(errorMsg);
        }
        return reVal;
    }

    //json에서 y, n으로 불리언 추출
    public static bool getBooleanFromJson(JsonData jData, string sKey)
    {
        bool reVal = false;
        if (jData.Keys.Contains(sKey) == true)
        {
            if (jData[sKey] != null)
            {
                string str = jData[sKey].ToString().ToUpper();
                if (str == "TRUE" || str == "FALSE")
                {
                    reVal = Convert.ToBoolean(jData[sKey].ToString());
                }
                else
                {
                    reVal = getBooleanValue(jData[sKey].ToString());
                }
            }
            else
            {
                string errorMsg = string.Format("<color=red>This Json Key Value is NULL: {0}</color>", sKey);
                Debug.Log(errorMsg);
            }
        }
        else
        {
            string errorMsg = string.Format("<color=red>Can Not Find Boolean From This Json Key: {0}</color>", sKey);
            Debug.Log(errorMsg);
        }
        return reVal;
    }

    //몇일 차인지 체크
    public static int getPeriodDay(string beforeDate)
    {
        //가입기간
        DateTime nDate = DateTime.Now;
        DateTime jDate = DateTime.ParseExact(beforeDate, "yyyy-MM-dd HH:mm:ss", null);
        TimeSpan fuck = nDate - jDate;
        
        return Mathf.CeilToInt((float)fuck.TotalDays);
    }

    //오늘 로컬 시간 문자열로 추출
    public static string getNowLocalTime()
    {
        DateTime t = DateTime.Now;
        string nTime = t.ToString("yyyy-MM-dd HH:mm:ss");
        Debug.Log(nTime);

        return nTime;
    }

    //오늘 로컬 날짜 문자열로 추출
    public static string getNowLocalDay()
    {
        DateTime t = DateTime.Now;
        string nTime = t.ToString("yyyy-MM-dd");
        Debug.Log(nTime);
        //Debug.Log(System.TimeZoneInfo.Local);                 //(UTC+09:00) 서울
        //Debug.Log(System.TimeZoneInfo.Local.Id);              //Korea Standard Time
        //Debug.Log(System.TimeZoneInfo.Local.BaseUtcOffset);   //09:00:00
        return nTime;
    }

    public const int calendarDateCnt = 42;

    //달력을 구성할 수 있는 행렬 제작
    public static DateTime[] getCalendarDate(int iYear, int iMonth)
    {
        //iMonth = 9;
        //달력을 구성할 행렬 초기화
        DateTime[] cDate = new DateTime[calendarDateCnt];  //6주 짜리
        //해당 월의 첫 날을 체크한다.
        DateTime mDay = new DateTime(iYear, iMonth, 1, 0, 0, 0);
        DateTime nowDay;    //날짜 카운트 체크

        //Debug.Log(string.Format("{0} : {1}", DayOfWeek.Monday, Convert.ToInt32(DayOfWeek.Monday)));
        //Debug.Log(string.Format("{0} : {1}", DayOfWeek.Tuesday, Convert.ToInt32(DayOfWeek.Tuesday)));
        //Debug.Log(string.Format("{0} : {1}", DayOfWeek.Wednesday, Convert.ToInt32(DayOfWeek.Wednesday)));
        //Debug.Log(string.Format("{0} : {1}", DayOfWeek.Thursday, Convert.ToInt32(DayOfWeek.Thursday)));
        //Debug.Log(string.Format("{0} : {1}", DayOfWeek.Friday, Convert.ToInt32(DayOfWeek.Friday)));
        //Debug.Log(string.Format("{0} : {1}", DayOfWeek.Saturday, Convert.ToInt32(DayOfWeek.Saturday)));
        //Debug.Log(string.Format("{0} : {1}", DayOfWeek.Sunday, Convert.ToInt32(DayOfWeek.Sunday)));
        //Debug.Log(string.Format("{0}년 {1}월 1일의 요일은 {2} : {3}", iYear, iMonth, mDay.DayOfWeek, Convert.ToInt32(mDay.DayOfWeek)));

        //첫 날의 요일 번호를 리턴
        int dwNum = Convert.ToInt32(mDay.DayOfWeek);
        //1일 등록
        cDate[dwNum] = mDay;
        //이전 달의 날짜 등록        
        nowDay = mDay;
        if (dwNum > 0)
        {
            for (int i = dwNum - 1; i >= 0; i--)
            {
                nowDay = nowDay.AddDays(-1);
                cDate[i] = nowDay;
                //Debug.Log(string.Format("{0}: {1}", i, nowDay));
            }
        }
        //1일 이후의 날짜 등록
        nowDay = mDay;
        for (int i = dwNum + 1; i < calendarDateCnt; i++)
        {
            nowDay = nowDay.AddDays(1);
            cDate[i] = nowDay;
            //Debug.Log(string.Format("{0}: {1}", i, nowDay));
        }
        //달력 구성값 확인
        /*
        for (int i = 0; i < calendarDateCnt; i++)
        {
            Debug.Log(string.Format("{0}: {1}", i, cDate[i]));
        }
        */
        return cDate;
    }

    //-----------------------------------------------------------------------------------------------------------



    //[기준값을 구한다]==========================================================================================

    public static int getReferValue(float mPoint)
    {

        float val = mPoint;
        int cnt = 0;
        int upCnt = 0;
        float reVal = 0.0f;
        float nVal = 0.0f;
        //if (val >= 100000)
        //{
        //    upCnt = 1;
        //}

        while (val >= 1.0f)
        {
            val = val / 10.0f;
            cnt++;
        }

        for (int i = 0; i < cnt; i++)
        {
            if (i == 0)
            {
                reVal = val;
                reVal = reVal * 10.0f;
            }
            else
            {
                reVal = reVal * 10.0f;
            }
            if (i == upCnt)
            {
                reVal = (int)(reVal + 1);
            }
        }
        if (reVal < 10.0f)
        {
            reVal = 10.0f;
        }

        //5단위로 끊는다.
        if (reVal >= 1000.0f)
        {
            nVal = 1.0f;
            for (int i = 0; i < cnt; i++)
            {
                nVal = nVal * 10.0f;
            }
            nVal = (int)nVal * 0.05f;

            if (reVal - nVal > mPoint)
            {
                reVal = reVal - nVal;
            }
        }
        //Debug.Log(string.Format("Input = {0}, OutPut = {1}, mPoint = {2}", (int)mPoint, (int)reVal, (int)nVal));
        return (int)reVal;
    }

    //두 점 사이의 길이
    public static float distanceToPoint(Vector2 a, Vector2 b)
    {
        return (float)Math.Sqrt(Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2));
    }

    //두 점 사이의 각도
    public static float angleToPoint(Vector2 a, Vector2 b)
    {
        Vector2 offset = b - a;
        return (float)Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
    }

    //------------------------------------------------------------------------------



    //[정렬]========================================================================

    public struct sortStr
    {
        public long index;
        public long val;
    }

    //낮은 값 순서로 정렬
    public static sortStr[] sortAsc(int cnt, sortStr[] tList)
    {
        sortStr[] t1 = new sortStr[cnt];
        sortStr tr = new sortStr();
	    long maxVal = 0;
        for (int i = 0; i < cnt; i++)
	    {
            //가장 큰 밸류 획득
            //long val = Convert.ToInt64(Mathf.Ceil(tList[i].val));
            long val = tList[i].val;
            if (val > maxVal)
		    {
			    maxVal = val;
            }
            //Debug.Log(string.Format("i = {0}, tVal = {1}, maxVal = {2}", i, tList[i].val, maxVal));
        }
	    maxVal = maxVal + 1;

        for (int i = 0; i < cnt; i++)
	    {   //초기화
		    t1[i].val = maxVal;
        }
        for (int i = 0; i < cnt; i++)
	    {
		    long val1 = tList[i].val;
            for (int j = 0; j < cnt; j++)
		    {
			    long val2 = t1[j].val;
                if (val1 < val2)
			    {
                    for (int k = cnt - 1; k > j; k--)
				    {
					    tr = t1[k];
					    t1[k] = t1[k - 1];
					    t1[k - 1] = tr;
                    }
				    t1[j] = tList[i];
                    break;
                }
            }
        }
        for (int i = 0; i < cnt; i++)
	    {   //결과
		    tList[i] = t1[i];
        }
        return tList;
    }

    //높은 값 순서로 정렬
    public static sortStr[] sortDesc(int cnt, sortStr[] tList)
    {
        sortStr[] t1 = new sortStr[cnt];
        sortStr tr = new sortStr();
        long minVal = 0;
        for (int i = 0; i < cnt; i++)
        {
            //가장 큰 밸류 획득
            //long val = Mathf.FloorToInt(tList[i].val);
            long val = tList[i].val;
            if (val < minVal)
            {
                minVal = val;
            }
        }
        minVal = minVal - 1;

        for (int i = 0; i < cnt; i++)
        {   //초기화
            t1[i].val = minVal;
        }
        for (int i = 0; i < cnt; i++)
        {
            long val1 = tList[i].val;
            for (int j = 0; j < cnt; j++)
            {
                long val2 = t1[j].val;
                if (val1 > val2)
                {
                    for (int k = cnt - 1; k > j; k--)
                    {
                        tr = t1[k];
                        t1[k] = t1[k - 1];
                        t1[k - 1] = tr;
                    }
                    t1[j] = tList[i];
                    break;
                }
            }
        }
        for (int i = 0; i < cnt; i++)
        {   //결과
            tList[i] = t1[i];
        }
        return tList;
    }

    //문자열 오름차순 정렬
    public static string[] sortDescString(string[] strings)
    {
        Array.Sort(strings);

        return strings;
    }

    //문자열 내림차순 정렬
    public static string[] sortAscString(string[] strings)
    {
        Array.Sort(strings);

        Array.Reverse(strings);
        
        return strings;
    }

    //리스트 랜덤
    public static List<T> shuffleList<T>(List<T> list)
    {
        int random1, random2;
        T temp;

        for (int i = 0; i < list.Count; ++i)
        {
            random1 = UnityEngine.Random.Range(0, list.Count);
            random2 = UnityEngine.Random.Range(0, list.Count);

            temp = list[random1];
            list[random1] = list[random2];
            list[random2] = temp;
        }

        return list;
    }


    //배열 랜덤
    public static T[] shuffleArray<T>(T[] array)
    {
        int random1, random2;
        T temp;

        for (int i = 0; i < array.Length; ++i)
        {
            random1 = UnityEngine.Random.Range(0, array.Length);
            random2 = UnityEngine.Random.Range(0, array.Length);

            temp = array[random1];
            array[random1] = array[random2];
            array[random2] = temp;
        }

        return array;
    }

    //------------------------------------------------------------------------------

}
