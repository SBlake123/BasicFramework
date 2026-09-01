/// [스크립트 명세]-----------------------------------------------------------------------------------------------------------------------
///  팝업 UI를 처리하는 매니저
///  로고 씬에서 GameObject 컴포넌트에 등록
/// --------------------------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LitJson;
using System;
#if USE_SPINE
using Spine;
using Spine.Unity;
#endif

public class gRewardManager : MonoBehaviour
{
    public GameObject panelReward;

    const float strechTime = 0.3f;      //열리고 닫히는 시간
    const float showTimeBase = 2.0f;    //기본값
    const float itemShowTime = 0.4f;    //아이템 보여주는 텀

    float showTime = 2.0f;      //보여주는 시간
    float rTimer = 0.0f;        //타이머
    float startScale = 0.5f;    //시작 스케일
    float endScale = 1.0f;      //종료 스케일
    int nowCnt = 0;             //보여준 수
    int endCnt = 0;             //애니까지 완료한 수
    float iTimer = 0.0f;        //아이템 보여주는 타이머
    float iWidth = 0.0f;        //아이템 사이즈
    int posVal = 0;             //아이템 시작점과 이동점을 체크하는 기준 값
    private static string header = "";

    bool bSkip = false;    //스킵

    public struct rewardInfo
    {
        public string code;
        public string category;
        public string iconImg;
        public int count;
    }
    private static rewardInfo[] rewardData = new rewardInfo[0];

    private static int nowState = (int)POPUP_STATE.READY;    //현재 상태

    // [팝업 상태 확인]
    public enum POPUP_STATE
    {
        READY = 0,  //준비 
        REQUEST,    //요청
        OPENING,    //열기
        SHOWING,    //선택
        CLOSING,    //닫기
        END
    }


