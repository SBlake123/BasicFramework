/// [스크립트 명세]------------------------------------------------------------------------------------------------------------------------
/// ※ 정의 : 게임 내 UI에 관련한 공용 처리 함수
/// --------------------------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System;
using UnityEngine.UI;
using UniRx;
using UnityEngine.EventSystems;
using TMPro;

public class gUi : MonoBehaviour
{

    //[일반 UI 관련]================================================================================================
    
    //보이는 영역을 체크한다.
    private static Vector2 nowViewRectVal;
    //유니티 게임의 버그일수도 있지만 간혹 rect가 제대로 셋팅 안되는 경우가 발생하기 때문에 두번 처리한다.
    private static bool bOneMore = false;

    //<시간에 따라 플로트 값을 넘겨준다. 0:증가 1:감소>
    public static float setAlphaValueOverTime(int type, float nTimer, float nTime)
    {
        float nAlpha = 0.0f;
        //byte nAlphaByte;

        if(type == 0)
        {
            //값을 증가시킨다.
            nAlpha = 255.0f / nTime * nTimer;
        }
        else
        {
            //값을 감소시킨다.
            nAlpha = 255.0f / nTime * (nTime - nTimer);
        }
        //Mathf.Clamp(nAlpha, 0.0f, 255.0f);
        if (nAlpha < 0.0f)
        {
            nAlpha = 0.0f;
        }
        else if (nAlpha > 255.0f)
        {
            nAlpha = 255.0f;
        }


        return nAlpha;
    }

    //<뒷배경 패널 최적화>
    public static void setBackPanel(GameObject backPanel)
    {
        RectTransform bRect = backPanel.GetComponent<RectTransform>();
        float baseVal = bRect.sizeDelta.y / bRect.sizeDelta.x;
        float screenVal = (float)Screen.height / (float)Screen.width;
        float sVal = Mathf.Abs(baseVal - screenVal) * 3.0f;
        bRect.localScale = new Vector3(1.0f + sVal, 1.0f + sVal, 1.0f);
    }

    //<프로그래스 바 구현>
    public static void setPrograssBar(GameObject pBar, float maxVal, float nowVal, float marginX = 0.0f, float marginY = 0.0f)
    {
        float gVal = 0.0f;
        if (maxVal > 0)
        {
            gVal = nowVal / maxVal;

            if (gVal > 1.0f)
            {
                gVal = 1.0f;
            }
        }
        if (pBar.transform.Find("Back") != null && pBar.transform.Find("In") != null)
        {
            RectTransform gBack = pBar.transform.Find("Back").gameObject.GetComponent<RectTransform>(); 
            GameObject gBar = pBar.transform.Find("In").gameObject;
            setAnchor(gBack, AnchorPresets.MiddleCenter);
            setAnchor(gBar.GetComponent<RectTransform>(), AnchorPresets.MiddleCenter);

            gBack.sizeDelta = new Vector2(pBar.GetComponent<RectTransform>().rect.width - marginX, pBar.GetComponent<RectTransform>().rect.height - marginY);

            gBar.GetComponent<RectTransform>().sizeDelta = new Vector2(gBack.rect.width * gVal, gBack.rect.height);
            gBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(-((gBack.rect.width - (gBack.rect.width * gVal)) / 2.0f), 0.0f);
        }
    }

    public static void setPrograssBarPivot(RectTransform rBar, GameObject gauge, float maxVal, float nowVal, float marginX = 0.0f, float marginY = 0.0f)
    {
        float gVal = 0.0f;
        if (maxVal > 0)
        {
            gVal = nowVal / maxVal;

            if (gVal > 1.0f)
            {
                gVal = 1.0f;
            }
        }
        setAnchor(gauge.GetComponent<RectTransform>(), AnchorPresets.MiddleCenter);

        gauge.GetComponent<RectTransform>().sizeDelta = new Vector2(rBar.rect.width * gVal - marginX, rBar.rect.height - marginY);
        gauge.GetComponent<RectTransform>().pivot = new Vector2(0.0f, 0.5f);
        gauge.GetComponent<RectTransform>().anchoredPosition = new Vector3(0.0f, 0.0f, 0.0f);
        gauge.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
        //gauge.GetComponent<RectTransform>().anchoredPosition = new Vector2(-((rBar.rect.width - (rBar.rect.width * gVal)) / 2.0f), 0.0f);
    }

    //그래프에서 두 점 사이의 라인을 그리는 함수
    public static void drawGraphLine(RectTransform action, Vector2 prevPos, Vector2 nowPos, float border)
    {
        float posX = prevPos.x + Mathf.Abs(prevPos.x - nowPos.x) / 2.0f;
        float posY = 0.0f;
        posY = Mathf.Abs(prevPos.y + nowPos.y) / 2.0f;
        float width = gSystem.distanceToPoint(prevPos, nowPos);
        float height = border;
        float angleZ = gSystem.angleToPoint(prevPos, nowPos);

        action.sizeDelta = new Vector2(width, height);
        action.localScale = Vector3.one;
        action.anchoredPosition = new Vector2(posX, posY);
        action.localEulerAngles = new Vector3(0.0f, 0.0f, angleZ);
    }

