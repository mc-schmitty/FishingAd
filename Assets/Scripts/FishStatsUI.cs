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
    Animator menuAnim;

    private Vector3 ogScale;

    private void OnEnable()
    {
        FishingRod.FishCaught += fish => StartCoroutine(DisplayFishStats(fish));
    }

    private void OnDisable()        // uh i don't actually know if this linq unsubscribe will properly reference the subscribe function thing it made before
    {
        FishingRod.FishCaught -= fish => StartCoroutine(DisplayFishStats(fish));
    }

    private void Start()
    {
        ogScale = transform.localScale;
        //transform.localScale = Vector3.zero;
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
        fish.transform.position = new Vector3(fish.transform.position.x, fish.transform.position.y - fish.Height/2 * 0.01f, fish.transform.position.z);     // Getting weird issue with NaN, hope this fixes it
        Debug.Log("yPos: " + fishNameText.rectTransform.position.y);
        Debug.Log("adding: " + (fishNameText.rectTransform.rect.height + fish.Height * 10));
        fishSizeText.rectTransform.localPosition = fishNameText.rectTransform.localPosition + Vector3.down * (fishNameText.rectTransform.rect.height + fish.Height * 10 + 30);  // magic numbers to make the ui look kinda good
        yield return new WaitForSeconds(TimingInfo.FishLingerSeconds - Time.deltaTime);     // then remove some delay to account for the waiting above
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
