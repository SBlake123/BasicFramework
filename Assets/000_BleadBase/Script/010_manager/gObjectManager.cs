/// [스크립트 명세]------------------------------------------------------------------------------------------------------------------------
/// ※ 정의 : 게임 내 오브젝트를 관리하는 매니저이다.
/// --------------------------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LitJson;
using System;
using UnityEngine.Rendering;
using System.IO;


public class gObjectManager : MonoBehaviour
{
    //[MonoBehaviour 메소드]========================================================================================

    private static GameObject pool;             //오브젝트를 넣어두는 곳이다
    private static GameObject recyclingBin;     //재활용 가능한 오브젝트들을 넣어두는 곳이다. 

    public static bool bActivate = false;
    public static bool bStart = false;
    public static bool bSuccess = false;

    //초기화
    void Awake()
    {
        var obj = FindObjectsOfType<gObjectManager>();
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

    //상태에 따른 요청 처리
    void Update()
    {
        if(bActivate == true && bStart == false)
        {
            startPool();
        }
    }

    //--------------------------------------------------------------------------------------------------------------



    //[인잇]========================================================================================================

    void initManager()
    {
        bSuccess = false;
        bActivate = false;
        bStart = false;
        if (this.transform.Find("Pool") != null)
        {
            pool = this.transform.Find("Pool").gameObject;
            recyclingBin = this.transform.Find("RecyclingBin").gameObject;
            //refreshPool();
        }
    }

    public static void activatePool()
    {
        bActivate = true;
        bStart = false;
        bSuccess = false;
    }

    void startPool()
    {
        bStart = true;
        Debug.Log("Start Pooling");
        StartCoroutine("refreshPool");
    }

    //풀에 예약된 모든 게임오브젝트를 올린다.
    IEnumerator refreshPool()
    {
        if (pool != null)
        {
            UnityEngine.Object[] iObj = Resources.LoadAll("100_Prefabs/010_Inven");
            UnityEngine.Object[] uiObj = Resources.LoadAll("100_Prefabs/100_Ui");
            setPoolAll(iObj);
            setPoolAll(uiObj);
        }
        bSuccess = true;
        Debug.Log("End Pooling");
        yield return null;
    }

    //풀에 모든 오브젝트를 올린다.
    public static void setPoolAll(UnityEngine.Object[] obj)
    {
        if (pool != null)
        {
            for (int i = 0; i < obj.Length; i++)
            {
                if (pool.transform.Find(obj[i].name) == null)
                {
                    GameObject nItem = Instantiate(obj[i]) as GameObject;
                    nItem.SetActive(false);
                    nItem.name = obj[i].name;
                    nItem.transform.SetParent(pool.transform);
                    nItem.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                    nItem.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                }
            }
        }
    }

    //풀에 오브젝트 하나를 올린다.
    public static void setPoolObj(string objRoot, string objName)
    {
        GameObject nItem = Instantiate(Resources.Load<GameObject>(string.Format("{0}/{1}", objRoot, objName)));
        if (pool != null && nItem != null)
        {
            nItem.SetActive(false);
            nItem.name = objName;
            nItem.transform.SetParent(pool.transform);
            nItem.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            nItem.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            Debug.Log(string.Format("Object {0} Load", objName));
        }
    }

    //풀에서 아이템을 가져온다.
    public static GameObject getObjToPool(string objRoot, string objName, GameObject oParents)
    {
        //bool reVal = false;
        GameObject nItem = null;
        if (pool != null)
        {
            if (pool.transform.Find(objName) == null)
            {
                //풀에 해당 오브젝트가 없으면 로드해서 풀에 넣는다.
                setPoolObj(objRoot, objName);
            }
            if (pool.transform.Find(objName) != null)
            {
                nItem = pool.transform.Find(objName).gameObject;
                nItem.transform.SetParent(oParents.transform);
            }
            else
            {
                Debug.Log("Object Not Find");
            }
        }
        //return reVal;
        return nItem;
    }

    //아이템을 풀로 되돌린다.
    public static void returnObjPool(GameObject obj)
    {
        //bool reVal = false;
        if (pool != null)
        {
            obj.transform.SetParent(pool.transform);
            obj.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
    }

    //풀을 정리한다.
    void clearPool()
    {
        //기존의 아이템을 삭제한다
        if (pool != null)
        {
            foreach (Transform child in pool.transform)
            {
                Destroy(child.gameObject);
            }
            StartCoroutine("refreshPool");
        }
    }

    //--------------------------------------------------------------------------------------------------------------



    //[동적 생성 오브젝트 재활용]===================================================================================

    //재활용 쓰레기통에서 오브젝트를 찾아서 로드한다
    public static GameObject getObjToRecyclingBin(string objName)
    {
        GameObject nItem = null;
        if (recyclingBin != null)
        {
            if (recyclingBin.transform.Find(objName) != null)
            {
                nItem = recyclingBin.transform.Find(objName).gameObject;
                //nItem.transform.SetParent(oParents.transform);
            }
        }
        return nItem;
    }

    //해당 오브젝트를 재활용 쓰레기통에 넣는다.
    public static void setObjRecyclingBin(GameObject obj)
    {
        if (recyclingBin != null)
        {
            obj.transform.SetParent(recyclingBin.transform);
            obj.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
    }




    //--------------------------------------------------------------------------------------------------------------


}


