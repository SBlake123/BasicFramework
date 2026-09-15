using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PooledObject : MonoBehaviour
{
    // Start is called before the first frame update
    public string name { get; set; }
    public GameObject prefab { get; set; }
    public GameObject poolParent { get; set; }

    public int count { get; set; }

    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

}
