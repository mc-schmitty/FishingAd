using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FitInSafeArea : MonoBehaviour
{
    [SerializeField]
    private bool enableScaling = true;

    /// <summary>
    /// Scales UI Elements parented under this GameObject within the safe area of most devices
    /// Note: use a width-biased Canvas Scaler
    /// </summary>
    private void Awake()
    {
        if (!enableScaling)
            return;

        Resolution resolution = Screen.currentResolution;
        Rect safeArea = Screen.safeArea;
        float referenceWidth = GetComponentInParent<CanvasScaler>().referenceResolution.x; // ok for my purposes I am assuming the scale reference is 1 in width
                                                                                           //  If this was production code or smth you would obviously need to check way more
        float scaleRatio = referenceWidth / resolution.width;

        float safeAreaTop = resolution.height - safeArea.height - safeArea.y;       // ok so i think rect.max/min might actually be this but uh
        float safeAreaBot = safeArea.y;

        // Also once again i saw no safe areas which had an offset width so im not including it
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;

        rt.offsetMin = new Vector2(0, safeAreaBot * scaleRatio);
        rt.offsetMax = new Vector2(0, -safeAreaTop * scaleRatio);
    }
}