    // Awake is called before the first frame update
    void Awake()
    {
        var obj = FindObjectsOfType<gRewardManager>();
        if (obj.Length <= 1)
        {
            gBase.setEnKey();
            //DontDestroyOnLoad(gameObject);
            initManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //현재 스테이트 상태에 따라 업데이트 처리한다.
        switch (nowState)
        {
            case (int)POPUP_STATE.REQUEST:
                setReward();
                break;
            case (int)POPUP_STATE.OPENING:
            case (int)POPUP_STATE.CLOSING:
                updateScaleAction();
                break;
            case (int)POPUP_STATE.SHOWING:
                updateScaleShowing();
                break;
            case (int)POPUP_STATE.END:
                initManager();
                break;
        }
    }

    //매니저 초기화
    void initManager()
    {
        rewardData = new rewardInfo[0];
        if (panelReward.activeSelf == true)
        {
            panelReward.SetActive(false);
        }
        bSkip = false;
        nowCnt = 0;        //보여준 수
        endCnt = 0;        //애니까지 완료한 수
        header = "";
        nowState = (int)POPUP_STATE.READY;
    }

    //보상 팝업을 요청한다.
    public static void requestReward(rewardInfo[] rInfo, string headerStr)
    {
        if (nowState == (int)POPUP_STATE.READY && rInfo.Length > 0)
        {
            //데이터를 셋팅한다.
            rewardData = new rewardInfo[rInfo.Length];
            rewardData = rInfo;
            header = headerStr;
            nowState = (int)POPUP_STATE.REQUEST;        //요청 상태로 변경
        }
    }

    //보여주는 상태로 변경한다.
    void setReward()
    {
        if (nowState == (int)POPUP_STATE.REQUEST)
        {
            //UI를 셋팅한다.
            gText.setText(panelReward.transform.Find("Body").transform.Find("Text").gameObject, header);
            panelReward.transform.Find("Body").transform.Find("Text").gameObject.SetActive(false);
            setRewardItem();
            rTimer = strechTime;
            startScale = 0.5f;      //시작 스케일
            endScale = 1.0f;        //종료 스케일
            //bSkip = false;
            nowState = (int)POPUP_STATE.OPENING;        //오프닝 상태로 변경
            panelReward.SetActive(true);
        }
    }

    //타이머에 따라 스케일을 변경한다.
    void updateScaleAction()
    {
        GameObject view = panelReward.transform.Find("Body").transform.Find("View").gameObject;
        RectTransform bRect = panelReward.transform.Find("Body").gameObject.GetComponent<RectTransform>();
        if (rTimer > 0.0f)
        {
            rTimer -= Time.smoothDeltaTime;
        }
        if (rTimer <= 0.0f)
        {
            rTimer = 0.0f;
            bRect.localScale = new Vector3(1.0f, endScale, 1.0f);
            if (nowState == (int)POPUP_STATE.OPENING)
            {
                rTimer = showTime;
                nowState = (int)POPUP_STATE.SHOWING;
                view.SetActive(true);
                nowCnt = 0;        //보여준 수
                endCnt = 0;        //애니까지 완료한 수
                iTimer = 0.0f;
            }
            else
            {
                nowState = (int)POPUP_STATE.END;
            }
        }
        else
        {
            float nScale = startScale + ((endScale - startScale) / strechTime * (strechTime - rTimer));
            bRect.localScale = new Vector3(1.0f, nScale, 1.0f);
        }
    }

    void updateScaleShowing()
    {
        GameObject view = panelReward.transform.Find("Body").transform.Find("View").gameObject;
        RectTransform viewTransform = view.GetComponent<RectTransform>();
        GameObject sItem = view.transform.Find("Item").gameObject;
        RectTransform rowRectTransform = sItem.GetComponent<RectTransform>();

        if (rTimer > 0.0f)
        {
            rTimer -= Time.smoothDeltaTime;

            //아이템을 처리한다.
            if (nowCnt < rewardData.Length)
            {
                iTimer -= Time.smoothDeltaTime;
                if (iTimer <= 0.0f)
                {
                    if (view.transform.Find("ScrollPanel").transform.Find(string.Format("Reward_{0}", nowCnt)) != null)
                    {
                        GameObject nowItem = view.transform.Find("ScrollPanel").transform.Find(string.Format("Reward_{0}", nowCnt)).gameObject;
#if USE_SPINE
                        if (nowItem.transform.Find("Ani").GetComponent<SkeletonAnimation>() != null)
                        {
                            //Debug.Log(string.Format("Reward_{0}", nowCnt));
                            nowItem.transform.Find("Ani").gameObject.SetActive(true);
                            SkeletonAnimation cRes = nowItem.transform.Find("Ani").GetComponent<SkeletonAnimation>();
                            gGraphic.setSpineAnimationAbsolute(cRes, "Open", false);
                        }
                        else
#endif
                        {
                            nowItem.transform.Find("Back").gameObject.SetActive(true);
                            nowItem.transform.Find("Img").gameObject.SetActive(true);
                            nowItem.transform.Find("Cnt").gameObject.SetActive(true);
                        }
                    }
                    if (rewardData.Length > posVal)
                    {
                        if (nowCnt >= Mathf.FloorToInt(posVal / 1.0f))
                        {
                            viewTransform.anchoredPosition = new Vector2(viewTransform.anchoredPosition.x - (iWidth * 2.0f), 0.0f);
                        }
                    }
                    else
                    {
                        if (nowCnt > 0)
                        {
                            viewTransform.anchoredPosition = new Vector2(viewTransform.anchoredPosition.x - iWidth, 0.0f);
                        }
                    }
                    nowCnt++;
                    iTimer = itemShowTime;
                }
            }
            //보여주기 처리
#if USE_SPINE
            foreach (Transform child in view.transform.Find("ScrollPanel").transform)
            {
                //Debug.Log(child.name);
                if (child.transform.Find("Back").gameObject.activeInHierarchy == false && child.transform.Find("Ani").gameObject.activeInHierarchy == true)
                {
                    SkeletonAnimation cRes = child.transform.Find("Ani").GetComponent<SkeletonAnimation>();
                    if (cRes.state.GetCurrent(0).IsComplete == true)
                    {
                        //Debug.Log("Animation End");
                        child.transform.Find("Back").gameObject.SetActive(true);
                        child.transform.Find("Img").gameObject.SetActive(true);
                        child.transform.Find("Cnt").gameObject.SetActive(true);
                        endCnt++;
                    }
                }
            }
#endif
            if (panelReward.transform.Find("Body").transform.Find("Text").gameObject.activeInHierarchy == false)
            {
                panelReward.transform.Find("Body").transform.Find("Text").gameObject.SetActive(true);
            }
        }
        if (rTimer <= 0.0f || (endCnt >= nowCnt && bSkip == true))
        {
            rTimer = strechTime;
            startScale = 1.0f;      //시작 스케일
            endScale = 0.5f;        //종료 스케일
            view.SetActive(false);
            nowState = (int)POPUP_STATE.CLOSING;        //클로징 상태로 변경
            panelReward.transform.Find("Body").transform.Find("Text").gameObject.SetActive(false);
        }
    }

    //리워드 아이템을 등록한다
    void setRewardItem()
    {
        GameObject view = panelReward.transform.Find("Body").transform.Find("View").gameObject;
        GameObject sPanel = view.transform.Find("ScrollPanel").gameObject;
        GameObject sBar = view.transform.Find("Scrollbar").gameObject;
        GameObject sItem = view.transform.Find("Item").gameObject;

        RectTransform rowRectTransform = sItem.GetComponent<RectTransform>();
        RectTransform conRectTransform = sPanel.GetComponent<RectTransform>();
        RectTransform viewTransform = view.GetComponent<RectTransform>();

        int itemCnt = 0;
        int rowCnt = 1;
        float posY = 0.0f;
        gUi.dynamicListVal lVal = new gUi.dynamicListVal();

        //기존의 아이템을 정리한다.
        for (int k = sPanel.transform.childCount - 1; k >= 0; k--)
        {
            try
            {
                if (sPanel.transform.GetChild(k).transform.Find("Img").transform.Find("Custom").transform.childCount > 0)
                {
                    try
                    {
                        //이미지를 처리한다
                        gObjectManager.returnObjPool(sPanel.transform.GetChild(k).transform.Find("Img").transform.Find("Custom").transform.GetChild(0).gameObject);
                    }
                    catch
                    {

                    }
                }
                //해당 오브젝트들을 재활용 통에 넣는다.
                gObjectManager.setObjRecyclingBin(sPanel.transform.GetChild(k).gameObject);
            }
            catch
            {
            }
        }
        /*
        //아이템 개수를 체크한다.
        itemCnt = rewardData.Length;
        //리스트에 사용할 밸류값을 셋팅한다.
        lVal = gUi.setDynamicHorizonListValue(itemCnt, rowCnt, rowRectTransform, conRectTransform);
        //스크롤 셋팅한다.
        gUi.setDynamicHorizonListScroll(lVal.itemCnt, lVal.scrollWidth, posY, 0.0f, viewTransform, conRectTransform, sBar, false);

        if (lVal.itemCnt > 0)
        {
            int j = 0;
            int iCnt = 0;
            for (int i = 0; i < rewardData.Length; i++)
            {
                if (iCnt % lVal.rowCnt == 0)
                    j++;

                string itemName = string.Format("Reward_{0}", i);
                //재활용 통에서 가져오거나 생성한다.
                GameObject nItem = gObjectManager.getObjToRecyclingBin(itemName);
                if (nItem == null)
                {
                    nItem = Instantiate(sItem) as GameObject;
                }
                setItemData(i, nItem);
                gUi.setHorizonListItemPos(itemName, nItem, sPanel, conRectTransform, lVal, j, iCnt);
                nItem.transform.Find("Back").gameObject.SetActive(false);
                nItem.transform.Find("Img").gameObject.SetActive(false);
                nItem.transform.Find("Cnt").gameObject.SetActive(false);
                nItem.transform.Find("Ani").gameObject.SetActive(false);
                nItem.SetActive(true);
                iCnt++;
            }
            //posVal = Mathf.CeilToInt(viewTransform.rect.width / rowRectTransform.rect.width);   //올림
            posVal = Mathf.FloorToInt(viewTransform.rect.width / rowRectTransform.rect.width);   //내림
            if (rewardData.Length > posVal)
            {
                viewTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
            }
            else
            {
                viewTransform.anchoredPosition = new Vector2((viewTransform.rect.width / 2.0f) - (rowRectTransform.rect.width / 2.0f), 0.0f);
            }
            iWidth = rowRectTransform.rect.width / 2.0f;
        }
        */
        view.SetActive(false);
        //쇼타임 셋팅
        showTime = showTimeBase + (itemCnt * itemShowTime);

    }

    void setItemData(int i, GameObject item)
    {
        //이미지 표시
        if (rewardData[i].category == "C100")
        {
            item.transform.Find("Img").transform.Find("Custom").gameObject.SetActive(false);
            switch (rewardData[i].code)
            {
                case "M001":
                    item.transform.Find("Img").transform.Find("Gold").gameObject.SetActive(true);
                    item.transform.Find("Img").transform.Find("Dia").gameObject.SetActive(false);
                    item.transform.Find("Img").transform.Find("Ticket").gameObject.SetActive(false);
                    break;
                case "M002":
                    item.transform.Find("Img").transform.Find("Gold").gameObject.SetActive(false);
                    item.transform.Find("Img").transform.Find("Dia").gameObject.SetActive(true);
                    item.transform.Find("Img").transform.Find("Ticket").gameObject.SetActive(false);
                    break;
                case "M003":
                    item.transform.Find("Img").transform.Find("Gold").gameObject.SetActive(false);
                    item.transform.Find("Img").transform.Find("Dia").gameObject.SetActive(false);
                    item.transform.Find("Img").transform.Find("Ticket").gameObject.SetActive(true);
                    break;
            }
        }
        else
        {
            string objName = rewardData[i].iconImg;
            item.transform.Find("Img").transform.Find("Gold").gameObject.SetActive(true);
            item.transform.Find("Img").transform.Find("Dia").gameObject.SetActive(false);
            item.transform.Find("Img").transform.Find("Ticket").gameObject.SetActive(false);
            item.transform.Find("Img").transform.Find("Custom").gameObject.SetActive(true);
            GameObject nItem = gObjectManager.getObjToPool("100_Prefabs/010_inven", objName, item.transform.Find("Img").transform.Find("Custom").gameObject);
            if (nItem != null)
            {
                RectTransform pRect = nItem.GetComponent<RectTransform>();
                pRect.offsetMin = new Vector3(0.0f, 0.0f);
                pRect.offsetMax = new Vector3(0.0f, 0.0f);
                pRect.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                pRect.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                nItem.SetActive(false);
            }
        }
        //개수 표시
        gText.setText(item.transform.Find("Cnt").transform.Find("Text").gameObject, string.Format("{0:n0}", rewardData[i].count));
    }


    //스킵 버튼
    public void clickBtnSkip()
    {
        bSkip = true;
    }

}
