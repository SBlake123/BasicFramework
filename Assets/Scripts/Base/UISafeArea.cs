using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISafeArea : MonoBehaviour
{
    RectTransform rect;

    int lastScreenWidth = 0;

    int lastScreenHeight = 0;
    private void Awake()
    {
        rect = GetComponent<RectTransform>();

        OnScreenSizeChanged();
    }

#if UNITY_EDITOR
    void Update()
    {
        if (lastScreenWidth != Screen.width || lastScreenHeight != Screen.height)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            OnScreenSizeChanged();
        }
    }
#endif

   

    private void OnScreenSizeChanged()
    {
        rect.sizeDelta = Vector2.zero;
        var safeArea = Screen.safeArea;
        var minAnchor = safeArea.position;
        var maxAnchor = minAnchor + safeArea.size;

        minAnchor.x /= Screen.width;
        minAnchor.y /= Screen.height;
        maxAnchor.x /= Screen.width;
        maxAnchor.y /= Screen.height;

        rect.anchorMin = minAnchor;
        rect.anchorMax = maxAnchor;
    }
}
