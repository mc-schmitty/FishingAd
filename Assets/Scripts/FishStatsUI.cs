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
        StartCoroutine(AddFishSizeLerp(fish.Size, 0.5f));
        yield return new WaitForSeconds(TimingInfo.FishLingerSeconds);
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
        
    }
}
