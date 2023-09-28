using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BountyText : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI wantedText;
    [SerializeField]
    private float duration = 1f;

    private void OnEnable()
    {
        FishingRod.FishCaught += ShowBountyText;
    }

    private void OnDisable()
    {
        FishingRod.FishCaught -= ShowBountyText;
    }

    // Start is called before the first frame update
    void Start()
    {
        wantedText.text = "";
        //StartCoroutine(DisplayBountyText(duration, 20f));
    }

    private void ShowBountyText(Fish fish)
    {
        if(fish.Bounty > 0)
        {
            StartCoroutine(DisplayBountyText(duration, fish.Bounty));
        }
    }

    IEnumerator DisplayBountyText(float duration, float bounty)     // Want to add each character in WANTED one by one
    {
        string wanttext = "wanted: " + bounty.ToString("n0");
        int charpos = 1;
        int tlen = wanttext.Length;
        float fontSize = wantedText.fontSize;
        float timer = 0;
        wantedText.text = "";

        // Want to last for var:duration, and start after var:fadeindelay while having window FishLingerSeconds + BountyBonusLinger
        yield return new WaitForSeconds(TimingInfo.FishPulledSeconds + 0.5f);

        while(charpos < 8)
        {
            wantedText.fontSize = fontSize;
            if((float)charpos/(float)tlen <= Mathf.InverseLerp(0, duration, timer))       // Check if timer has passed a letter threshold
            {
                wantedText.text += wanttext[charpos - 1];
                //Debug.Log("Adding char: " + wanttext[charpos - 1]);
                charpos++;
                wantedText.fontSize = fontSize + 2;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        // Done adding letters to wanted
        wantedText.text = wanttext;
        wantedText.fontSize = fontSize + 2;
        while(timer < duration)   // Still count down to duration
        {
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(TimingInfo.FishLingerSeconds - 0.5f - duration + TimingInfo.FishLingerBountyBonusSeconds);
        // extra time gone, return everything to normal
        wantedText.fontSize = fontSize;
        wantedText.text = "";
    }
}
