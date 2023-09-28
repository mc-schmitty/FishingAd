using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    //[SerializeField]
    //private float timeToMoveFish = 1f;
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private Image fishImg;

    private void OnEnable()
    {
        FishingRod.FishCaught += MoveFishToIcon;
    }

    private void OnDisable()
    {
        FishingRod.FishCaught -= MoveFishToIcon;
    }

    [Obsolete("Method now uses FishingRod.FishCaught delegate")]
    public void MoveFishToIcon(Transform fish)
    {
        StartCoroutine(MoveToIcon(fish, tankIconTransform, TimingInfo.FishReturnSeconds, false));
    }

    private void MoveFishToIcon(Fish fish)
    {
        StartCoroutine(MoveToIcon(fish.transform, tankIconTransform, TimingInfo.FishReturnSeconds, fish.Bounty > 0));
    }

    // Smoothly lerp fish to tank icon
    IEnumerator MoveToIcon(Transform fish, RectTransform icon, float timeToComplete, bool bounty)
    {
        yield return new WaitForSeconds(TimingInfo.FishPulledSeconds + TimingInfo.FishLingerSeconds + (bounty ? TimingInfo.FishLingerBountyBonusSeconds : 0));  // Wait until fish is finished showing up

        StartCoroutine(FishEnterAnimation(fish));

        //Vector3 rectPos = Camera.main.ScreenToWorldPoint(icon.transform.position);        // Get to and from positions
        Ray r = RectTransformUtility.ScreenPointToRay(Camera.main, icon.position);      // need ray from screen to world
        Vector3 rectPos = r.origin + (r.direction.normalized * 1);                      // move along ray by set amount
        Vector3 ogFishPos = fish.position;

        Quaternion rotOrigin = fish.rotation;
        Quaternion rotFlipped = Quaternion.AngleAxis(180f, Vector3.forward) * rotOrigin;        // Quaternions kinda scare me sometimes

        Vector3 ogFishSize = fish.localScale;                                   // Get to and from scales
        Vector3 minFishSize = ogFishSize * minScalePercentage;

        // Move fish to icon, while shrinking it
        for(float step = 0; step <= 1; step += Time.deltaTime / timeToComplete)
        {
            fish.position = Vector3.Lerp(ogFishPos, rectPos, smoothCurve.Evaluate(step));
            fish.rotation = Quaternion.Lerp(rotOrigin, rotFlipped, smoothCurve.Evaluate(step));
            fish.localScale = Vector3.Lerp(ogFishSize, minFishSize, smoothCurve.Evaluate(step));
            yield return null;
        }

        fish.gameObject.SetActive(false);
        newText.SetActive(true);
    }

    IEnumerator FishEnterAnimation(Transform fish)
    {
        yield return new WaitForSeconds(TimingInfo.FishReturnSeconds * 0.8f);
        anim.SetTrigger("doBob");
        fishImg.sprite = fish.GetComponent<SpriteRenderer>().sprite;
        fishImg.transform.localScale = fish.localScale * 10;
    }
}
