using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Newtonsoft.Json;
using System.Linq;
using System.Text;


public enum WordInfoField
{
    WORD,
    KR,
    KR_GAMEWORD,
    ENGHINT,
    KORHINT
}

[Serializable]
public class ScriptClass
{
    public int idx { get; set; }
}

[Serializable]
public class LanguageScript : ScriptClass
{
    public string kr { get; set; }
    public string en { get; set; }
    public string jp { get; set; }
}

[Serializable]
public class WordInfo : ScriptClass
{
    public string word { get; set; }
    public string kr { get; set; }
    public string engHint { get; set; }
    public string korHint { get; set; }
    public int tgVal { get; set; }
}

public class LanguageManager : Singleton<LanguageManager>
{
    private LanguageType currentLanguage = LanguageType.KR;
    public Dictionary<int, LanguageScript> languageScriptDic { get; set; } = new Dictionary<int, LanguageScript>();
    public Dictionary<int, WordInfo> wordInfoDic { get; set; } = new Dictionary<int, WordInfo>();

    protected async UniTask OnInitializing()
    {
        await LanguageScriptDicLoad("contents", languageScriptDic);
        await LanguageScriptDicLoad("returnCode", languageScriptDic);
        await LanguageScriptDicLoad("ui", languageScriptDic);
        await LanguageScriptDicLoad("word", wordInfoDic);
    }

    public LanguageType GetCurrentLanguageType()
    {
        return currentLanguage;
    }

    private async UniTask LanguageScriptDicLoad<T>(string jsonName, Dictionary<int, T> saveDic) where T : ScriptClass
    {
        TextAsset _textAsset = await ResourceManager.Instance.LoadAsset<TextAsset>(jsonName);

        var items = JsonConvert.DeserializeObject<List<T>>(_textAsset.text);

        foreach (var item in items)
        {
            if (saveDic.ContainsKey(item.idx)) continue;

            if (item.idx == 92054)
            {
                LanguageScript _lang = item as LanguageScript;
                Debug.Log(_lang.kr);
            }

            saveDic.Add(item.idx, item);
        }
    }

    public void GetLangScript(int idx, Dictionary<int, LanguageScript> scriptDic, Text text, ContentSizeFitter fitter = null, RectTransform rect = null)
    {
        text.text = GetString(idx, scriptDic);
        FitterRefresh(fitter, rect);
    }

    public void GetLangScript(int idx, Dictionary<int, LanguageScript> scriptDic, TextMeshProUGUI tmp, ContentSizeFitter fitter = null, RectTransform rect = null)
    {
        tmp.text = GetString(idx, scriptDic);
        FitterRefresh(fitter, rect);
    }

    private string GetString(int idx, Dictionary<int, LanguageScript> scriptDic)
    {
        try
        {
            string _str = "";

            switch (currentLanguage)
            {
                case LanguageType.KR:
                    _str = scriptDic[idx].kr;
                    break;

                case LanguageType.EN:
                    _str = scriptDic[idx].en;
                    break;

                default:
                    _str = scriptDic[idx].kr;
                    break;
            }

            return _str;
        }

        catch (KeyNotFoundException)
        {
            return "NF";
        }
    }

    public void FitterRefresh(ContentSizeFitter fitter = null, RectTransform rect = null)
    {
        if (fitter != null)
        {
            fitter.SetLayoutHorizontal();
            fitter.SetLayoutVertical();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
    }

    public string StrFormatForLangScript(int idx, Dictionary<int, LanguageScript> scriptDic = null, params string[] str)
    {
        if (scriptDic == null) scriptDic = languageScriptDic;

        string _text = GetString(idx, scriptDic);

        _text = string.Format(_text, str);

        //if (_text != "" || _text != null) _text = _text.Replace(' ', '\u00A0');

        return _text;
    }

    public string ConvertToReadableFormat(long value)
    {
        if (value >= 100000000) // 억 단위 (10^8)
        {
            return $"{(value / 100000000.0):0.##}b";
        }
        else if (value >= 1000000) // 백만 단위 (10^6)
        {
            return $"{(value / 1000000.0):0.##}m";
        }
        else if (value >= 1000) // 천 단위 (10^3)
        {
            return $"{(value / 1000.0):0.##}k";
        }
        else // 천 미만
        {
            return value.ToString();
        }
    }
}

