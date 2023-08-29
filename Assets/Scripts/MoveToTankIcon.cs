using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToTankIcon : MonoBehaviour
{
    // Move Fish to Tank Icon, then disable
    [SerializeField]
    private RectTransform tankIconTransform;
    [SerializeField]
    private GameObject newText;
    [SerializeField]
    private AnimationCurve smoothCurve;
    [SerializeField][Range(0f, 1f)]
    private float minScalePercentage = 0.2f;
    [SerializeField]
    private float timeToMoveFish = 1f;


    public void MoveFishToIcon(Transform fish)
    {
        StartCoroutine(MoveToIcon(fish, tankIconTransform, timeToMoveFish));
    }

    // Smoothly lerp fish to tank icon
    IEnumerator MoveToIcon(Transform fish, RectTransform icon, float timeToComplete)
    {
        //Vector3 rectPos = Camera.main.ScreenToWorldPoint(icon.transform.position);        // Get to and from positions
        Ray r = RectTransformUtility.ScreenPointToRay(Camera.main, icon.position);      // need ray from screen to world
        Vector3 rectPos = r.origin + (r.direction.normalized * 1);                      // move along ray by set amount
        Vector3 ogFishPos = fish.position;

        Vector3 ogFishSize = fish.localScale;                                   // Get to and from scales
        Vector3 minFishSize = ogFishSize * minScalePercentage;

        // Move fish to icon, while shrinking it
        for(float step = 0; step <= 1; step += Time.deltaTime / timeToComplete)
        {
            fish.position = Vector3.Lerp(ogFishPos, rectPos, smoothCurve.Evaluate(step));
            fish.localScale = Vector3.Lerp(ogFishSize, minFishSize, smoothCurve.Evaluate(step));
            yield return null;
        }

        fish.gameObject.SetActive(false);
        newText.SetActive(true);
    }
}
