using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LitJson;
using System;
using Cysharp.Threading.Tasks;
using TMPro;
public class SoundDisk : MonoBehaviour
{
    public AudioSource audioSource;
    public SoundCategory category { get; set; }
}