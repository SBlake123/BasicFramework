/// [스크립트 명세]------------------------------------------------------------------------------------------------------------------------
/// 1. 정의 : 등록된 소팅레이어를 체크 및 적용합니다.
/// --------------------------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using LitJson;
using System;
using UnityEngine.UI;
using UnityEditor;
using System.Reflection;


public class sortingLayerController
{

    //배열 번호로 소팅레이어 이름을 알아내는 함수
    public static string getLayerName(int count)
    {
        var layers = SortingLayer.layers;

        string reStr = "";
        if(layers.Length <= count)
        {
            reStr = layers[count - 1].name;
        }
        else
        {
            reStr = layers[count].name;
        }
        return reStr;
    }

    public static void showSortingLayerNames()
    {
        var layers = SortingLayer.layers;
        for (int i = 0; i < layers.Length; i++)
        {
            Debug.Log(layers[i].name);
        }
    }
}
