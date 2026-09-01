/// [스크립트 명세]-----------------------------------------------------------------------------------------------------------------------
/// 로컬 푸시를 처리하는 매니저
/// --------------------------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System;
using Assets.SimpleAndroidNotifications;
using LitJson;
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

//에셋 스토어에서 simple notification을 다운로드 받아서 해당 구현을 진행
//iOS도 하려면 Mobile Notification 패키지도 다운로드 받아 설치해야 한다. 
public class gLocalSimpleNotiManager : MonoBehaviour
{
    private static string[] title;
    private static string[] content;

    private const int pushCnt = 14;

    void Awake()
    {
        var obj = FindObjectsOfType<gLocalSimpleNotiManager>();
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

    private void initManager()
    {
        DeleteNotification();
        AddLocalNotification();
    }

    //로컬 푸시를 셋팅한다.
    public static void setLocalNotification(bool bAble)
    {
        if (bAble == true)
        {
            DeleteNotification();
            AddLocalNotification();
        }
        else
        {
            DeleteNotification();
        }
    }

    private static void DeleteNotification() //알람 초기화
    {
#if UNITY_ANDROID
        NotificationManager.CancelAll();
#elif UNITY_IOS
        iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
    }

    private static void AddLocalNotification() //알람 추가
    {
        DateTime[] notify = new DateTime[pushCnt];
        TimeSpan[] pTime = new TimeSpan[pushCnt];
        NotificationIcon[] icon = new NotificationIcon[pushCnt];

        title = new string[pushCnt];
        content = new string[pushCnt];
        for (int i = 0; i < title.Length; i++)
        {
            //Debug.Log(string.Format("Push {0}: {1} / {2}", i, gText.getBaseText(1000 + i), gText.getBaseText(2000 + i)));
            title[i] = gText.getBaseText(1000 + i);
            content[i] = gText.getBaseText(2000 + i);
        }

        //1일 후 12시 30분 알림
        notify[0] = Convert.ToDateTime(DateTime.Today.AddDays(1).ToString("yyyy/MM/dd") + " " + "0:30:00 PM");
        pTime[0] = notify[0] - DateTime.Now;
        icon[0] = NotificationIcon.Clock;

        //1일 후 20시 알림
        notify[1] = Convert.ToDateTime(DateTime.Today.AddDays(1).ToString("yyyy/MM/dd") + " " + "8:00:00 PM");
        pTime[1] = notify[1] - DateTime.Now;
        icon[1] = NotificationIcon.Clock;

        //2일 후 12시 30분 알림
        notify[2] = Convert.ToDateTime(DateTime.Today.AddDays(2).ToString("yyyy/MM/dd") + " " + "0:30:00 PM");
        pTime[2] = notify[2] - DateTime.Now;
        icon[2] = NotificationIcon.Clock;

        //2일 후 20시 알림
        notify[3] = Convert.ToDateTime(DateTime.Today.AddDays(2).ToString("yyyy/MM/dd") + " " + "8:00:00 PM");
        pTime[3] = notify[3] - DateTime.Now;
        icon[3] = NotificationIcon.Clock;

        //3일 후 12시 30분 알림
        notify[4] = Convert.ToDateTime(DateTime.Today.AddDays(3).ToString("yyyy/MM/dd") + " " + "0:30:00 PM");
        pTime[4] = notify[4] - DateTime.Now;
        icon[4] = NotificationIcon.Clock;

        //3일 후 20시 알림
        notify[5] = Convert.ToDateTime(DateTime.Today.AddDays(3).ToString("yyyy/MM/dd") + " " + "8:00:00 PM");
        pTime[5] = notify[5] - DateTime.Now;
        icon[5] = NotificationIcon.Clock;

        //5일 후 12시 30분 알림
        notify[6] = Convert.ToDateTime(DateTime.Today.AddDays(5).ToString("yyyy/MM/dd") + " " + "0:30:00 PM");
        pTime[6] = notify[6] - DateTime.Now;
        icon[6] = NotificationIcon.Bell;

        //7일 후 12시 30분 알림
        notify[7] = Convert.ToDateTime(DateTime.Today.AddDays(7).ToString("yyyy/MM/dd") + " " + "0:30:00 PM");
        pTime[7] = notify[7] - DateTime.Now;
        icon[7] = NotificationIcon.Bell;

        //10일 후 12시 30분 알림
        notify[8] = Convert.ToDateTime(DateTime.Today.AddDays(10).ToString("yyyy/MM/dd") + " " + "0:30:00 PM");
        pTime[8] = notify[8] - DateTime.Now;
        icon[8] = NotificationIcon.Bell;

        //15일 후 12시 30분 알림
        notify[9] = Convert.ToDateTime(DateTime.Today.AddDays(15).ToString("yyyy/MM/dd") + " " + "0:30:00 PM");
        pTime[9] = notify[9] - DateTime.Now;
        icon[9] = NotificationIcon.Bell;

        //20일 후 12시 30분 알림
        notify[10] = Convert.ToDateTime(DateTime.Today.AddDays(20).ToString("yyyy/MM/dd") + " " + "0:30:00 PM");
        pTime[10] = notify[10] - DateTime.Now;
        icon[10] = NotificationIcon.Bell;

        //25일 후 12시 30분 알림
        notify[11] = Convert.ToDateTime(DateTime.Today.AddDays(25).ToString("yyyy/MM/dd") + " " + "0:30:00 PM");
        pTime[11] = notify[11] - DateTime.Now;
        icon[11] = NotificationIcon.Bell;

        //30일 후 12시 30분 알림
        notify[12] = Convert.ToDateTime(DateTime.Today.AddDays(30).ToString("yyyy/MM/dd") + " " + "0:30:00 PM");
        pTime[12] = notify[12] - DateTime.Now;
        icon[12] = NotificationIcon.Bell;

        //2일 후 밤 10시 알림
        notify[13] = Convert.ToDateTime(DateTime.Today.AddDays(2).ToString("yyyy/MM/dd") + " " + "10:00:00 PM");
        pTime[13] = notify[13] - DateTime.Now;
        icon[13] = NotificationIcon.Clock;

        //어플 종료 후 3분 후에 알림
        //notify[14] = DateTime.Now.AddMinutes(3);
        //pTime[14] = notify[14] - DateTime.Now;
        //icon[14] = NotificationIcon.Bell;

        /*
        content = new string[5];
        content[0] = "Daily Login Test";
        content[1] = "1 Minute!";
        content[2] = "점심 식사 시간입니다!";
        content[3] = "저녁 퇴근 시간입니다 :D";
        content[4] = "새로운 리그가 시작되었습니다!";

        //오늘
        DateTime dtToday = DateTime.Today;
        DateTime dtNextday = DateTime.Today.AddDays(1);
        DateTime[] notify = new DateTime[5];
        TimeSpan[] pTime = new TimeSpan[5];
        NotificationIcon[] icon = new NotificationIcon[5];

        //하루 후 알림
        notify[0] = DateTime.Now.AddDays(1);
        pTime[0] = notify[0] - DateTime.Now;

        //어플 종료 후 1분 후에 알림
        notify[1] = DateTime.Now.AddMinutes(1);
        pTime[1] = notify[1] - DateTime.Now;

        //어플 종료 후 알림(점심)
        notify[2] = Convert.ToDateTime(dtToday.ToString("yyyy/MM/dd") + " " + "1:00:00 PM");
        pTime[2] = notify[2] - DateTime.Now;
        if(pTime[2].TotalSeconds <= 0)
        {
            notify[2] = Convert.ToDateTime(dtNextday.ToString("yyyy/MM/dd") + " " + "1:00:00 PM");
            pTime[2] = notify[2] - DateTime.Now;
        }

        //어플 종료 후 알림(저녁)
        notify[3] = Convert.ToDateTime(dtToday.ToString("yyyy/MM/dd") + " " + "7:00:00 PM");
        pTime[3] = notify[3] - DateTime.Now;
        if (pTime[3].TotalSeconds <= 0)
        {
            notify[3] = Convert.ToDateTime(dtNextday.ToString("yyyy/MM/dd") + " " + "7:00:00 PM");
            pTime[3] = notify[3] - DateTime.Now;
        }

        //어플 종료 후 특정 요일에 등록 알림
        int nDay = Convert.ToInt32(DayOfWeek.Monday) - Convert.ToInt32(dtToday.DayOfWeek);
        DateTime dtNow;
        if (nDay > 0)
        {
            dtNow = dtToday.AddDays(Convert.ToInt32(DayOfWeek.Monday) - Convert.ToInt32(dtToday.DayOfWeek));
        }
        else
        {       
            dtNow = dtToday.AddDays(7 - Convert.ToInt32(dtToday.DayOfWeek) + Convert.ToInt32(DayOfWeek.Monday));
        }
        notify[4] = Convert.ToDateTime(dtNow.ToString("yyyy/MM/dd") + " " + "9:00:00 AM");
        pTime[4] = notify[4] - DateTime.Now;

        icon[0] = NotificationIcon.Clock;
        icon[1] = NotificationIcon.Bell;
        icon[2] = NotificationIcon.Event;
        icon[3] = NotificationIcon.Event;
        icon[4] = NotificationIcon.Star;
        */

#if UNITY_ANDROID
        for (int i = 0; i < notify.Length; i++)
        {
            NotificationManager.SendWithAppIcon(pTime[i], title[i], content[i], Color.gray, icon[i]);
        }
        Debug.Log("Set Android Notification");
#elif UNITY_IOS
        iOSNotificationTimeIntervalTrigger tTrigger;
        for (int i = 0; i < notify.Length; i++)
        {        
            tTrigger = new iOSNotificationTimeIntervalTrigger()
            {
                TimeInterval = notify[i] - DateTime.Now,
                Repeats = false
            };
            iOSNotification nNoti = new iOSNotification()
            {
                Identifier = string.Format("_noti_{0}", i),
                Title = title[i],
                Body = content[i],
                ShowInForeground = false,
                ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
                CategoryIdentifier = "category_f",
                ThreadIdentifier = "thread_f",
                Trigger = tTrigger,
            };
            //Debug.Log(string.Format("IOS Push: {0}", nNoti.Identifier));
            iOSNotificationCenter.ScheduleNotification(nNoti);
        }
        Debug.Log("Set IOS Notification");
#endif
    }
}
