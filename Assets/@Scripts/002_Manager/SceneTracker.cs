using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTracker : Singleton<SceneTracker>
{
    public string currentScene { get; private set; }
    public string previousScene { get; private set; }

    public void SceneNameSave(string sceneName)
    {
        previousScene = SceneManager.GetActiveScene().name;
        currentScene = sceneName;
    }
}
