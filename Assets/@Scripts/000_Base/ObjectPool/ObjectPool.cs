using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoSingleton<ObjectPool>
{
    public Dictionary<string, GameObject> pooledObjInfoDic = new Dictionary<string, GameObject>();

    public Dictionary<string, Stack<PooledObject>> objectDictionary = new Dictionary<string, Stack<PooledObject>>();
    int pooledObjDefaultCount { get; set; } = 1;

    //void MakeObjectDic()
    //{
    //    for (int i = 0; i < objectList.Count; i++)
    //    {
    //        string _objName = objectList[i].name;

    //        pooledObjInfoDic.Add(_objName, objectList[i]);
    //        objectDictionary.Add(_objName, new Stack<PooledObject>());

    //        if (pooledObjInfoDic[_objName].TryGetComponent(out PooledObject _pooledObject))
    //            MakeObjectsIntoDicStack(_objName, _pooledObject);
    //    }      
    //}

    public void MakeObjectsIntoDicStack(string _key, PooledObject _pooledObject)
    {
        //if (objectDictionary.ContainsKey(_key))
        //{
        if (_pooledObject.count == 0) _pooledObject.count = pooledObjDefaultCount;

        for (int i = 0; i < _pooledObject.count; i++)
        {
            GameObject _gameObject = Instantiate(_pooledObject.gameObject);

            if (_gameObject.TryGetComponent(out PooledObject _instantPooledObj))
            {
                objectDictionary[_key].Push(_instantPooledObj);
                _gameObject.name = _key;
                _instantPooledObj.name = _key;
                _instantPooledObj.prefab = _gameObject;
                _instantPooledObj.poolParent = gameObject;
            }

            _gameObject.transform.SetParent(transform);
            _gameObject.SetActive(false);
        }
    }

    public async UniTask<GameObject> PopFromPool(string _objName)
    {
        //DebugX.Log($"PopFromPool name : {_objName}");
        return await PopFromPool(_objName, null);
    }

    public async UniTask<GameObject> PopFromPool(string _objName, Transform parent = null, bool isActive = true)
    {
        Debug.Log($"_objName: {_objName}");

        GameObject _obj;

        if (objectDictionary.ContainsKey(_objName) == false)
        {
            objectDictionary.Add(_objName, new Stack<PooledObject>());
        }

        if (objectDictionary[_objName].Count > 0)
        {
            PooledObject _pooledObj = objectDictionary[_objName].Pop();

            _obj = _pooledObj.gameObject;
            _obj.SetActive(isActive);

            //if (_obj.activeSelf)
            //{
            //    Time.timeScale = 0f;
            //    Debug.Log($"Obj Name : {_obj.name}");
            //}


            if (parent != null) _obj.transform.SetParent(parent);
            _obj.transform.localPosition = Vector3.zero;

            return _obj;
        }
        else
        {
            _obj = Instantiate(await ResourceManager.Instance.LoadAsset<GameObject>(_objName));

            if (_obj.TryGetComponent(out PooledObject _instantPooledObj))
            {
                _obj.name = _objName;
                _instantPooledObj.name = _objName;
                _instantPooledObj.prefab = _obj;
                _instantPooledObj.poolParent = gameObject;
            }
            else
            {
                PooledObject _instantPooledObjGetCom = _obj.GetComponent<PooledObject>();
                _obj.name = _objName;
                _instantPooledObjGetCom.name = _objName;
                _instantPooledObjGetCom.prefab = _obj;
                _instantPooledObjGetCom.poolParent = gameObject;
            }

            //_obj = objectDictionary[_objName].Pop().gameObject;
            //_obj.SetActive(true);
            if (parent != null) _obj.transform.SetParent(parent);

            _obj.transform.localPosition = Vector3.zero;

            return _obj;
        }

        //if (objectDictionary.ContainsKey(_objName))
        //{
        //    if (objectDictionary[_objName].Count > 0)
        //    {
        //        PooledObject _pooledObj = objectDictionary[_objName].Pop();

        //        _obj = _pooledObj.gameObject;
        //        //_obj.SetActive(true);

        //        if (_obj.activeSelf)
        //        {
        //            Time.timeScale = 0f;
        //            Debug.Log($"Obj Name : {_obj.name}");
        //        }


        //        if (parent != null) _obj.transform.SetParent(parent);

        //        return _obj;
        //    }
        //    else
        //    {
        //        _obj = Instantiate(await ResourceManager.Instance.LoadAsset<GameObject>(_objName));

        //        if (_obj.TryGetComponent(out PooledObject _instantPooledObj))
        //        {
        //            objectDictionary[_objName].Push(_instantPooledObj);
        //            _obj.name = _objName;
        //            _instantPooledObj.name = _objName;
        //            _instantPooledObj.prefab = _obj;
        //            _instantPooledObj.poolParent = gameObject;
        //        }
        //        else
        //        {
        //            PooledObject _instantPooledObjGetCom = _obj.GetComponent<PooledObject>();
        //            _obj.name = _objName;
        //            _instantPooledObjGetCom.name = _objName;
        //            _instantPooledObjGetCom.prefab = _obj;
        //            _instantPooledObjGetCom.poolParent = gameObject;
        //        }

        //        //_obj = objectDictionary[_objName].Pop().gameObject;
        //        //_obj.SetActive(true);
        //        if (parent != null) _obj.transform.SetParent(parent);

        //        return _obj;
        //    }
        //}

        //else
        //{
        //    return null;
        //}
    }

    public async UniTask<GameObject> PopFromPool(string _objName, RectTransform parent = null, bool isActive = true)
    {
        GameObject _obj;

        if (objectDictionary.ContainsKey(_objName) == false)
        {
            objectDictionary.Add(_objName, new Stack<PooledObject>());
        }

        if (objectDictionary[_objName].Count > 0)
        {
            PooledObject _pooledObj = objectDictionary[_objName].Pop();

            _obj = _pooledObj.gameObject;
            _obj.SetActive(isActive);

            //if (_obj.activeSelf)
            //{
            //    Time.timeScale = 0f;
            //    Debug.Log($"Obj Name : {_obj.name}");
            //}


            if (parent != null) _obj.transform.SetParent(parent);
            _obj.transform.localPosition = Vector3.zero;

            return _obj;
        }
        else
        {
            _obj = Instantiate(await ResourceManager.Instance.LoadAsset<GameObject>(_objName));

            if (_obj.TryGetComponent(out PooledObject _instantPooledObj))
            {
                _obj.name = _objName;
                _instantPooledObj.name = _objName;
                _instantPooledObj.prefab = _obj;
                _instantPooledObj.poolParent = gameObject;
            }
            else
            {
                PooledObject _instantPooledObjGetCom = _obj.GetComponent<PooledObject>();
                _obj.name = _objName;
                _instantPooledObjGetCom.name = _objName;
                _instantPooledObjGetCom.prefab = _obj;
                _instantPooledObjGetCom.poolParent = gameObject;
            }

            //_obj = objectDictionary[_objName].Pop().gameObject;
            //_obj.SetActive(true);
            if (parent != null) _obj.transform.SetParent(parent);

            _obj.transform.localPosition = Vector3.zero;

            return _obj;
        }


        //if (objectDictionary.ContainsKey(_objName))
        //{
        //    if (objectDictionary[_objName].Count > 0)
        //    {
        //        PooledObject _pooledObj = objectDictionary[_objName].Pop();

        //        _obj = _pooledObj.gameObject;
        //        //_obj.SetActive(true);

        //        if (_obj.activeSelf)
        //        {
        //            Time.timeScale = 0f;
        //            Debug.Log($"Obj Name : {_obj.name}");
        //        }


        //        if (parent != null) _obj.transform.SetParent(parent);

        //        return _obj;
        //    }
        //    else
        //    {
        //        _obj = Instantiate(await ResourceManager.Instance.LoadAsset<GameObject>(_objName));

        //        if (_obj.TryGetComponent(out PooledObject _instantPooledObj))
        //        {
        //            objectDictionary[_objName].Push(_instantPooledObj);
        //            _obj.name = _objName;
        //            _instantPooledObj.name = _objName;
        //            _instantPooledObj.prefab = _obj;
        //            _instantPooledObj.poolParent = gameObject;
        //        }
        //        else
        //        {
        //            PooledObject _instantPooledObjGetCom = _obj.GetComponent<PooledObject>();
        //            _obj.name = _objName;
        //            _instantPooledObjGetCom.name = _objName;
        //            _instantPooledObjGetCom.prefab = _obj;
        //            _instantPooledObjGetCom.poolParent = gameObject;
        //        }

        //        //_obj = objectDictionary[_objName].Pop().gameObject;
        //        //_obj.SetActive(true);
        //        if (parent != null) _obj.transform.SetParent(parent);

        //        return _obj;
        //    }
        //}

        //else
        //{
        //    return null;
        //}
    }

    public void PushToPool(GameObject _gameObj)
    {
        if (!_gameObj.activeSelf) return;

        if (_gameObj.TryGetComponent(out PooledObject _pooledObj))
            objectDictionary[_pooledObj.name].Push(_pooledObj);
        else
        {
            PooledObject _pooledObjGetCom = _gameObj.GetComponent<PooledObject>();
            objectDictionary[_pooledObjGetCom.name].Push(_pooledObjGetCom);
        }

        //if (_gameObj.name.Contains("Pet")) DebugX.Log($"{_gameObj.name} Count = {objectDictionary[_pooledObj.name].Count}");

        _gameObj.SetActive(false);
        _gameObj.transform.SetParent(this.transform);
    }
}
