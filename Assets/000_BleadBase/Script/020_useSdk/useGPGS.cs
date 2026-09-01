/// [스크립트 명세]------------------------------------------------------------------------------------------------------------------------
/// 
///  1. 정의 : 구글 플레이 / 게임센터 연동을 위한 라이브러리 클래스
///  2. 기능
///     A. 활성화, 로그인, 로그아웃               [이원진, 2017년 5월 19일]
///     B. 데이터 가져오기                        [이원진, 2017년 5월 19일]
///     C. 업적 연동                              [이원진, 2017년 5월 19일]
///     D. 리더보드 연동                          [이원진, 2017년 5월 19일]     
/// --------------------------------------------------------------------------------------------------------------------------------------
#define USE_GPGS

using UnityEngine;
using System.Collections;
using LitJson;
using System;

#if UNITY_ANDROID && USE_GPGS
using GooglePlayGames;
using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames.BasicApi;
#endif

public class useGPGS
{
    /// [구글 연동 외부 셋팅 가이드]------------------------------------------------------------------------------------------------------
    /// 
    ///  1. SDK를 받아 임포트하고 해당 파일을 프로젝트에 삽입합니다.
    ///  2. 구글 플레이 콘솔에서 앱을 등록한 다음 출시합니다.
    ///  3. 구글 플레이 콘솔에 들어가 게임 서비스에서 새 게임을 추가하거나, 이미 있는 게임에 앱을 연동합니다.
    ///  4. 업적이 5개 미만이거나 사용할 업적이 없다면 구글 플레이 콘솔에서 업적을 추가하십시오. 리더보드는 필수가 아닙니다.
    ///  5. 구글플레이 콘솔에서 업적 메뉴나 리더보드 메뉴에서 리소스 받기 버튼을 누른 다음, 해당 값을 복사하세요.
    ///  6. 유니티 메뉴 중 Window메뉴로 들어가서 구글플레이 게임의 셋업 항목을 찾아 누릅니다.
    ///  7. 리소스를 입력하고 패키지 네임을 확인합니다. 만약 지금 패키지 네임과 다르다면 리소스를 수정하세요.
    ///  8. 다시 구글 플레이 콘솔로 돌아가 연결된 앱에서 해당 앱을 찾아 OAuth2 클라이언트 ID 값을 복사한 다음 셋업으로 돌아가서 입력합니다.
    ///  9. 셋팅 확인하고 (구글플러스 버튼 해제) 셋업을 누릅니다. 좋은 팝업들이 뜬다면 셋팅이 완료된 것입니다.
    /// 10. GPGS 셋팅이 저장되어 생성된 파일과 클래스를 확인합니다
    /// 
    /// ---------------------------------------------------------------------------------------------------------------------------------

#if USE_GPGS
    /// [전역 변수 선언]------------------------------------------------------------------------------------------------------------------ 
    private static bool bLogin = false;  //외부 클래스에서 GPGS 로그인이 되어 있는지를 파악하는데 사용할 수 있는 전역 변수입니다.
    private static bool bEnd = false;    //외부 클래스에서 GPGS 연동 작업이 끝난 상태인지를 파악하는데 사용할 수 있는 전역 변수입니다.
    /// ---------------------------------------------------------------------------------------------------------------------------------
#endif
    /// <summary>
    /// [GPGS 활성화]---------------------------------------------------------------------------------------------------------------------
    /// 
    /// 1. GPGS를 활성화합니다. 자동적으로 초기 셋팅 및 로그인이 같이 이루어지므로 외부 클래스는 loginGPGS() 대신 이 함수를 쓰면 됩니다.
    /// ※ 모든 함수에서 iOS는 동작하지 않습니다.
    /// </summary>
    public static void initGPGS()
    {
#if USE_GPGS
#if UNITY_ANDROID
        bLogin = false;
        bEnd = false;
        PlayGamesPlatform.Activate();   //GPGS 활성화
        loginGPGS();
#elif UNITY_IOS
        bLogin = false;
        bEnd = false;
        UnityEngine.SocialPlatforms.GameCenter.GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
        loginGPGS();
#endif
#endif
    }
    /// ---------------------------------------------------------------------------------------------------------------------------------


    /// <summary>
    /// [GPGS 로그인]-------------------------------------------------------------------------------------------------------------------- 
    /// 
    /// 1. GPGS가 로그인 되어 있지 않다면, GPGS를 로그인 합니다.
    /// 2. 로그인이 완료된 후에는 loginCallBack로 콜백이 들어옵니다. 
    /// </summary>
    public static void loginGPGS()
    {
#if USE_GPGS
        if (!Social.localUser.authenticated)
            Social.localUser.Authenticate(loginCallBack);
        else
        {
            bLogin = true;
            bEnd = true;
        }
#endif
    }
    // GPGS Login Callback
    public static void loginCallBack(bool result)
    {
#if USE_GPGS
        bLogin = result;
        bEnd = true;
#endif
    }

    public static bool checkLogin()
    {
#if USE_GPGS
        if (!Social.localUser.authenticated)
        {
            return false;
        }
        else
        {
            return bLogin;
        }
#else
        return false;
#endif
    }

    /// ---------------------------------------------------------------------------------------------------------------------------------


    /// <summary>
    /// [GPGS 로그아웃]-------------------------------------------------------------------------------------------------------------------
    /// 
    /// 1. GPGS가 로그인 되어 있다면 GPGS를 로그아웃 합니다.
    /// </summary>
    public static void logoutGPGS()
    {
#if USE_GPGS
        if (Social.localUser.authenticated)
        {
#if UNITY_ANDROID
            //((GooglePlayGames.PlayGamesPlatform)Social.Active).SignOut();
#elif UNITY_IOS
            //((PlayGamesPlatform)Social.Active).SignOut();
#endif
            //bLogin = false;
        }
#endif

    }
    /// ---------------------------------------------------------------------------------------------------------------------------------


