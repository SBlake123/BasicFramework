/// [스크립트 명세]-----------------------------------------------------------------------------------------------------------------------
///  웹에서 로테이션을 처리하는 매니저
///  ※ 사파리에서 문제가 있어 현재는 사용하지 않는다. 그지같은 사파리...
///  로고 씬에서 GameObject 컴포넌트에 등록
/// --------------------------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LitJson;
using System;

public class gRotateManager : MonoBehaviour
{
    //private bool bRotate = false;
    private bool bAwake = false;

    // Awake is called before the first frame update
    void Awake()
    {
        var obj = FindObjectsOfType<gRotateManager>();
        if (obj.Length <= 1)
        {
            gBase.setEnKey();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        //bRotate = false;
        bAwake = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (bAwake == true)
        {
            //if (Application.platform != RuntimePlatform.OSXPlayer)
            //{
            if (Screen.fullScreen == true)
            {
                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    if (Screen.autorotateToPortrait == false)
                    {
                        Screen.autorotateToPortrait = true;
                        Screen.autorotateToPortraitUpsideDown = true;
                    }
                }
                if (Screen.width * 1.1f < Screen.height)
                {
                    //if (bRotate == false)
                    if (Screen.autorotateToPortrait != false || Screen.orientation != ScreenOrientation.AutoRotation)
                    {
                        Screen.orientation = ScreenOrientation.AutoRotation;
                        Screen.autorotateToPortrait = false;
                        Screen.autorotateToPortraitUpsideDown = false;
                        Screen.autorotateToLandscapeLeft = true;
                        Screen.autorotateToLandscapeRight = true;
                        //bRotate = true;
                        //Debug.Log(string.Format("????"));
                    }
                }
            }
            else
            {
                //bRotate = false;
                if (Screen.autorotateToPortrait == false)
                {
                    Screen.autorotateToPortrait = true;
                    Screen.autorotateToPortraitUpsideDown = true;
                }
                }
            //}
        }
    }

    void OnApplicationFocus(bool focus)
    {
        //if (Application.platform != RuntimePlatform.OSXPlayer)
        //{
        if (focus == false && Screen.fullScreen == true)
        {
            if (Screen.autorotateToPortrait == false)
            {
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = true;
            }
        }
        //}
        //bRotate = false;
    }



}
