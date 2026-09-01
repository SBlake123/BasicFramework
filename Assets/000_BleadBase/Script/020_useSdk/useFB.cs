/// [스크립트 명세]------------------------------------------------------------------------------------------------------------------------
/// 
///  1. 정의 : 페이스북 연동을 위한 라이브러리 클래스
///  2. 기능
///     A. 페이스북 로그인, 로그아웃   [이원진, 2017년 5월 19일]
///     B. 페이스북 친구 초대          [이원진, 2017년 5월 19일]
/// --------------------------------------------------------------------------------------------------------------------------------------
//#define USE_FB

using UnityEngine;
using System.Collections;
using LitJson;
using System;
using System.Collections.Generic;
#if USE_FB
using Facebook.Unity;
#endif

public class useFB
{
    /// [페이스북 연동 외부 셋팅 가이드]---------------------------------------------------------------------------------------------------
    /// 
    ///  1. SDK를 받아 임포트하고 해당 파일을 프로젝트에 삽입합니다.
    ///  2. 구글 플레이 콘솔 또는 아이튠즈 커넥트에서 앱을 등록한 다음 출시합니다.
    ///  3. 페이스북 개발자 페이지에 앱을 생성한 다음, 해당 앱의 대시보드에 키 해시와 패키지 이름, 클래스 이름, 번들ID 등을 등록합니다.
    ///     ※ 아이튠즈는 심사되어 앱스토어에 올라가기 전까지는, 스토어ID 를 넣으면 없다고 합니다. 번들 ID로 연동은 되니까 당황하지 말고 출시 후에 대시보드를 수정해주세요.
    ///  4. 필요한 기능을 구현합니다.
    ///     ※ 해당 테크 킷은 친구 초대 기능을 검증하기 위해 구현하였으므로 기타 기능은 추가적으로 구현해야 합니다. 
    /// ---------------------------------------------------------------------------------------------------------------------------------


    /// [전역 변수 선언]------------------------------------------------------------------------------------------------------------------
    public static List<string> perms = new List<string>();  //페이스북 데이터를 관리하는 리스트입니다. 사실 아직까진 잘 써본 적 없음
    public static bool bInit = false;                       //외부 클래스에서 페이스북 인잇이 되었는지 여부를 파악하는데 사용할 수 있는 전역 변수입니다.
    public static bool bInitEnd = false;                    //페이스북 인잇을 호출했을 때 외부 클래스에서 해당 루틴이 끝났는지를 파악하는데 사용할 수 있는 전역 변수입니다.
    public static bool bLogin = false;                      //외부 클래스에서 페이스북 로그인 상태를 파악하는데 사용할 수 있는 전역 변수입니다.
    public static bool bLoging = false;                     //페이스북 로그인을 호출했을 때 외부 클래스에서 해당 루틴이 끝났는지를 파악하는데 사용할 수 있는 전역 변수입니다.
    public static string userName;                          //페이스북 이름을 확인할 수 있는 문자열 변수입니다.
    public static string userId;                            //페이스북 아이디를 확인할 수 있는 문자열 변수입니다.
    /// ---------------------------------------------------------------------------------------------------------------------------------


    /// <summary>
    /// [FaceBook 인잇]
    /// 
    /// 1. fInit() : 페이스북이 이니셜라이즈되어 있으면 활성화 시키고, 그렇지 않으면 이니셜라이즈 시킵니다. 
    ///              ※ 페이스북은 현재 버튼을 눌러서 로그인-로그아웃 하도록 설계되었고 계정 연동은 되지 않습니다. 따라서 로그인과 분화했습니다.
    /// 2. initCallback() : 페이스북이 이니셜라이즈되면 활성화 시킵니다.
    /// </summary>
    public static void fInit()
    {
#if USE_FB
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(initCallback, onHideUnity);
            bInit = false;
            bLogin = false;
            bInitEnd = false;
            //Debug.Log("FB Initialize");
        }
#else
        bInit = true;
        bLogin = false;
        bInitEnd = true;
#endif
    }
