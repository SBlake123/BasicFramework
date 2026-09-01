/// [스크립트 명세]-----------------------------------------------------------------------------------------------------------------------
/// 웹서버와 통신하기 위한 라이브러리 모듈
/// --------------------------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using UnityEngine.Networking;


public class WWWHelper : MonoBehaviour
{
    public static IEnumerator coroutine;

    /** 이벤트 연결을 위한 델리게이터 (대기자) */
    public delegate void HttpRequestDelegate(int id, UnityWebRequest www);

    /** 이벤트 핸들러 */
    public event HttpRequestDelegate OnHttpRequest;

    /** 웹 서버로의 요청을 구분하기 위한 ID값 */
    private int requestId;

    /** 이 클래스의 싱글톤 객체 */
    static WWWHelper current = null;

    /** 객체를 생성하기 위한 GameObject */
    static GameObject container = null;

    /** 싱글톤 객체 만들기 */
    public static WWWHelper Instance
    {
        get
        {
            if (current == null)
            {
                container = new GameObject();
                container.name = "WWWHelper";
                current = container.AddComponent(typeof(WWWHelper)) as WWWHelper;
            }
            return current;
        }
    }

    /** HTTP GET 방식 통신 처리 */
    public void get(int id, string url)
    {
        UnityWebRequest www = new UnityWebRequest(url);
        StartCoroutine(WaitForRequest(id, www));
    }

    /** HTTP POST 방식 통신 처리 */
    public void post(int id, string url, IDictionary<string, string> data)
    {
        WWWForm form = new WWWForm();

        foreach (KeyValuePair<string, string> post_arg in data)
        {
            form.AddField(post_arg.Key, post_arg.Value);
        }

        UnityWebRequest www = new UnityWebRequest();
        www = UnityWebRequest.Post(url, form);
        coroutine = WaitForRequest(id, www);
        StartCoroutine(coroutine);
    }

    /** HTTP POST 방식 통신 처리 + 헤더값을 추가 */
    public void post(int id, string url, IDictionary<string, string> data, string headerName, string headerValue)
    {
        WWWForm form = new WWWForm();
        //헤더값 추가
        form.headers.Add(headerName, headerValue);
        foreach (KeyValuePair<string, string> post_arg in data)
        {
            form.AddField(post_arg.Key, post_arg.Value);
        }
        UnityWebRequest www = new UnityWebRequest();
        www = UnityWebRequest.Post(url, form);
        coroutine = WaitForRequest(id, www);
        StartCoroutine(coroutine);
    }

    /** HTTP 통신 처리:직접 만든 리퀘스트를 전송한다 */
    public void sendRequest(int id, UnityWebRequest request)
    {
        //WWWForm form = new WWWForm();
        coroutine = WaitForRequest(id, request);
        StartCoroutine(coroutine);
    }

    //HTTP 통신 처리:직접 만든 리퀘스트를 전송"만" 한다
    public IEnumerator sendRequestOnly(int id, UnityWebRequest request)
    {
        yield return request.SendWebRequest();  // 응답이 올때까지 대기한다.

        if (request.error == null)  // 에러가 나지 않으면 동작.
        {
            Debug.Log(string.Format("Send Only : {0}", request.downloadHandler.text));
        }
        else
        {
            Debug.Log(string.Format("Send Only Error: {0}", request.downloadHandler.text));
        }
        // 통신 해제
        request.Dispose();
    }

    /** 통신 처리를 위한 코루틴 */
    private IEnumerator WaitForRequest(int id, UnityWebRequest www)
    {
        // 응답이 올떄까지 기다림
        yield return www.SendWebRequest();

        // 응답이 왔다면, 이벤트 리스너에 응답 결과 전달
        bool hasCompleteListener = (OnHttpRequest != null);

        if (hasCompleteListener)
        {
            OnHttpRequest(id, www);
        }

        // 통신 해제
        www.Dispose();
    }

}