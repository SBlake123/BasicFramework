/// [스크립트 명세]------------------------------------------------------------------------------------------------------------------------
/// ※ 정의 : 게임 내부 테스트용 관련한 공용 임시 함수
/// --------------------------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using LitJson;
using System;
using UnityEngine.UI;

public class gTest
{
    public static void testFunc()
    {
        DateTime t = DateTime.Now;
        string nTime = t.ToString("yyyy-MM-dd HH:mm:ss");
        Debug.Log(nTime);


        //[시간 관련 테스트]=====================================================================
        //DateTime t = DateTime.Now;
        //string nTime = t.ToString("yyyy-MM-dd HH:mm:ss");
        //Debug.Log(nTime);
        //DateTime parsedDate = TimeZoneInfo.ConvertTimeToUtc(System.DateTime.Parse(time));
        //Debug.Log(System.TimeZoneInfo.Local);
        //Debug.Log(System.TimeZoneInfo.Local.Id);
        //Debug.Log(System.TimeZoneInfo.Local.BaseUtcOffset);
        //Debug.Log(gBase.getCountry());
        //---------------------------------------------------------------------------------------
        //Debug.Log(string.Format("+{0:D2}:{1:D2}", System.TimeZoneInfo.Local.BaseUtcOffset.Hours, System.TimeZoneInfo.Local.BaseUtcOffset.Minutes));
        //로컬 타임계는 해당 방법으로도 변경 가능
        //DateTime sTime = DateTime.Parse(noticeList[i].startTime).ToLocalTime();
        //DateTime eTime = DateTime.Parse(noticeList[i].endTime).ToLocalTime();

        //string nTime = t.ToString("HH:mm:ss");
        //Debug.Log(nTime);
        //로컬 타임을 UTC로
        //DateTime parsedDate = TimeZoneInfo.ConvertTimeToUtc(System.DateTime.Parse(nTime));
        //Debug.Log(parsedDate.ToString("yyyy-MM-dd HH:mm:ss"));

        //[전처리기 관련 테스트]=================================================================
#if BUILD_VER_BETA
        Debug.Log("This is Beta");
#elif BUILD_VER_GAMMA1
        Debug.Log("This is Gamma1");
#elif BUILD_VER_GAMMA2
        Debug.Log("This is Gamma2");
#else
        Debug.Log("This is Spartaaaaaaa!");
#endif
        //---------------------------------------------------------------------------------------

    }

}