    /// <summary>
    /// [GPGS 데이터 가져오기]------------------------------------------------------------------------------------------------------------
    ///
    /// 1. getImageGPGS() : GPGS가 로그인 되어 있다면 GPGS에서 자신의 프로필 이미지를 가져옵니다. 
    /// 2. getNameGPGS()  : GPGS가 로그인 되어 있다면 GPGS 에서 사용자 이름을 가져옵니다.
    /// 3. getPlatformIDGPGS() : GPGS가 로그인 되어 있다면 GPGS 에서 플랫폼 ID를 가져옵니다. ※ 핵심 기능입니다.
    /// </summary>
    /// <returns> Texture2D 이미지 </returns>
    public static Texture2D getImageGPGS()
    {
#if USE_GPGS
        if (Social.localUser.authenticated)
            return Social.localUser.image;
        else
            return null;
#else
        return null;
#endif
    }
    /// <returns> 이름 </returns>
    public static string getNameGPGS()
    {
#if USE_GPGS
        if (Social.localUser.authenticated)
            return Social.localUser.userName;
        else
            return null;
#else
        return null;
#endif

    }
    /// <returns> 플랫폼 ID </returns>
    public static string getPlatformIDGPGS()
    {
#if USE_GPGS
        if (Social.localUser.authenticated)
            return Social.localUser.id;
        else
            return null;
#else
        return null;
#endif
    }
    /// ---------------------------------------------------------------------------------------------------------------------------------


    /// <summary>
    /// [GPGS 업적 연동]------------------------------------------------------------------------------------------------------------------
    ///
    /// 1. clearAchievement(int val) : 업적 달성 처리를 하는 함수입니다.
    /// 2. callBackAchievement(bool result) : 업적 처리한 후 콜백을 받아 처리해야 할 것이 있다면 여기에 등록합니다.
    /// 3. showAchievement() : 업적을 보여줍니다.
    /// </summary>
    public static void clearAchievement(int val)
    {
#if USE_GPGS
#if UNITY_ANDROID
        if (Social.localUser.authenticated)
        {
            //사용 예: Social.ReportProgress([생성한 GPGS 파일].[등록된 업적], (float)[구현 퍼센트(100.0f면 업적 달성)], [콜백함수]);  
            //(이하 프로젝트에 맞춰 구현할 것)====================================================
            /*
            if (val == 0)
                Social.ReportProgress(GPGSIds.achievement, 100.0f, callBackAchievement);
            else if (val == 1)
                Social.ReportProgress(GPGSIds.achievement_2, 100.0f, callBackAchievement);
            else if (val == 2)
                Social.ReportProgress(GPGSIds.achievement_3, 100.0f, callBackAchievement);
            else if (val == 3)
                Social.ReportProgress(GPGSIds.achievement_4, 100.0f, callBackAchievement);
            else if (val == 4)
                Social.ReportProgress(GPGSIds.achievement_5, 100.0f, callBackAchievement);
            else if (val == 5)
                Social.ReportProgress(GPGSIds.achievement_6, 100.0f, callBackAchievement);
                */
            //================================================================================== 
        }
#endif
#endif
    }
    //Achievement call back
    public static void callBackAchievement(bool result)
    {

    }
    /// <returns> true or false </returns>
    public static bool showAchievement()
    {
#if USE_GPGS
#if UNITY_ANDROID
        if (Social.localUser.authenticated)
        {
            Social.Active.ShowAchievementsUI();
            return true;
        }
        else
            return false;
#else
        return false;
#endif
#else
        return false;
#endif
    }
    /// ---------------------------------------------------------------------------------------------------------------------------------



    /// <summary>
    /// [GPGS 리더보드 연동]--------------------------------------------------------------------------------------------------------------
    /// 
    /// ※ 리더보드를 등록하지 않은 경우, 해당 리더보드 정보가 리소스에 없습니다. 따라서 그런 상황에서는 이 모듈을 볼 필요도, 함수를 적용할 수도 없습니다.
    /// 1. setLeaderboard(long score) : 스코어를 받아 리더보드에 기록하는 함수입니다.
    /// 2. callBackLeaderboard(bool result) : 리더보드 처리한 후 콜백을 받아 처리해야 할 것이 있다면 여기에 등록합니다.
    /// 3. showLeaderboard() : 리더보드를 보여줍니다.
    /// </summary>
    public static void setLeaderboard(long score)
    {
#if USE_GPGS
#if UNITY_ANDROID
        //사용 예: Social.Active.ReportScore((long)[점수], [생성한 GPGS 파일].[등록된 리더보드], [콜백함수]);
        //(이하 프로젝트에 맞춰 구현할 것)============================================================
        //if (Social.localUser.authenticated)
        //   Social.Active.ReportScore(score, GPGSIds.leaderboard_ranking, callBackLeaderboard);
        //==========================================================================================
#endif
#endif
    }
    //Leaderboard call back
    public static void callBackLeaderboard(bool result)
    {

    }
    /// <returns> true or false </returns>
    public static bool showLeaderboard()
    {
#if USE_GPGS
#if UNITY_ANDROID
        if (Social.localUser.authenticated)
        {
            Social.Active.ShowLeaderboardUI();
            return true;
        }
        else
            return false;
#else
        return false;
#endif
#else
        return false;
#endif
    }
    /// ---------------------------------------------------------------------------------------------------------------------------------
}