    //드래그 무브 밸류를 리턴한다.
    public static float getDragMoveVal(Transform transform, Camera cam, float seedVal = 5.0f)
    {
        float dragMoveVal = 0.0f;
        
        Canvas canvas = transform.root.GetComponentInChildren<Canvas>();
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            //스크린 스페이스가 카메라일 때
            if (cam.orthographic == true)
            {
                dragMoveVal = seedVal / 100.0f * cam.orthographicSize;
            }
            else
            {
                dragMoveVal = seedVal / 100.0f * cam.fieldOfView;
            }
        }
        else if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            //스크린 스페이스가 오버레이일 때
            dragMoveVal = seedVal * 5.0f;
        }
        else
        {
            //월드 스페이스일 때. 추후에 업데이트한다.
            dragMoveVal = seedVal * 1.0f;
        }
        return dragMoveVal;
    }

    //스크린 포인터 값을 가져와 부모의 로컬 좌표로 변환
    public static Vector2 getPosScreenToLocal(RectTransform parents, Vector2 screenPointer, Camera cam)
    {
        Vector2 anchoredPos;
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(dragPanel.GetComponent<RectTransform>(), pointerData.position, Camera.main, out anchoredPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parents, screenPointer, cam, out anchoredPos);

        return anchoredPos;
    }

    //오버로드 1
    public static Vector2 getPosScreenToLocal(RectTransform parents, Vector2 screenPointer, Transform transform, Camera cam)
    {
        Vector2 anchoredPos;

        Canvas canvas = transform.root.GetComponentInChildren<Canvas>();
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            //스크린 스페이스가 카메라일 때
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parents, screenPointer, cam, out anchoredPos);
        }
        else if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            //스크린 스페이스가 오버레이일 때
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parents, screenPointer, null, out anchoredPos);
        }
        else
        {
            //월드 스페이스일 때. 추후에 업데이트한다.
            anchoredPos = screenPointer;
        }
        return anchoredPos;
    }

    //---------------------------------------------------------------------------------------------------------



    //[오브젝트 액티브 관련]===================================================================================
    //해당 오브젝트 안에 해당 이름의 오브젝트가 있는지 확인하고 해당 오브젝트의 Active 상태를 변경한다.
    public static void checkActiveGameObject(GameObject obj, string childName, bool bActive)
    {
        if (obj.transform.Find(childName) != null)
        {
            if (obj.transform.Find(childName).gameObject.activeInHierarchy != bActive)
                obj.transform.Find(childName).gameObject.SetActive(bActive);
        }
    }
    //----------------------------------------------------------------------------------------------------------



    //[동적 리스트 처리 관련]====================================================================================================================

    //동적 리스트 밸류 목록
    public struct dynamicListVal
    {
        public int itemCnt;
        public int rowCnt;
        public int colCnt;
        public float width;
        public float ratio;
        public float height;
        public int rowCount;
        public int colCount;
        public float scrollHeight;
        public float scrollWidth;
        public float scrollPos;
    }

    //[이하 버티컬(수직) 리스트 처리 함수]======================================================================================================

    //버티컬(수직) 리스트 패널 셋팅
    public static dynamicListVal setDynamicVerticalList(GameObject view, GameObject sPanel, GameObject sBar, GameObject sItem, int itemCnt, int colCnt, float scrollPos = 1.0f, float posX = 0.0f)
    {
        RectTransform rowRectTransform = sItem.GetComponent<RectTransform>();
        RectTransform conRectTransform = sPanel.GetComponent<RectTransform>();
        RectTransform viewRectTransform = view.GetComponent<RectTransform>();

        //앵커값을 처리한다.
        setAnchor(conRectTransform, AnchorPresets.HorStretchMiddle);    //sPanel은 미들 스트레치
        //setAnchor(viewRectTransform, AnchorPresets.StretchAll);       //view는 풀 스트레치
        setAnchor(rowRectTransform, AnchorPresets.TopLeft);             //아이템은 탑 레프트

        //앵커에서 Left, Right값 0.0f로 고정
        conRectTransform.offsetMin = new Vector2(0.0f, conRectTransform.offsetMin.y);
        conRectTransform.offsetMax = new Vector2(0.0f, conRectTransform.offsetMax.y);

        gUi.dynamicListVal lVal = new gUi.dynamicListVal();

        //기존의 아이템을 삭제한다
        gUi.removeChild(sPanel);

        //리스트에 사용할 밸류값을 셋팅한다.
        lVal = gUi.setDynamicVerticalListValue(itemCnt, colCnt, rowRectTransform, conRectTransform);

        //리스트 패널과 스크롤을 셋팅한다.
        gUi.setDynamicVerticalListScroll(itemCnt, lVal.scrollHeight, posX, scrollPos, viewRectTransform, conRectTransform, sBar, true);

        lVal.scrollPos = scrollPos;

        bOneMore = true;

        return lVal;
    }

    //버티컬(수직) 리스트 패널 업데이트
    public static dynamicListVal updateDynamicVerticalList(GameObject view, GameObject sPanel, GameObject sBar, GameObject sItem, int itemCnt, int colCnt, float scrollPos = 1.0f, float posX = 0.0f)
    {
        RectTransform rowRectTransform = sItem.GetComponent<RectTransform>();
        RectTransform conRectTransform = sPanel.GetComponent<RectTransform>();
        RectTransform viewRectTransform = view.GetComponent<RectTransform>();

        //앵커값을 처리한다.
        setAnchor(conRectTransform, AnchorPresets.HorStretchMiddle);    //sPanel은 미들 스트레치
        //setAnchor(viewRectTransform, AnchorPresets.StretchAll);       //view는 풀 스트레치
        setAnchor(rowRectTransform, AnchorPresets.TopLeft);             //아이템은 탑 레프트

        //앵커에서 Left, Right값 0.0f로 고정
        conRectTransform.offsetMin = new Vector2(0.0f, conRectTransform.offsetMin.y);
        conRectTransform.offsetMax = new Vector2(0.0f, conRectTransform.offsetMax.y);

        gUi.dynamicListVal lVal = new gUi.dynamicListVal();

        //리스트에 사용할 밸류값을 셋팅한다.
        lVal = gUi.setDynamicVerticalListValue(itemCnt, colCnt, rowRectTransform, conRectTransform);

        //리스트 패널과 스크롤을 업데이트한다.
        lVal.scrollPos = gUi.updateDynamicVerticalList(itemCnt, lVal.scrollHeight, posX, scrollPos, viewRectTransform, conRectTransform, sBar, true);

        return lVal;
    }

    //동적 버티컬(수직) 리스트에서 아이템을 처리한다.
    public static void setVerticalListItem(string itemName, GameObject item, GameObject parent, dynamicListVal lVal, int j, int iCnt)
    {
        item.name = itemName;
        item.transform.SetParent(parent.transform);

        RectTransform rectTransform = item.GetComponent<RectTransform>();

        //아이템의 Rect Trensform 기준은 top left 여야 한다.
        setAnchor(rectTransform, AnchorPresets.TopLeft);

        float posX = +(lVal.width / 2.0f) + (lVal.width * (iCnt % lVal.colCnt));
        float posY = -(lVal.height / 2.0f) - (lVal.height * (j - 1));

        rectTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        rectTransform.sizeDelta = new Vector2(lVal.width, lVal.height);
        rectTransform.anchoredPosition3D = new Vector3(posX, posY, 0.0f);
    }

    //버티컬(수직) 리스트의 스크롤값을 고정값 기준으로 처리한다.
    public static float getVerticalScrollValFixed(GameObject view, GameObject sPanel, GameObject sBar, GameObject sItem, float speedVal)
    {
        RectTransform conRect = sPanel.GetComponent<RectTransform>();
        RectTransform viewRect = view.GetComponent<RectTransform>();
        RectTransform itemRect = sItem.GetComponent<RectTransform>();


        float reVal = 0.0f;
        float vVal = viewRect.rect.height / conRect.rect.height;
        float iVal = (itemRect.rect.height / viewRect.rect.height) * 10.0f;
        float sVal = 0.0f;
        if (iVal > 1.0f)
        {
            sVal = (vVal * iVal) * speedVal;
        }
        else
        {
            sVal = vVal * speedVal;
        }
        if (sVal < 0.1f)
        {
            sVal = 0.1f;
        }
        //Debug.Log(string.Format("itemRect = {0}, viewRect = {1}, vVal = {2}, iVal = {3}, sVal = {4}", itemRect.rect.height, viewRect.rect.height, vVal, iVal, sVal));
        reVal = sBar.GetComponent<Scrollbar>().value - (gBase.posValue * 0.25f * sVal);
        if (reVal < 0.0f)
        {
            reVal = 0.0f;
        }
        else if (reVal > 1.0f)
        {
            reVal = 1.0f;
        }
        return reVal;
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------



    //[이하 동적 버티컬 리스트의 계산식 함수]==============================================================================================================

    //동적 버티컬(수직) 리스트의 밸류값을 가져와서 필요한 값을 리턴한다.
    private static dynamicListVal setDynamicVerticalListValue(int itemCnt, int colCnt, RectTransform rowRectTransform, RectTransform conRectTransform)
    {
        dynamicListVal reVal = new dynamicListVal();
        reVal.itemCnt = 0;
        reVal.colCnt = 0;
        reVal.width = 0.0f;
        reVal.ratio = 0.0f;
        reVal.height = 0.0f;
        reVal.rowCount = 0;
        reVal.scrollHeight = 0.0f;

        if (itemCnt > 0 && colCnt > 0)
        {
            reVal.itemCnt = itemCnt;
            reVal.colCnt = colCnt;
            reVal.width = conRectTransform.rect.width / colCnt;
            reVal.ratio = reVal.width / rowRectTransform.rect.width;
            reVal.height = rowRectTransform.rect.height;// * reVal.ratio;
            reVal.rowCount = itemCnt / colCnt;
            if (reVal.rowCount < 1)
                reVal.rowCount = 1;
            if (itemCnt > colCnt && itemCnt % colCnt > 0)
                reVal.rowCount++;
            reVal.scrollHeight = reVal.height * reVal.rowCount;
        }
        return reVal;
    }

    /*
    //동적 버티컬(수직) 리스트의 Row 밸류값을 가져와서 필요한 값을 리턴한다.
    private static dynamicListVal setDynamicVerticalListByRow(int rowCnt, int colCnt, RectTransform rowRectTransform, RectTransform conRectTransform)
    {
        dynamicListVal reVal = new dynamicListVal();
        reVal.itemCnt = 0;
        reVal.colCnt = 0;
        reVal.width = 0.0f;
        reVal.ratio = 0.0f;
        reVal.height = 0.0f;
        reVal.rowCount = 0;
        reVal.scrollHeight = 0.0f;

        if (colCnt > 0)
        {
            reVal.itemCnt = rowCnt / colCnt;
            reVal.colCnt = colCnt;
            reVal.width = conRectTransform.rect.width / colCnt;
            reVal.ratio = reVal.width / rowRectTransform.rect.width;
            reVal.height = rowRectTransform.rect.height;// * reVal.ratio;
            reVal.rowCount = rowCnt;
            reVal.scrollHeight = reVal.height * reVal.rowCount;
        }
        return reVal;
    }
    */

    //동적 버티컬(수직) 리스트의 스크롤값을 셋팅한다.
    private static void setDynamicVerticalListScroll(int itemCnt, float scrollHeight, float posX, float scrollPos, RectTransform viewTransform, RectTransform conRectTransform, GameObject bar, bool bBarVisible)
    {
        gBase.checkScroll = false;
        if (itemCnt <= 0 || scrollHeight <= viewTransform.rect.height)
        {
            //Debug.Log("!!!");
            bar.SetActive(bBarVisible);
            if (bBarVisible == false)
            {
                posX = 0.0f;
            }
            bar.GetComponent<Scrollbar>().value = 0.5f;
            bar.GetComponent<Scrollbar>().size = 1.0f;
            viewTransform.GetComponent<ScrollRect>().enabled = false;
            if (itemCnt > 0)
            {
                conRectTransform.offsetMin = new Vector2(conRectTransform.offsetMin.x, -scrollHeight / 2);
                conRectTransform.offsetMax = new Vector2(conRectTransform.offsetMax.x, scrollHeight / 2);
                //conRectTransform.localPosition = new Vector2(posX, (viewTransform.rect.height - scrollHeight) / 2);
                conRectTransform.anchoredPosition3D = new Vector3(posX, (viewTransform.rect.height - scrollHeight) / 2.0f, 0.0f);
            }
            else
            {
                //conRectTransform.localPosition = new Vector2(posX, 0.0f);
                conRectTransform.anchoredPosition3D = new Vector3(posX, 0.0f, 0.0f);
            }
            //Debug.Log(string.Format("viewHeight = {0}, scrollHeight = {1}, posY = {2}", viewTransform.rect.height, scrollHeight, conRectTransform.anchoredPosition3D));

        }
        else
        {
            //Debug.Log(scrollPos);
            bar.SetActive(true);
            conRectTransform.offsetMin = new Vector2(conRectTransform.offsetMin.x, -scrollHeight / 2.0f);
            conRectTransform.offsetMax = new Vector2(conRectTransform.offsetMax.x, scrollHeight / 2.0f);
            float scY = (scrollPos * 2.0f) - 1.0f;
            viewTransform.GetComponent<ScrollRect>().enabled = true;
            conRectTransform.anchoredPosition3D = new Vector3(posX, ((viewTransform.rect.height - scrollHeight) / 2.0f) * scY, 0.0f);
        }
        nowViewRectVal = new Vector2(viewTransform.rect.width, viewTransform.rect.height);
        gBase.checkScroll = true;
    }

    //동적 버티컬(수직) 리스트의 스크롤값을 업데이트 중에 다시 셋팅한다.
    private static float updateDynamicVerticalList(int itemCnt, float scrollHeight, float posX, float scrollPos, RectTransform viewTransform, RectTransform conRectTransform, GameObject bar, bool bBarVisible)
    {
        gBase.checkScroll = false;
        if(nowViewRectVal.x != viewTransform.rect.width || nowViewRectVal.y != viewTransform.rect.height || bOneMore == true)
        {
            //사이즈가 바뀌었다!
            //Debug.Log(string.Format("Change Resolution! SH = {0}, vRect = {1}", scrollHeight, viewTransform.rect.height));
            if (itemCnt <= 0 || scrollHeight <= viewTransform.rect.height)
            {
                bar.SetActive(bBarVisible);
                if (bBarVisible == false)
                {
                    posX = 0.0f;
                }
                //scrollPos = bar.GetComponent<Scrollbar>().value;
                bar.GetComponent<Scrollbar>().value = 0.5f;
                bar.GetComponent<Scrollbar>().size = 1.0f;
                viewTransform.GetComponent<ScrollRect>().enabled = false;
                if (itemCnt > 0)
                {
                    conRectTransform.offsetMin = new Vector2(conRectTransform.offsetMin.x, -scrollHeight / 2.0f);
                    conRectTransform.offsetMax = new Vector2(conRectTransform.offsetMax.x, scrollHeight / 2.0f);
                    conRectTransform.anchoredPosition3D = new Vector3(posX, (viewTransform.rect.height - scrollHeight) / 2.0f, 0.0f);
                }
                else
                {
                    conRectTransform.anchoredPosition3D = new Vector3(posX, 0.0f, 0.0f);
                }
                //Debug.Log(string.Format("viewHeight = {0}, scrollHeight = {1}, posY = {2}", viewTransform.rect.height, scrollHeight, conRectTransform.anchoredPosition3D));
            }
            else
            {
                bar.SetActive(true);
                float scY = (scrollPos * 2.0f) - 1.0f;
                viewTransform.GetComponent<ScrollRect>().enabled = true;
                conRectTransform.offsetMin = new Vector2(conRectTransform.offsetMin.x, -scrollHeight / 2.0f);
                conRectTransform.offsetMax = new Vector2(conRectTransform.offsetMax.x, scrollHeight / 2.0f);
                conRectTransform.anchoredPosition3D = new Vector3(posX, ((viewTransform.rect.height - scrollHeight) / 2.0f) * scY, 0.0f);
                bar.GetComponent<Scrollbar>().value = scrollPos;
            }
            if (nowViewRectVal.x == viewTransform.rect.width && nowViewRectVal.y == viewTransform.rect.height && bOneMore == true)
            {
                bOneMore = false;
            }
            else
            {
                bOneMore = true;
            }
            nowViewRectVal = new Vector2(viewTransform.rect.width, viewTransform.rect.height);
        }
        else
        {
            if (itemCnt > 0 && scrollHeight > viewTransform.rect.height)
            {
                scrollPos = bar.GetComponent<Scrollbar>().value;
            }
            bOneMore = false;
        }

        return scrollPos;
    }

    /*
    //버티컬(수직) 리스트의 스크롤값을 처리한다.
    public static float getVerticalScrollVal(RectTransform conRectTransform, RectTransform viewTransform, GameObject bar)
    {
        float reVal = 0.0f;
        //float sVal = conRectTransform.sizeDelta.y / viewTransform.sizeDelta.y;    //스트레치 상태에서는 값이 제대로 나오지 않으니 아래의 rect를 써야 한다.
        float sVal = conRectTransform.rect.height / viewTransform.rect.height;
        //Debug.Log(sVal);
        if (sVal < 1.0f)
        {
            sVal = 1.0f;
        }
        reVal = bar.GetComponent<Scrollbar>().value - (gBase.posValue / (sVal * 4.0f));
        if (reVal < 0.0f)
        {
            reVal = 0.0f;
        }
        else if (reVal > 1.0f)
        {
            reVal = 1.0f;
        }
        return reVal;
    }
    */

    //--------------------------------------------------------------------------------------------------------------------------------------------



    //[이하 호라이즌(수평) 리스트 처리 함수]========================================================================================================

    //호라이즌 리스트 패널 셋팅
    public static dynamicListVal setDynamicHorizonList(GameObject view, GameObject sPanel, GameObject sBar, GameObject sItem, int itemCnt, int rowCnt, float scrollPos = 0.0f, float posX = 0.0f)
    {
        RectTransform rowRectTransform = sItem.GetComponent<RectTransform>();
        RectTransform conRectTransform = sPanel.GetComponent<RectTransform>();
        RectTransform viewRectTransform = view.GetComponent<RectTransform>();

        //앵커값을 처리한다.
        setAnchor(conRectTransform, AnchorPresets.VertStretchCenter);   //sPanel은 버티컬 스트레치 센터
        setAnchor(rowRectTransform, AnchorPresets.TopLeft);             //아이템은 탑 레프트

        //앵커에서 Top, Bottom값 0.0f로 고정
        conRectTransform.offsetMin = new Vector2(conRectTransform.offsetMin.x, 0.0f);
        conRectTransform.offsetMax = new Vector2(conRectTransform.offsetMax.y, 0.0f);

        gUi.dynamicListVal lVal = new gUi.dynamicListVal();

        //기존의 아이템을 삭제한다
        gUi.removeChild(sPanel);

        //리스트에 사용할 밸류값을 셋팅한다.
        lVal = gUi.setDynamicHorizonListValue(itemCnt, rowCnt, rowRectTransform, conRectTransform);

        //리스트 패널과 스크롤을 셋팅한다.
        gUi.setDynamicHorizonListScroll(itemCnt, lVal.scrollWidth, posX, scrollPos, viewRectTransform, conRectTransform, sBar, true);

        lVal.scrollPos = scrollPos;

        bOneMore = true;

        return lVal;
    }

    //호라이즌 리스트 패널 업데이트
    public static dynamicListVal updateDynamicHorizonList(GameObject view, GameObject sPanel, GameObject sBar, GameObject sItem, int itemCnt, int rowCnt, float scrollPos = 0.0f, float posX = 0.0f)
    {
        RectTransform rowRectTransform = sItem.GetComponent<RectTransform>();
        RectTransform conRectTransform = sPanel.GetComponent<RectTransform>();
        RectTransform viewRectTransform = view.GetComponent<RectTransform>();

        //앵커값을 처리한다.
        setAnchor(conRectTransform, AnchorPresets.VertStretchCenter);   //sPanel은 버티컬 스트레치 센터
        setAnchor(rowRectTransform, AnchorPresets.TopLeft);             //아이템은 탑 레프트

        //앵커에서 Top, Bottom값 0.0f로 고정
        conRectTransform.offsetMin = new Vector2(conRectTransform.offsetMin.x, 0.0f);
        conRectTransform.offsetMax = new Vector2(conRectTransform.offsetMax.y, 0.0f);

        gUi.dynamicListVal lVal = new gUi.dynamicListVal();

        //리스트에 사용할 밸류값을 셋팅한다.
        lVal = gUi.setDynamicHorizonListValue(itemCnt, rowCnt, rowRectTransform, conRectTransform);

        //리스트 패널과 스크롤을 업데이트한다.
        lVal.scrollPos = gUi.updateDynamicHorizonList(itemCnt, lVal.scrollWidth, posX, scrollPos, viewRectTransform, conRectTransform, sBar, true);

        return lVal;
    }

    //동적 호라이즌(수평) 리스트에서 아이템의 위치를 처리한다.
    public static void setHorizonListItem(string itemName, GameObject item, GameObject parent, dynamicListVal lVal, int j, int iCnt)
    {
        item.name = itemName;
        item.transform.SetParent(parent.transform);

        RectTransform rectTransform = item.GetComponent<RectTransform>();

        //아이템의 Rect Trensform 기준은 top left 여야 한다.
        setAnchor(rectTransform, AnchorPresets.TopLeft);

        float posX = (lVal.width / 2.0f) + (lVal.width * (j - 1));
        float posY = -(lVal.height / 2.0f) - (lVal.height * (iCnt % lVal.rowCnt));

        rectTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        rectTransform.sizeDelta = new Vector2(lVal.width, lVal.height);
        rectTransform.anchoredPosition3D = new Vector3(posX, posY, 0.0f);

        /*
        item.name = itemName;
        item.transform.SetParent(parent.transform);
        RectTransform rectTransform = item.GetComponent<RectTransform>();
        float x = -conRectTransform.rect.width / 2 + lVal.width * (j - 1);
        float y = conRectTransform.rect.height / 2 - lVal.height * ((iCnt % lVal.rowCnt) + 1);
        rectTransform.offsetMin = new Vector3(x, y, 1.0f);
        x = rectTransform.offsetMin.x + lVal.width;
        y = rectTransform.offsetMin.y + lVal.height;
        rectTransform.offsetMax = new Vector3(x, y, 1.0f);
        rectTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0.0f);
        */
    }


    //호라이즌(수평) 리스트의 스크롤값을 고정값 기준으로 처리한다.
    public static float getHorizonScrollValFixed(RectTransform conRect, RectTransform viewRect, RectTransform itemRect, GameObject bar, float speedVal)
    {
        float reVal = 0.0f;
        float vVal = viewRect.rect.width / conRect.rect.width;
        float iVal = (itemRect.rect.width / viewRect.rect.width) * 10.0f;
        float sVal = 0.0f;
        if (iVal > 1.0f)
        {
            sVal = (vVal * iVal) * speedVal;
        }
        else
        {
            sVal = vVal * speedVal;
        }
        if (sVal < 0.1f)
        {
            sVal = 0.1f;
        }
        //Debug.Log(string.Format("itemRect = {0}, viewRect = {1}, vVal = {2}, iVal = {3}, sVal = {4}", itemRect.rect.height, viewRect.rect.height, vVal, iVal, sVal));
        reVal = bar.GetComponent<Scrollbar>().value - (gBase.posValue * 0.25f * sVal);
        if (reVal < 0.0f)
        {
            reVal = 0.0f;
        }
        else if (reVal > 1.0f)
        {
            reVal = 1.0f;
        }
        return reVal;
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------



    //[이하 호라이즌(수평) 리스트 계산식]===========================================================================================================

    //동적 호라이즌(수평) 리스트의 밸류값을 가져와서 필요한 값을 리턴한다.
    private static dynamicListVal setDynamicHorizonListValue(int itemCnt, int rowCnt, RectTransform rowRectTransform, RectTransform conRectTransform)
    {
        dynamicListVal reVal = new dynamicListVal();
        reVal.itemCnt = 0;
        reVal.rowCnt = 0;
        reVal.width = 0.0f;
        reVal.ratio = 0.0f;
        reVal.height = 0.0f;
        reVal.colCount = 0;
        reVal.scrollWidth = 0.0f;

        if (itemCnt > 0 && rowCnt > 0)
        {
            reVal.itemCnt = itemCnt;
            reVal.rowCnt = rowCnt;
            reVal.height = conRectTransform.rect.height / rowCnt;
            reVal.ratio = reVal.height / rowRectTransform.rect.height;
            reVal.width = rowRectTransform.rect.width;  // * reVal.ratio;
            reVal.colCount = itemCnt / rowCnt;
            if (reVal.colCount < 1)
                reVal.colCount = 1;
            if (itemCnt > rowCnt && itemCnt % rowCnt > 0)
                reVal.colCount++;
            reVal.scrollWidth = reVal.width * reVal.colCount;
        }
        return reVal;
    }

    /*
    //동적 호라이즌(수평) 리스트의 col 밸류값을 가져와서 필요한 값을 리턴한다.
    public static dynamicListVal setDynamicHorizonListByRow(int colCnt, int rowCnt, RectTransform rowRectTransform, RectTransform conRectTransform)
    {
        dynamicListVal reVal = new dynamicListVal();
        reVal.itemCnt = 0;
        reVal.rowCnt = 0;
        reVal.width = 0.0f;
        reVal.ratio = 0.0f;
        reVal.height = 0.0f;
        reVal.colCount = 0;
        reVal.scrollWidth = 0.0f;

        if (colCnt > 0 && rowCnt > 0)
        {
            reVal.itemCnt = colCnt / rowCnt;
            reVal.rowCnt = rowCnt;
            reVal.height = conRectTransform.rect.height / rowCnt;
            reVal.ratio = reVal.height / rowRectTransform.rect.height;
            reVal.width = rowRectTransform.rect.width * reVal.ratio;
            reVal.colCount = colCnt;
            reVal.scrollWidth = reVal.width * reVal.colCount;
        }
        return reVal;
    }
    */

    //동적 호라이즌(수평) 리스트의 스크롤값을 셋팅한다.
    private static void setDynamicHorizonListScroll(int itemCnt, float scrollWidth, float posY, float scrollPos, RectTransform viewTransform, RectTransform conRectTransform, GameObject bar, bool bBarVisible)
    {
        gBase.checkScroll = false;
        if (itemCnt <= 0 || scrollWidth <= viewTransform.rect.width)
        {
            bar.SetActive(bBarVisible);
            if (bBarVisible == false)
            {
                posY = 0.0f;
            }
            bar.GetComponent<Scrollbar>().value = 0.5f;
            bar.GetComponent<Scrollbar>().size = 1.0f;
            viewTransform.GetComponent<ScrollRect>().enabled = false;
            if (itemCnt > 0)
            {
                conRectTransform.offsetMin = new Vector2(-scrollWidth / 2, conRectTransform.offsetMin.y);
                conRectTransform.offsetMax = new Vector2(scrollWidth / 2, conRectTransform.offsetMax.y);
                //conRectTransform.localPosition = new Vector2(-(viewTransform.rect.width - scrollWidth) / 2, posY);
                conRectTransform.anchoredPosition3D = new Vector3(-(viewTransform.rect.width - scrollWidth) / 2.0f, posY, 0.0f);
            }
            else
            {
                //conRectTransform.localPosition = new Vector2(0.0f, posY);
                conRectTransform.anchoredPosition3D = new Vector3(0.0f, posY, 0.0f);
            }
        }
        else
        {
            bar.SetActive(true);
            conRectTransform.offsetMin = new Vector2(-scrollWidth / 2, conRectTransform.offsetMin.y);
            conRectTransform.offsetMax = new Vector2(scrollWidth / 2, conRectTransform.offsetMax.y);
            float scX = (scrollPos * 2.0f) - 1.0f;
            viewTransform.GetComponent<ScrollRect>().enabled = true;
            //conRectTransform.localPosition = new Vector2(-((viewTransform.rect.width - scrollWidth) / 2) * -scX, posY);
            conRectTransform.anchoredPosition3D = new Vector3(-((viewTransform.rect.width - scrollWidth) / 2) * -scX, posY, 0.0f);
        }
        nowViewRectVal = new Vector2(viewTransform.rect.width, viewTransform.rect.height);
        gBase.checkScroll = true;
    }

    //동적 버티컬(수직) 리스트의 스크롤값을 업데이트 중에 다시 셋팅한다.
    private static float updateDynamicHorizonList(int itemCnt, float scrollWidth, float posY, float scrollPos, RectTransform viewTransform, RectTransform conRectTransform, GameObject bar, bool bBarVisible)
    {
        gBase.checkScroll = false;
        if (nowViewRectVal.x != viewTransform.rect.width || nowViewRectVal.y != viewTransform.rect.height || bOneMore == true)
        {
            //사이즈가 바뀌었다!
            //Debug.Log(string.Format("Change Resolution! SH = {0}, vRect = {1}", scrollHeight, viewTransform.rect.height));
            if (itemCnt <= 0 || scrollWidth <= viewTransform.rect.width)
            {
                bar.SetActive(bBarVisible);
                if (bBarVisible == false)
                {
                    posY = 0.0f;
                }
                bar.GetComponent<Scrollbar>().value = 0.5f;
                bar.GetComponent<Scrollbar>().size = 1.0f;
                viewTransform.GetComponent<ScrollRect>().enabled = false;
                if (itemCnt > 0)
                {
                    conRectTransform.offsetMin = new Vector2(-scrollWidth / 2, conRectTransform.offsetMin.y);
                    conRectTransform.offsetMax = new Vector2(scrollWidth / 2, conRectTransform.offsetMax.y);
                    //conRectTransform.localPosition = new Vector2(-(viewTransform.rect.width - scrollWidth) / 2, posY);
                    conRectTransform.anchoredPosition3D = new Vector3(-(viewTransform.rect.width - scrollWidth) / 2.0f, posY, 0.0f);
                }
                else
                {
                    //conRectTransform.localPosition = new Vector2(0.0f, posY);
                    conRectTransform.anchoredPosition3D = new Vector3(0.0f, posY, 0.0f);
                }
            }
            else
            {
                bar.SetActive(true);
                conRectTransform.offsetMin = new Vector2(-scrollWidth / 2, conRectTransform.offsetMin.y);
                conRectTransform.offsetMax = new Vector2(scrollWidth / 2, conRectTransform.offsetMax.y);
                float scX = (scrollPos * 2.0f) - 1.0f;
                viewTransform.GetComponent<ScrollRect>().enabled = true;
                //conRectTransform.localPosition = new Vector2(-((viewTransform.rect.width - scrollWidth) / 2) * -scX, posY);
                conRectTransform.anchoredPosition3D = new Vector3(-((viewTransform.rect.width - scrollWidth) / 2) * -scX, posY, 0.0f);
                bar.GetComponent<Scrollbar>().value = scrollPos;
            }
            if (nowViewRectVal.x == viewTransform.rect.width && nowViewRectVal.y == viewTransform.rect.height && bOneMore == true)
            {
                bOneMore = false;
            }
            else
            {
                bOneMore = true;
            }
            nowViewRectVal = new Vector2(viewTransform.rect.width, viewTransform.rect.height);
        }
        else
        {
            if (itemCnt <= 0 || scrollWidth <= viewTransform.rect.width)
            {
                scrollPos = bar.GetComponent<Scrollbar>().value;
            }
            bOneMore = false;
        }

        return scrollPos;
    }

    /*
    //호라이즌(수평) 리스트의 스크롤값을 처리한다.
    public static float getHorizonScrollVal(RectTransform conRectTransform, RectTransform viewTransform, GameObject bar)
    {
        float reVal = 0.0f;
        float sVal = conRectTransform.rect.width / viewTransform.rect.width;
        if (sVal < 1.0f)
        {
            sVal = 1.0f;
        }
        reVal = bar.GetComponent<Scrollbar>().value - (gBase.posValue / (sVal * 4.0f));
        if (reVal < 0.0f)
        {
            reVal = 0.0f;
        }
        else if (reVal > 1.0f)
        {
            reVal = 1.0f;
        }
        return reVal;
    }
    */

    //-----------------------------------------------------------------------------------------------------------



    //[동적 스크롤 페이지 처리 관련]==============================================================================
    //패널 안에 하나의 아이템(자동 리사이징되는 텍스트같은 것)을 처리할 때 사용하는 메소드
    public static float setDynamicVerticalScrollPage(RectTransform sViewRect, RectTransform sPanelRect, RectTransform sContentsRect, GameObject sBar, float panelPosX, float paddingY)
    {
        sContentsRect.anchoredPosition = new Vector3(0.0f, -((sContentsRect.rect.height / 2.0f) + (paddingY / 2.0f)), 0.0f);
        //패널 크기 재조정 및 위치 선정
        if (sContentsRect.rect.height + paddingY >= sViewRect.rect.height)
        {
            sPanelRect.sizeDelta = new Vector2(sPanelRect.rect.width, sContentsRect.rect.height + paddingY);
            sPanelRect.anchoredPosition = new Vector3(panelPosX, (sViewRect.rect.height - (sContentsRect.rect.height + paddingY) / 2.0f), 0.0f);
            sViewRect.GetComponent<ScrollRect>().enabled = true;
            sBar.SetActive(true);
            sBar.GetComponent<Scrollbar>().value = 1.0f;
        }
        else
        {
            sPanelRect.sizeDelta = new Vector2(sPanelRect.rect.width, sViewRect.rect.height);
            sPanelRect.anchoredPosition = new Vector3(0.0f, 0.0f, 0.0f);
            sViewRect.GetComponent<ScrollRect>().enabled = false;
            sBar.SetActive(false);
            sBar.GetComponent<Scrollbar>().value = 0.5f;
            sBar.GetComponent<Scrollbar>().size = 1.0f;
        }
        return sContentsRect.rect.height;
    }

    public static void setDynamicVerticalScroll(RectTransform sViewRect, RectTransform sContentsRect, GameObject sBar, float paddingY)
    {
        if (sContentsRect.rect.height + paddingY >= sViewRect.rect.height)
        {
            sBar.SetActive(true);
            sBar.GetComponent<Scrollbar>().value = 1.0f;
        }
        else
        {
            sBar.SetActive(false);
            sBar.GetComponent<Scrollbar>().value = 0.5f;
            sBar.GetComponent<Scrollbar>().size = 1.0f;
        }
    }
    //-----------------------------------------------------------------------------------------------------------



    //[이하 UI 오브젝트 풀 관련]=================================================================================

    public static void removeChild(GameObject parents)
    {
        for (int i = parents.transform.childCount - 1; i >= 0; i--)
        {
            try
            {
                //해당 오브젝트들을 재활용 통에 넣는다.
                gObjectManager.setObjRecyclingBin(parents.transform.GetChild(i).gameObject);
            }
            catch
            {

            }
        }
    }

    public static void removeItem(GameObject item)
    {
        //해당 오브젝트를 재활용 통에 넣는다.
        gObjectManager.setObjRecyclingBin(item);
    }

    public static GameObject getItem(GameObject item, string itemName)
    {
        //재활용 통에서 가져오거나 생성한다.
        GameObject nItem = gObjectManager.getObjToRecyclingBin(itemName);
        if (nItem == null)
        {
            nItem = Instantiate(item) as GameObject;
        }
        return nItem;
    }

    public static GameObject setItemRoute(GameObject parent, string itemName, string route, float posX = 0.0f, float posY = 0.0f, float scale = 1.0f)
    {
        //재활용 통에서 가져오거나 생성한다.
        GameObject nItem = gObjectManager.getObjToRecyclingBin(itemName);
        if (nItem == null)
        {
            nItem = Instantiate(Resources.Load<GameObject>(route));
        }
        if(nItem != null)
        {
            nItem.transform.SetParent(parent.transform);
            nItem.GetComponent<RectTransform>().localScale = new Vector3(scale, scale, scale);
            nItem.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(posX, posY, 0.0f);
        }
        return nItem;
    }

    public static GameObject loadGameObject(string itemName, string route)
    {
        GameObject nItem = null;
        if (itemName.Length > 0 && route.Length > 0)
        {
            //재활용 통에서 가져오거나 생성한다.
            nItem = gObjectManager.getObjToRecyclingBin(itemName);
            if (nItem == null)
            {
                if (Resources.Load<GameObject>(route) != null)
                {
                    nItem = Instantiate(Resources.Load<GameObject>(route));
                }
            }
            if (nItem != null)
            {
                nItem.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
                nItem.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }
        return nItem;
    }

    public static GameObject setItemSpine(GameObject parent, string itemName, string route)
    {
        //재활용 통에서 가져오거나 생성한다.
        GameObject nItem = gObjectManager.getObjToRecyclingBin(itemName);
        if (nItem == null)
        {
            nItem = Instantiate(Resources.Load<GameObject>(route));
        }
        if (nItem != null)
        {
            Vector3 sVal = nItem.GetComponent<RectTransform>().localScale;
            Vector3 pVal = nItem.GetComponent<RectTransform>().anchoredPosition3D;

            nItem.transform.SetParent(parent.transform);
            nItem.GetComponent<RectTransform>().localScale = sVal;
            nItem.GetComponent<RectTransform>().anchoredPosition3D = pVal;
        }
        return nItem;
    }

    //--------------------------------------------------------------------------------------------------


    //[이하 앵커 값 스크립트 사용]======================================================================

    public enum AnchorPresets
    {
        TopLeft,
        TopCenter,
        TopRight,

        MiddleLeft,
        MiddleCenter,
        MiddleRight,

        BottomLeft,
        BottonCenter,
        BottomRight,
        BottomStretch,

        VertStretchLeft,
        VertStretchRight,
        VertStretchCenter,

        HorStretchTop,
        HorStretchMiddle,
        HorStretchBottom,

        StretchAll
    }

    public static void setAnchor(RectTransform source, AnchorPresets allign)//, int offsetX = 0, int offsetY = 0)
    {
        //source.anchoredPosition = new Vector3(offsetX, offsetY, 0);

        switch (allign)
        {
            case (AnchorPresets.TopLeft):
                {
                    source.anchorMin = new Vector2(0, 1);
                    source.anchorMax = new Vector2(0, 1);
                    break;
                }
            case (AnchorPresets.TopCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 1);
                    source.anchorMax = new Vector2(0.5f, 1);
                    break;
                }
            case (AnchorPresets.TopRight):
                {
                    source.anchorMin = new Vector2(1, 1);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }

            case (AnchorPresets.MiddleLeft):
                {
                    source.anchorMin = new Vector2(0, 0.5f);
                    source.anchorMax = new Vector2(0, 0.5f);
                    break;
                }
            case (AnchorPresets.MiddleCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 0.5f);
                    source.anchorMax = new Vector2(0.5f, 0.5f);
                    break;
                }
            case (AnchorPresets.MiddleRight):
                {
                    source.anchorMin = new Vector2(1, 0.5f);
                    source.anchorMax = new Vector2(1, 0.5f);
                    break;
                }

            case (AnchorPresets.BottomLeft):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(0, 0);
                    break;
                }
            case (AnchorPresets.BottonCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 0);
                    source.anchorMax = new Vector2(0.5f, 0);
                    break;
                }
            case (AnchorPresets.BottomRight):
                {
                    source.anchorMin = new Vector2(1, 0);
                    source.anchorMax = new Vector2(1, 0);
                    break;
                }

            case (AnchorPresets.HorStretchTop):
                {
                    source.anchorMin = new Vector2(0, 1);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }
            case (AnchorPresets.HorStretchMiddle):
                {
                    source.anchorMin = new Vector2(0, 0.5f);
                    source.anchorMax = new Vector2(1, 0.5f);
                    break;
                }
            case (AnchorPresets.HorStretchBottom):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(1, 0);
                    break;
                }

            case (AnchorPresets.VertStretchLeft):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(0, 1);
                    break;
                }
            case (AnchorPresets.VertStretchCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 0);
                    source.anchorMax = new Vector2(0.5f, 1);
                    break;
                }
            case (AnchorPresets.VertStretchRight):
                {
                    source.anchorMin = new Vector2(1, 0);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }

            case (AnchorPresets.StretchAll):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }
        }
    }
    //--------------------------------------------------------------------------------------------------


    //[이벤트 등록]=====================================================================================

    //버튼 이벤트를 등록한다.
    public static void addButtonListener(Button btn, Action action)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => { action.Invoke(); });
    }

    //메소드 오버로딩 1. 동적 아이템을 위한 버튼 이벤트를 등록한다.
    public static void addButtonListener(Button btn, Action<GameObject> action, GameObject item = null)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => { action.Invoke(item); });
    }

    //메소드 오버로딩 2. 부모를 기억해야 하는 동적 아이템을 위한 버튼 이벤트를 등록한다.
    public static void addButtonListener(Button btn, Action<GameObject, GameObject> action, GameObject parents = null, GameObject item = null)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => { action.Invoke(parents, item); });
    }

    //메소드 오버로딩 3. 게임 오브젝트로 버튼 이벤트를 등록한다.
    public static void addButtonListener(GameObject obj, Action action)
    {
        if (obj != null && obj.GetComponent<Button>() != null)
        {
            Button btn = obj.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => { action.Invoke(); });
        }
    }

    //메소드 오버로딩 4. 동적 아이템을 위한 게임 오브젝트로 버튼 이벤트를 등록한다.
    public static void addButtonListener(GameObject obj, Action<GameObject> action, GameObject item = null)
    {
        if (obj != null && obj.GetComponent<Button>() != null)
        {
            Button btn = obj.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => { action.Invoke(item); });
        }
    }

    //메소드 오버로딩 5. 부모를 기억해야 하는 동적 아이템을 위한 게임 오브젝트로 버튼 이벤트를 등록한다.
    public static void addButtonListener(GameObject obj, Action<GameObject, GameObject> action, GameObject parents = null, GameObject item = null)
    {
        if (obj != null && obj.GetComponent<Button>() != null)
        {
            Button btn = obj.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => { action.Invoke(parents, item); });
        }
    }

    //메소드 오버로딩 6. 동적 인덱스를 위한 게임 오브젝트로 버튼 이벤트를 등록한다.
    public static void addButtonListener(GameObject obj, Action<long> action, long idx = -1)
    {
        if (obj != null && obj.GetComponent<Button>() != null)
        {
            Button btn = obj.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => { action.Invoke(idx); });
        }
    }

    //동적 아이템을 위한 트리거 이벤트를 등록한다. (포인터 다운, 포인터 업)
    public static void addEventTrigger(EventTrigger aTrigger, EventTriggerType tNum, Action<long> action, long idx = -1)
    {
        bool bAdd = true;
        //기존 이벤트 확인
        foreach (var entry in aTrigger.triggers)
        {
            if (entry.eventID == tNum)
            {
                //remove listener from entry
                entry.callback.RemoveAllListeners();
                entry.callback.AddListener((x) => { action.Invoke(idx); });
                bAdd = false;
            }
        }
        if (bAdd == true)
        {
            //새로 이벤트 트리거 등록
            EventTrigger.Entry tEvent = new EventTrigger.Entry();
            tEvent.eventID = tNum;
            tEvent.callback.AddListener((x) => { action.Invoke(idx); });
            aTrigger.triggers.Add(tEvent);
        }
    }

    public static void addEventTrigger(GameObject obj, EventTriggerType tNum, Action<long> action, long idx = -1)
    {
        if (obj != null && obj.GetComponent<EventTrigger>() != null)
        {
            EventTrigger aTrigger = obj.GetComponent<EventTrigger>();

            bool bAdd = true;
            //기존 이벤트 확인
            foreach (var entry in aTrigger.triggers)
            {
                if (entry.eventID == tNum)
                {
                    //remove listener from entry
                    entry.callback.RemoveAllListeners();
                    entry.callback.AddListener((x) => { action.Invoke(idx); });
                    bAdd = false;
                }
            }
            if (bAdd == true)
            {
                //새로 이벤트 트리거 등록
                EventTrigger.Entry tEvent = new EventTrigger.Entry();
                tEvent.eventID = tNum;
                tEvent.callback.AddListener((x) => { action.Invoke(idx); });
                aTrigger.triggers.Add(tEvent);
            }
        }
    }

    public static void addEventTrigger(GameObject obj, EventTriggerType tNum, Action<long, BaseEventData> action, long idx = -1)
    {
        if (obj != null && obj.GetComponent<EventTrigger>() != null)
        {
            EventTrigger aTrigger = obj.GetComponent<EventTrigger>();

            bool bAdd = true;
            //기존 이벤트 확인
            foreach (var entry in aTrigger.triggers)
            {
                if (entry.eventID == tNum)
                {
                    //remove listener from entry
                    entry.callback.RemoveAllListeners();
                    entry.callback.AddListener((x) => { action.Invoke(idx, x); });
                    bAdd = false;
                }
            }
            if (bAdd == true)
            {
                //새로 이벤트 트리거 등록
                EventTrigger.Entry tEvent = new EventTrigger.Entry();
                tEvent.eventID = tNum;
                tEvent.callback.AddListener((x) => { action.Invoke(idx, x); });
                aTrigger.triggers.Add(tEvent);
            }
        }
    }

    public static void addEventTrigger(GameObject obj, EventTriggerType tNum, Action<float> action, float pos = 0.0f)
    {
        if (obj != null && obj.GetComponent<EventTrigger>() != null)
        {
            EventTrigger aTrigger = obj.GetComponent<EventTrigger>();

            bool bAdd = true;
            //기존 이벤트 확인
            foreach (var entry in aTrigger.triggers)
            {
                if (entry.eventID == tNum)
                {
                    //remove listener from entry
                    entry.callback.RemoveAllListeners();
                    entry.callback.AddListener((x) => { action.Invoke(pos); });
                    bAdd = false;
                }
            }
            if (bAdd == true)
            {
                //새로 이벤트 트리거 등록
                EventTrigger.Entry tEvent = new EventTrigger.Entry();
                tEvent.eventID = tNum;
                tEvent.callback.AddListener((x) => { action.Invoke(pos); });
                aTrigger.triggers.Add(tEvent);
            }
        }
    }

    //스크롤바 밸류 변경
    public static void addScrollbarListener(GameObject obj, Action action)
    {
        if (obj != null && obj.GetComponent<Scrollbar>() != null)
        {
            Scrollbar bar = obj.GetComponent<Scrollbar>();
            bar.onValueChanged.RemoveAllListeners();
            bar.onValueChanged.AddListener((x) => { action.Invoke(); });
        }
    }

    //스크롤바 밸류 변경
    public static void addScrollbarListener(GameObject obj, Action<float> action)
    {
        if (obj != null && obj.GetComponent<Scrollbar>() != null)
        {
            Scrollbar bar = obj.GetComponent<Scrollbar>();
            bar.onValueChanged.RemoveAllListeners();
            bar.onValueChanged.AddListener((x) => { action.Invoke(x); });
        }
    }

    //텍스트 인풋 관련 리스너 등록
    public static void addInputListener(GameObject obj, Action action)
    {
        if (obj != null && obj.GetComponent<InputField>() != null)
        {
            //Debug.Log("맞나?");
            InputField ifObj = obj.GetComponent<InputField>();
            //ifObj.onEndEdit.RemoveAllListeners();
            //ifObj.onEndEdit.AddListener((x) => { action.Invoke(); });
            //ifObj.onValueChanged.RemoveAllListeners();
            //ifObj.onValueChanged.AddListener((x) => { action.Invoke(); });
            ifObj.onValueChanged.RemoveAllListeners();
            ifObj.onValueChanged.AddListener((x) => { action.Invoke(); });
        }
    }

    //오버로딩
    public static void addInputListener(GameObject obj, Action changeAction, Action editAction)
    {
        if (obj != null && obj.GetComponent<InputField>() != null)
        {
            InputField ifObj = obj.GetComponent<InputField>();
            ifObj.onValueChanged.RemoveAllListeners();
            ifObj.onValueChanged.AddListener((x) => { changeAction.Invoke(); });
            ifObj.onEndEdit.AddListener((x) => { editAction.Invoke(); });
        }
    }

    //--------------------------------------------------------------------------------------------------



    //[글씨 뒤에 이미지]================================================================================

    public static float setPosX_TextRightObj(GameObject pText, GameObject pRoot, GameObject lObj, float rightX, float marginX)
    {
        float posX = 0.0f;
        if (pText.GetComponent<TMP_Text>() != null)
        {
            TMP_Text a = pText.GetComponent<TMP_Text>();
            float b = a.preferredWidth;
            RectTransform pRect = pRoot.GetComponent<RectTransform>();
            RectTransform cbRect = lObj.GetComponent<RectTransform>();
            float pWidth = pRect.rect.width - rightX;
            posX = (cbRect.rect.width / 2.0f) + b + marginX;
            if (posX > pWidth - (cbRect.rect.width / 2.0f))
            {
                posX = pWidth - (cbRect.rect.width / 2.0f);
            }
        }
        //TMP_Text a = nItem.transform.Find("T_Name").gameObject.GetComponent<TMP_Text>();
        //float b = a.preferredWidth;
        //RectTransform pRect = nItem.GetComponent<RectTransform>();
        //RectTransform cbRect = nItem.transform.Find("ChangeNameBtn").gameObject.GetComponent<RectTransform>();
        //float pWidth = pRect.rect.width - 120.0f;
        //float posX = (cbRect.rect.width / 2.0f) + b + 150.0f;
        //if (posX > pWidth - (cbRect.rect.width / 2.0f))
        //{
        //    posX = pWidth - (cbRect.rect.width / 2.0f);
        //}
        //cbRect.anchoredPosition3D = new Vector3(posX, cbRect.anchoredPosition3D.y, cbRect.anchoredPosition3D.z);
        return posX;
    }


}

