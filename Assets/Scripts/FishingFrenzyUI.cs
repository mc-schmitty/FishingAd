using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FishingFrenzyUI : MonoBehaviour
{
    public bool doMove;

    [SerializeField]
    private float textPeriod = 3f;
    [SerializeField]
    private float fadeInSeconds = 2f;
    [SerializeField]
    private TextMeshProUGUI text1;
    [SerializeField]
    private TextMeshProUGUI text2;
    [SerializeField]
    private AudioSource frenzySource;
    private Vector3 startingPos;

    private void OnEnable()
    {
        FishTank.TriggerFishFrenzy += DoFishingFrenzy;
    }

    private void OnDisable()
    {
        FishTank.TriggerFishFrenzy -= DoFishingFrenzy;
    }

    void Start()
    {
        startingPos = text1.rectTransform.position;
        Color newCol = new Color(text1.color.r, text1.color.g, text1.color.b, 0);
        text1.color = newCol;
        text2.color = newCol;
    }

    // Move banner, looping
    void Update()
    {
        if (doMove)
        {
            float width = text1.rectTransform.rect.width;
            text1.transform.position += new Vector3(width/textPeriod * Time.deltaTime, 0, 0);
            if(text1.rectTransform.offsetMin.x >= width)
            {
                text1.rectTransform.position = startingPos;
            }
        }
    }

    private void DoFishingFrenzy(bool enable)
    {
        StartCoroutine(FadeInOut(enable, fadeInSeconds));
    }

    IEnumerator FadeInOut(bool fadein, float time)
    {
        doMove = true;
        float audioVolume = frenzySource.volume;

        int min, max;
        if (fadein)         // Just determine whether we are fading in or out
        {
            min = 0;
            max = 1;        // prob a cleverer way of doing it than this

            frenzySource.Play();    // play audiosource
        }
        else
        {
            min = 1;
            max = 0;
        }

        float timer = 0;
        while(timer < time)
        {
            float alphaVal = Mathf.Lerp(min, max, Mathf.InverseLerp(0, time, timer));
            Color newCol = new Color(text1.color.r, text1.color.g, text1.color.b, alphaVal);
            text1.color = newCol;
            text2.color = newCol;

            frenzySource.volume = alphaVal * audioVolume;

            timer += Time.deltaTime;
            yield return null;
        }

        frenzySource.volume = audioVolume;
        if (!fadein)
        {
            doMove = false;
            frenzySource.Stop();
        }           // Finally we want the movement to stop
    }
}