#if USE_FB
    //InIt call back
    private static void initCallback()
    {
        if (FB.IsInitialized)
        {
            //Debug.Log("FB Active");
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            bInit = true;
            //이미 로그인이 되어 있는 상태라면 로그인 상태라는 걸 인지한다
            if (FB.IsLoggedIn)
            {
                userName = "";
                userId = "";
                var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
                userId = aToken.UserId;
                bLogin = true;
            }
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    bInitEnd = true;
    }
    //인잇 처리 중 유니티를 갱신하지 않습니다
    private static void onHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }
    /// ---------------------------------------------------------------------------------------------------------------------------------
#endif

    /// <summary>
    /// [FaceBook 로그인]
    /// 
    /// 1. fLogin() : 페이스북이 이니셜라이즈되어 있고, 로그인 중이거나 로그인 상태가 아니라면 로그인을 요청합니다. 
    /// 2. AuthCallback() : 인증 결과값을 받아 해당 데이터를 처리합니다.
    /// </summary>
    public static void fLogin()
    {
#if USE_FB
        if (bInit == true && bLogin == false && bLoging == false && !FB.IsLoggedIn)
        {
            userName = "";
            userId = "";
            bLoging = true;
            perms = new List<string>() { "public_profile", "email", "user_friends" };
            FB.LogInWithReadPermissions(perms, authCallback);
        }
#endif
    }
#if USE_FB
    //Auth call back
    private static void authCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            //Debug.Log(aToken.UserId);
            userId = aToken.UserId;
            // Print current access token's granted permissions
            foreach (string perm in aToken.Permissions)
            {
                Debug.Log(perm);
            }
            bLogin = true;
        }
        else
        {
            Debug.Log("User cancelled login");
        }
        bLoging = false;
    }
#endif
    /// ---------------------------------------------------------------------------------------------------------------------------------


    /// <summary>
    /// [FaceBook 로그아웃]
    /// 
    /// 1. fLogout() : 로그인 상태라면 로그아웃합니다. 
    /// </summary>
    public static void fLogout()
    {
#if USE_FB
        if (FB.IsLoggedIn && bLogin == true)
        {
            bLogin = false;
            userName = "";
            userId = "";
            FB.LogOut();          
        }
#endif
    }
    /// --------------------------------------------------------------------------------------------------------------------------------


    /// <summary>
    /// [FaceBook 친구초대]
    /// 
    /// 1. fInvite() : 로그인 상태라면 친구초대 페이지로 이동합니다. 현재는 FB.Mobile.AppInvite 함수가 정상적으로 작동하지 않아 FB.AppRequest를 사용중입니다.
    /// 2. inviteCallback() : 현재 사용하지 않습니다.
    /// </summary>
    public static void fInvite()
    {
#if USE_FB
        if (FB.IsLoggedIn)
        {
            FB.AppRequest( message: "Play with me!", title: "Invite Friends");
        }
#endif
    }
    //콜백
    //private static void inviteCallback(IResult result)
    //{
    //}
    /// --------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// [FaceBook 친구초대]
    /// 
    /// 1. fGetUserName()     : 페이스북 이름을 요청합니다.
    /// 2. userNameCallback() : 결과값을 받아 이름을 저장합니다.
    /// </summary>
    //
    ///페이스북 프로파일 사용자 이름을 가져옵니다
    public static void fGetUserName()
    {
#if USE_FB
        if (FB.IsLoggedIn)
        {
            FB.API("/me?fields=id,first_name", Facebook.Unity.HttpMethod.GET, userNameCallback);
        }
#endif
    }
    ///사용자의 프로필 이름을 가져와 userName에 등록합니다.
#if USE_FB
    public static void userNameCallback(IResult result)
    {
        if (result.Error == null)
        {
            JsonData json = JsonMapper.ToObject(result.RawResult);
            //Debug.Log(result.RawResult);
            userName = json["first_name"].ToString();
        }
    }
