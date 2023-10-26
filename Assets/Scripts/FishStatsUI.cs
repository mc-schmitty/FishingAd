using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class FishStatsUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI fishNameText;
    [SerializeField]
    TextMeshProUGUI fishSizeText;
    [SerializeField]
    RectTransform rulerTransform;
    [SerializeField]
    RectTransform wantedTransform;
    [SerializeField]
    Animator menuAnim;

    [SerializeField]
    float uiPadding = 2f;
    [SerializeField]
    float extraRulerPadding = 0.5f;

    [SerializeField]
    bool showDebugDots = false;
    [SerializeField]
    RectTransform testImage1;
    [SerializeField]
    RectTransform testImage2;
    

    private Vector3 ogScale;

    private void OnEnable()
    {
        FishingRod.FishCaught += ShowFishStats;
    }

    private void OnDisable()
    {
        //FishingRod.FishCaught -= fish => StartCoroutine(DisplayFishStats(fish));      // This line doesn't properly unsubscribe method
        FishingRod.FishCaught -= ShowFishStats;
    }

    private void Start()
    {
        ogScale = transform.localScale;
        //transform.localScale = Vector3.zero;
    }

    private void ShowFishStats(Fish fish)
    {
        StartCoroutine(DisplayFishStats(fish));
    }

    IEnumerator DisplayFishStats(Fish fish)
    {
        yield return new WaitForSeconds(TimingInfo.FishPulledSeconds);
        //transform.localScale = ogScale;
        menuAnim.SetTrigger("bounceIn");
        fishNameText.text = fish.FishName;
        rulerTransform.sizeDelta = new Vector2(fish.Length * 10, rulerTransform.sizeDelta.y);
        
        StartCoroutine(AddFishSizeLerp(fish.Length, 0.5f));

        yield return null;      // need time to let the fish get in position
        //fish.transform.position = new Vector3(fish.transform.position.x, fish.transform.position.y - fish.Height/2 * 0.01f, fish.transform.position.z);     // Getting weird issue with NaN, hope this fixes it
        //Vector3 middlePos = fishNameText.rectTransform.position + Vector3.down * (fishNameText.rectTransform.rect.height + fish.Height * 10);
        //Ray r = RectTransformUtility.ScreenPointToRay(Camera.main, middlePos);            // Get ray under fish name ui element
        float fishHalfHeight = fish.Height * fish.ScaleMod * fish.SizeUnits / 2;
        Vector3 worldToScreenPosTop = RectTransformUtility.WorldToScreenPoint(Camera.main, fish.transform.position + Vector3.up * fishHalfHeight);      // Get where top and bottom of fish would be on the canvas
        Vector3 worldToScreenPosBot = RectTransformUtility.WorldToScreenPoint(Camera.main, fish.transform.position + Vector3.down * fishHalfHeight);
        
        float uiHeight = worldToScreenPosTop.y - worldToScreenPosBot.y;
        float wantedHeight = fish.Bounty > 0 ? wantedTransform.rect.height : 0;     // Spacing if the wanted sign will be present
        Vector3 uiFishPosition = fishNameText.rectTransform.position + Vector3.down * (fishNameText.rectTransform.rect.height * uiPadding + wantedHeight + uiHeight / 2);
        Ray r = RectTransformUtility.ScreenPointToRay(Camera.main, uiFishPosition);
        fish.transform.position = r.origin + r.direction.normalized * Vector3.Distance(Camera.main.transform.position, fish.transform.position);      // Move fish inbetween ui elements

        fishSizeText.rectTransform.position = fishNameText.rectTransform.position + Vector3.down
            * (fishNameText.rectTransform.rect.height * (uiPadding + extraRulerPadding) + wantedHeight + uiHeight);       // Move the fish size info underneath the fish

        if (showDebugDots)
        {
            Debug.Log($"Fish Height: {fish.Height}, Fish UI Height: {uiHeight}, Ratio: {fish.Height / uiHeight}");
            testImage1.transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, fish.transform.position + Vector3.up * fishHalfHeight);
            testImage2.transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, fish.transform.position + Vector3.down * fishHalfHeight);
        }
        //Debug.Log(Vector3.Distance(fish.transform.position, Camera.main.transform.position));
        //fish.transform.position = r.origin + r.direction.normalized * 4;        // Use put fish on ray at same camera dist
        //Debug.Log("yPos: " + fishNameText.rectTransform.position.y);
        //Debug.Log("adding: " + (fishNameText.rectTransform.rect.height + fish.Height * 10));
        //fishSizeText.rectTransform.localPosition = fishNameText.rectTransform.localPosition + Vector3.down * (fishNameText.rectTransform.rect.height + fish.Height * 10 + 30);  // magic numbers to make the ui look kinda good
        yield return new WaitForSeconds(TimingInfo.FishLingerSeconds + (fish.Bounty > 0 ? TimingInfo.FishLingerBountyBonusSeconds : 0)  - Time.deltaTime);     // then remove some delay to account for the waiting above
        menuAnim.SetTrigger("bounceIn");
        //transform.localScale = Vector3.zero;
    }

    // Make that rolling number update that looks good
    IEnumerator AddFishSizeLerp(float value, float updateTime)
    {
        for(float step = 0; step <= 1; step += Time.deltaTime / updateTime)
        {
            fishSizeText.text = Mathf.Lerp(0, value, step).ToString("n2") + " cm";
            yield return null;
        }

        fishSizeText.text = value.ToString("n2") + " cm";
        
    }
}
