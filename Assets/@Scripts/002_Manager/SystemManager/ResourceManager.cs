using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceManager : Singleton<ResourceManager>
{
    List<string> labelNameList = new List<string> { "Sound", "Text", "Sprite" };

    public int loadCount { get; set; } = 0;

    public int totalCount { get; set; } = 0;

    long loadSize { get; set; } = 0L;
    public string loadPercent { get; set; } = "";
    long downloadSize { get; set; } = 0L;
    long minDownloadSize { get; set; } = 1L;

    //protected override async UniTask OnInitializing()
    //{
    //    await base.OnInitializing();
    //}

    //protected override async UniTask OnInitialized()
    //{
    //    await base.OnInitialized();
    //}

    UnityEngine.Object obj;

    bool isAssetLoaded = false;

    ////private AsyncOperationHandle<List<string>> handle;
    //public async UniTask LoadAssetCheck()
    //{
    //    bool _downloadComplete = false;

    //    await Addressables.InitializeAsync();

    //    string _downSizeStr = "";

    //    // await UniTask.WaitUntil(() => _getDownloadSizeDone == true);

    //    string[] labels = { "Sprite", "Text", "Sound", "GameObject" };

    //    Addressables.CheckForCatalogUpdates().Completed += async handle =>
    //    {
    //        if (handle.Result.Count > 0)
    //        {
    //            await Addressables.UpdateCatalogs();

    //            string resolved = AddressablesRuntimeProperties.EvaluateString("{RemoteLoadPath}");
    //            Debug.Log(" ½ÇÁ¦ RemoteLoadPath: " + resolved);

    //            bool _getDownloadSizeDone = false;

    //            Addressables.InitializeAsync().Completed += async handle =>
    //            {
    //                await getDownloadSize(handle);
    //                _getDownloadSizeDone = true;
    //            };

    //            await UniTask.WaitUntil(() => _getDownloadSizeDone == true);

    //            async UniTask getDownloadSize(AsyncOperationHandle<IResourceLocator> obj)
    //            {
    //                if (obj.Status == AsyncOperationStatus.Succeeded)
    //                {
    //                    foreach (var locator in Addressables.ResourceLocators)
    //                    {
    //                        if (locator.Keys != null)
    //                        {
    //                            Addressables.GetDownloadSizeAsync(locator.Keys).Completed += async handle =>
    //                            {
    //                                if (handle.Status == AsyncOperationStatus.Succeeded)
    //                                {
    //                                    downloadSize += handle.Result;
    //                                }
    //                                else
    //                                {

    //                                }

    //                                await UniTask.WaitForSeconds(1f);
    //                            };
    //                        }
    //                    }
    //                }

    //                await UniTask.WaitForSeconds(1f);
    //            }

    //            float _randomFloat = UnityEngine.Random.Range(0, 9) / 10f;

    //            _downSizeStr = $"{((downloadSize / (1024 * 1024)) + _randomFloat):F1}MB";

    //            gPopUpManager.setPopUpCode(false, LanguageManager.Instance.GetLangScript(90013, LanguageManager.Instance.languageScriptDic), "", "", LanguageManager.Instance.StrFormatForLangScript(99003, null, _downSizeStr.ToString()));
    //            gPopUpManager.AddMethodToBtn(async () =>
    //            {
    //                long downloadBytes = 0L;
    //                foreach (var item in labels)
    //                {
    //                    var downLoadSize = await Addressables.GetDownloadSizeAsync(item);

    //                    if (downLoadSize > 0)
    //                    {
    //                        var downloadHandle = Addressables.DownloadDependenciesAsync(item);

    //                        Debug.Log($"downloadSize : {downloadSize}");

    //                        while (!downloadHandle.IsDone)
    //                        {
    //                            var downStatus = downloadHandle.GetDownloadStatus();

    //                            loadSize = downloadBytes + downStatus.DownloadedBytes;
    //                            loadPercent = $"{((float)loadSize / downloadSize * 100f):F0}%";
    //                            if (((float)loadSize / downloadSize) < 0.06f)
    //                            {
    //                                GameManager.Instance.loadingGaugeValue = 0f;
    //                            }
    //                            else
    //                            {
    //                                GameManager.Instance.loadingGaugeValue = ((float)loadSize / downloadSize);

    //                            }
    //                            //Debug.Log($"{downStatus.DownloadedBytes}/{downStatus.TotalBytes}");
    //                            //Debug.Log($"{downStatus.Percent * 100f} %");

    //                            await UniTask.Yield();
    //                        }

    //                        if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
    //                        {
    //                            downloadBytes += downloadHandle.GetDownloadStatus().TotalBytes;
    //                        }

    //                        else
    //                        {

    //                        }

    //                        Addressables.Release(downloadHandle);
    //                    }
    //                }

    //                _downloadComplete = true;

    //            });
    //        }

    //        else
    //        {
    //            bool _getDownloadSizeDone = false;

    //            Addressables.InitializeAsync().Completed += async handle =>
    //            {
    //                await getDownloadSize(handle);
    //                _getDownloadSizeDone = true;
    //            };

    //            await UniTask.WaitUntil(() => _getDownloadSizeDone == true);

    //            async UniTask getDownloadSize(AsyncOperationHandle<IResourceLocator> obj)
    //            {
    //                if (obj.Status == AsyncOperationStatus.Succeeded)
    //                {
    //                    foreach (var locator in Addressables.ResourceLocators)
    //                    {
    //                        if (locator.Keys != null)
    //                        {
    //                            Addressables.GetDownloadSizeAsync(locator.Keys).Completed += async handle =>
    //                            {
    //                                if (handle.Status == AsyncOperationStatus.Succeeded)
    //                                {
    //                                    downloadSize += handle.Result;
    //                                }
    //                                else
    //                                {

    //                                }

    //                                await UniTask.WaitForSeconds(1f);
    //                            };
    //                        }
    //                    }
    //                }

    //                await UniTask.WaitForSeconds(1f);
    //            }

    //            float _randomFloat = UnityEngine.Random.Range(0, 9) / 10f;
    //            _downSizeStr = $"{((downloadSize / (1024 * 1024)) + _randomFloat):F1}MB";


    //            if (downloadSize > 0)
    //            {
    //                gPopUpManager.setPopUpCode(false, LanguageManager.Instance.GetLangScript(90013, LanguageManager.Instance.languageScriptDic), "", "", LanguageManager.Instance.StrFormatForLangScript(99003, null, _downSizeStr.ToString()));
    //                gPopUpManager.AddMethodToBtn(async () =>
    //                {
    //                    long downloadBytes = 0L;
    //                    foreach (var item in labels)
    //                    {
    //                        var downLoadSize = await Addressables.GetDownloadSizeAsync(item);

    //                        if (downLoadSize > 0)
    //                        {
    //                            var downloadHandle = Addressables.DownloadDependenciesAsync(item);

    //                            Debug.Log($"downloadSize : {downloadSize}");

    //                            while (!downloadHandle.IsDone)
    //                            {
    //                                var downStatus = downloadHandle.GetDownloadStatus();

    //                                loadSize = downloadBytes + downStatus.DownloadedBytes;
    //                                loadPercent = $"{((float)loadSize / downloadSize * 100f):F0}%";
    //                                if (((float)loadSize / downloadSize) < 0.06f)
    //                                {
    //                                    GameManager.Instance.loadingGaugeValue = 0f;
    //                                }
    //                                else
    //                                {
    //                                    GameManager.Instance.loadingGaugeValue = ((float)loadSize / downloadSize);

    //                                }

    //                                await UniTask.Yield();
    //                            }

    //                            if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
    //                            {
    //                                downloadBytes += downloadHandle.GetDownloadStatus().TotalBytes;
    //                            }

    //                            else
    //                            {

    //                            }

    //                            Addressables.Release(downloadHandle);
    //                        }
    //                    }

    //                    _downloadComplete = true;

    //                });
    //            }
    //            else
    //            {
    //                _downloadComplete = true;
    //            }
    //        }
    //    };




    //    await UniTask.WaitUntil(() => _downloadComplete == true);


    //    //Addressables.CheckForCatalogUpdates(true).Completed += async handle =>
    //    //{
    //    //    if (handle.Result.Count > 0)
    //    //    {
    //    //        await Addressables.UpdateCatalogs();

    //    //        bool _getDownloadSizeDone = false;

    //    //        Addressables.InitializeAsync().Completed += async handle =>
    //    //        {
    //    //            await getDownloadSize(handle);
    //    //            _getDownloadSizeDone = true;
    //    //        };

    //    //        await UniTask.WaitUntil(() => _getDownloadSizeDone == true);

    //    //        async UniTask getDownloadSize(AsyncOperationHandle<IResourceLocator> obj)
    //    //        {
    //    //            if (obj.Status == AsyncOperationStatus.Succeeded)
    //    //            {
    //    //                foreach (var locator in Addressables.ResourceLocators)
    //    //                {
    //    //                    if (locator.Keys != null)
    //    //                    {
    //    //                        Addressables.GetDownloadSizeAsync(locator.Keys).Completed += async handle =>
    //    //                        {
    //    //                            if (handle.Status == AsyncOperationStatus.Succeeded)
    //    //                            {
    //    //                                downloadSize += handle.Result;
    //    //                            }
    //    //                            else
    //    //                            {

    //    //                            }

    //    //                            await UniTask.WaitForSeconds(1f);
    //    //                        };
    //    //                    }
    //    //                }
    //    //            }

    //    //            await UniTask.WaitForSeconds(1f);
    //    //        }

    //    //        string _downSizeStr = $"{downloadSize / (1024 * 1024)}MB";

    //    //        gPopUpManager.setPopUpCode(false, LanguageManager.Instance.GetLangScript(90013, LanguageManager.Instance.languageScriptDic), "", "", LanguageManager.Instance.StrFormatForLangScript(99003, null, _downSizeStr));
    //    //        gPopUpManager.AddMethodToBtn(() =>
    //    //        {
    //    //            gPopUpManager.setPopUpClose();

    //    //            Addressables.InitializeAsync().Completed += async handle =>
    //    //            {
    //    //                await InitializationDone(handle);
    //    //            };

    //    //            async UniTask InitializationDone(AsyncOperationHandle<IResourceLocator> obj)
    //    //            {
    //    //                if (obj.Status == AsyncOperationStatus.Succeeded)
    //    //                {
    //    //                    foreach (var locator in Addressables.ResourceLocators)
    //    //                    {
    //    //                        foreach (string key in locator.Keys)
    //    //                        {
    //    //                            Addressables.GetDownloadSizeAsync(key).Completed += handle =>
    //    //                            {
    //    //                                if (handle.Status == AsyncOperationStatus.Succeeded) downloadSize += handle.Result;
    //    //                            };

    //    //                            totalCount++;
    //    //                        }
    //    //                    }

    //    //                    foreach (var locator in Addressables.ResourceLocators)
    //    //                    {
    //    //                        foreach (string key in locator.Keys)
    //    //                        {
    //    //                            try
    //    //                            {
    //    //                                var handle = Addressables.DownloadDependenciesAsync(key);

    //    //                                while (!handle.IsDone)
    //    //                                {
    //    //                                    var downStatus = handle.GetDownloadStatus();
    //    //                                    Debug.Log($"{downStatus.DownloadedBytes}/{downStatus.TotalBytes}");
    //    //                                    Debug.Log($"{downStatus.Percent * 100f} %");

    //    //                                    await UniTask.Yield();
    //    //                                }

    //    //                                Addressables.Release(handle);
    //    //                            }

    //    //                            catch
    //    //                            {
    //    //                                Debug.Log($"FailKey = {key}");
    //    //                                Debug.LogError("Status is failed");
    //    //                                loadCount++;

    //    //                                callback?.Invoke(loadCount, totalCount);
    //    //                            }                                   
    //    //                        }

    //    //                    }
    //    //                }

    //    //                await UniTask.WaitForSeconds(1f);
    //    //            }
    //    //        });
    //    //    }       
    //    //};
    //    await UniTask.WaitForSeconds(1f);
    //}

    private string lastKey = "";

    public async UniTask<T> LoadAsset<T>(string key, Transform parent = null, Vector3? position = null, Quaternion? rotation = null) where T : UnityEngine.Object
    {
        key = RefineKey();

        isAssetLoaded = false;

        if (key == "") return null;

        lastKey = key;

        Addressables.LoadAssetAsync<T>(key).Completed += OnAssetLoaded;

        await UniTask.WaitUntil(() => isAssetLoaded);

        if (obj is GameObject _obj && typeof(T) == typeof(GameObject))
        {
            SetGameObjectProperties(_obj, position, rotation, parent);
        }

        try
        {
            return (T)obj;
        }
        catch (InvalidCastException)
        {
            Debug.Log(key);
            return null;
        }

        string RefineKey()
        {
            string _key = "";

            switch (typeof(T))
            {
                case Type type when type == typeof(GameObject):
                    //Debug.Log(typeof(T).ToString());
                    _key = string.Format(GScriptAddress.gameObjectAddress, key);
                    break;

                case Type type when type == typeof(Sprite):
                    //Debug.Log(typeof(T).ToString());
                    _key = string.Format(GScriptAddress.spriteAddress, key);
                    break;

                case Type type when type == typeof(TextAsset):
                    _key = string.Format(GScriptAddress.langScriptAddress, key);
                    break;

                case Type type when type == typeof(AudioClip):
                    {
                        int _idx = -1;
                        bool _keyExists;

                        for (int i = 0; i < GScriptAddress.soundNameExtensions.Length; i++)
                        {
                            _key = string.Format(GScriptAddress.soundAddress + GScriptAddress.soundNameExtensions[i], key);
                            _keyExists = Addressables.ResourceLocators.Any(locator => locator.Keys.Contains(_key));

                            if (_keyExists)
                            {
                                _idx = i;
                                break;
                            }
                        }

                        if (_idx == -1)
                        {
                            _key = "";
                            break;
                        }

                        _key = string.Format(GScriptAddress.soundAddress + GScriptAddress.soundNameExtensions[_idx], key);
                    }
                    break;

                default:
                    _key = "default";
                    break;
            }

            return _key;
            //string _key = "";

            //if(typeof(T) == typeof(Sprite)) _key = string.Format(GScriptAddress.spriteAddress, key);
            //else if (typeof(T) == typeof(TextAsset)) _key = string.Format(GScriptAddress.langScriptAddress, key);

            //return _key;
        }

        void SetGameObjectProperties(GameObject gameObject, Vector3? position, Quaternion? rotation, Transform parent)
        {
            gameObject.transform.position = position ?? Vector3.zero;
            gameObject.transform.rotation = rotation ?? Quaternion.Euler(Vector3.zero);

            if (parent != null)
            {
                gameObject.transform.SetParent(parent);
            }
        }
    }

    public void OnAssetLoaded<T>(AsyncOperationHandle<T> handle) where T : UnityEngine.Object
    {
        try
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                obj = handle.Result;

                isAssetLoaded = true;
            }

            else
            {
                if (handle.OperationException != null)
                {
                    switch (handle.OperationException.GetType())
                    {
                        case Type type when type == typeof(InvalidKeyException):
                            {
                                Debug.Log($"KeyException! : {lastKey}");

                                obj = null;

                                isAssetLoaded = true;
                            }
                            break;

                        default:
                            {
                                obj = null;

                                isAssetLoaded = true;
                            }
                            break;
                    }
                }
            }
        }
        catch (InvalidKeyException)
        {
            Debug.Log($"KeyException! : {lastKey}");

            obj = null;

            isAssetLoaded = true;
        }

    }

    public void ReleaseAsset<T>(T obj) where T : UnityEngine.Object
    {
        Addressables.Release(obj);
    }
}