#endif
    /// --------------------------------------------------------------------------------------------------------------------------------



    /// <summary>
    /// [이하 FaceBook 샘플소스]
    /// 
    /// 1. 필요한 기능이 있다면 해당 소스를 참고하세요
    /// </summary>
    /*
        public GameObject UIFBIsLoggedIn;
        public GameObject UIFBNotLoggedIn;
        public GameObject UIFBAvatar;
        public GameObject UIFBUserName;
        public UILabel ScoresConsole;
        public GameObject ScoreEntryPanel;
        public GameObject ScoreScrollList;


        private Dictionary<string, string> profile = null;
        private List<object> scoreList = null;

        void Awake()
        {
            FB.Init(SetInit, OnHideUnity);
        }


        //초기화
        private void SetInit()
        {
            Debug.Log("FB Init Done.");

            if (FB.IsLoggedIn)
            {
                DealWithFBMenus(true);
                Debug.Log("FB Logged In");
            }
            else
            {
                //버튼 이벤트 연결로 변경
                //FBlogin();
                DealWithFBMenus(false);

            }
        }


        private void OnHideUnity(bool isGameShown)
        {
            if (!isGameShown)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
        }

        //로그인 
        public void FBlogin()
        {
            //FB.Login("user_about_me, user_birthday", AuthCallback);
            FB.Login("email,publish_actions", AuthCallback);
        }

        //로그인 인증 체크
        void AuthCallback(FBResult result)
        {
            if (FB.IsLoggedIn)
            {
                Debug.Log("FB Login worked!");
                DealWithFBMenus(true);
            }
            else
            {
                Debug.Log("FB Login failed!");
                DealWithFBMenus(false);
            }
        }

        //페이스북 프로파일 이미지, 사용자 이름 가져오기
        void DealWithFBMenus(bool isLoggedIn)
        {
            if (isLoggedIn)
            {
                UIFBIsLoggedIn.SetActive(true);
                UIFBNotLoggedIn.SetActive(false);

                // Get Profile Picture Code
                FB.API(Util.GetPictureURL("me", 128, 128), Facebook.HttpMethod.GET, DealWithProfilePicture);

                // Get Username Code
                FB.API("/me?fields=id,first_name", Facebook.HttpMethod.GET, DealWithUserName);
            }
            else
            {
                UIFBIsLoggedIn.SetActive(false);
                UIFBNotLoggedIn.SetActive(true);
            }
        }

        //사용자 프로필 사진 가져오기
        void DealWithProfilePicture(FBResult result)
        {
            if (result.Error != null)
            {
                Debug.Log("problem with getting profile picture");
                FB.API(Util.GetPictureURL("me", 128, 128), Facebook.HttpMethod.GET, DealWithProfilePicture);
                return;
            }

            UITexture UserAvatar = UIFBAvatar.GetComponent<UITexture>();
            UserAvatar.mainTexture = result.Texture;
            UserAvatar.MakePixelPerfect();
        }

        //사용자 프로필 이름 가져오기
        void DealWithUserName(FBResult result)
        {
            if (result.Error != null)
            {
                Debug.Log("problem with getting user name");
                FB.API("/me?field=id,first_name", Facebook.HttpMethod.GET, DealWithUserName);
                return;
            }

            profile = Util.DeserializeJSONProfile(result.Text);
            UILabel UserMsg = UIFBUserName.GetComponent<UILabel>();
            UserMsg.text = "Hello, " + profile["first_name"];
        }

        //공유하기
        public void ShareWithFirends()
        {
            // 순서 변경시 컴파일 에러 발생
            // linkCaption : 링크 설명(링크 본문)
            // picutre : 링크 사진
            // linkName : 링크 제목
            // link : 연결할 링크 주소
            FB.Feed(
                linkCaption: "I'm playing thi awesome game",
                picture: "http://greyzoned.com/images/evilelf2_icon.png",
                linkName: "Check out this game",
                link: "http://apps.facebook.com/" + FB.AppId + "/?challenge_brag=" + (FB.IsLoggedIn ? FB.UserId : "guest")
                );
        }

        //친구초대
        public void InviteFriends()
        {
            // 순서 변경시 컴파일 에러 발생
            // message : 보낼 메시지
            // title : 메시지 보낼 친구목록 창의 타이틀
            FB.AppRequest(
                message: "This gmae is awesome, join me. now!",
                title: "Invite your firends to join you"
            );
        }

        // ALL Scores API related Things
        //점수 불러오기
        public void QueryScores()
        {
            FB.API("/app/scores?fields=score,user.limit(30)", Facebook.HttpMethod.GET, ScoresCallback);
        }

        //점수 Scores Console에 표시하기
        private void ScoresCallback(FBResult result)
        {
            Debug.Log("Scores Callback : " + result.Text);
            //ScoresConsole.text = result.Text;
            ScoresConsole.text = "";

            scoreList = Util.DeserializeScores(result.Text);

            //스크롤뷰 새로고침
            foreach (Transform child in ScoreScrollList.transform.FindChild("Grid").transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            //스크롤뷰 재사용을 위한 그리드 가비지청소 (좌표 초기화)
            UIGrid dataContainer = ScoreScrollList.transform.Find("Grid").GetComponent<UIGrid>();
            dataContainer.transform.DetachChildren();

            #region 불필요코드
            //스크롤뷰 컨테이너들의 좌표 초기화
            //ScoreScrollList.transform.GetComponent<UIScrollView>().ResetPosition();

            //스크롤뷰 패널 새로고침
            //ScoreScrollList.transform.GetComponent<UIPanel>().Refresh();            
            #endregion

            //페이스북 점수와 사용자 이름, 사용자 사진을 불러와 스크롤뷰로 표현
            foreach (object score in scoreList)
            {

                var entry = (Dictionary<string, object>)score;
                var user = (Dictionary<string, object>)entry["user"];
                ScoresConsole.text = ScoresConsole.text + "KR : " + user["name"] + " " + entry["score"] + ",\n";

                //스크롤뷰에 사용자이름, 점수, 사용자사진 붙이기
                GameObject ScorePanel = null;
                ScorePanel = Instantiate(ScoreEntryPanel, new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;

                ScorePanel.transform.parent = ScoreScrollList.transform.FindChild("Grid");


                //NGUI에서 상위 패널 때문에 위치와 크기가 자동으로 변경되므로 초기화 시켜준다.
                ScorePanel.transform.localScale = new Vector3(1f, 1f, 1f);
                ScorePanel.transform.localPosition = new Vector3(0f, 0f, 0f);

                Transform ThisScoreName = ScorePanel.transform.Find("FriendName");
                Transform ThisScoreScore = ScorePanel.transform.Find("FriendScore");
                Transform ThisUserAvatar = ScorePanel.transform.Find("FriendAvatar");

                UILabel ScoreName = ThisScoreName.GetComponent<UILabel>();
                UILabel ScoreScore = ThisScoreScore.GetComponent<UILabel>();
                UITexture UserAvatar = ThisUserAvatar.GetComponent<UITexture>();

                FB.API(Util.GetPictureURL(user["id"].ToString(), 128, 128), Facebook.HttpMethod.GET, delegate (FBResult picutureResult)
                {
                    if (picutureResult.Error != null)
                    {
                        Debug.Log(picutureResult.Error);
                    }
                    else
                    {
                        UserAvatar.mainTexture = picutureResult.Texture;
                        UserAvatar.MakePixelPerfect();
                    }
                });
                ScoreName.text = user["name"].ToString();
                ScoreScore.text = entry["score"].ToString();
            }

            //스크롤뷰의 오브젝트들이 겹치지 않게 자동으로 위치 변경
            ScoreScrollList.transform.FindChild("Grid").GetComponent<UIGrid>().Reposition();


        }

        //점수 설정하기
        public void SetScores()
        {
            var scoreData = new Dictionary<string, string>();
            scoreData["score"] = Random.Range(10, 200).ToString();
            FB.API("/me/scores", Facebook.HttpMethod.POST, delegate (FBResult result)
            {
                Debug.Log("Score submit result :" + result.Text);
            }, scoreData);
        }

        출처: http://genieker.tistory.com/118 [레브네인의 이야기] 
    */
    /// --------------------------------------------------------------------------------------------------------------------------------

}
