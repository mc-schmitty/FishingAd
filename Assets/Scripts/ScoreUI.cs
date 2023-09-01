using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    public float score;
    [SerializeField]
    private TextMeshProUGUI scoreText;
    [SerializeField]
    private TextMeshProUGUI addScoreText;

    private void OnEnable()
    {
        FishingRod.FishCaught += AddFishCoin;
    }

    private void OnDisable()
    {
        FishingRod.FishCaught -= AddFishCoin;
    }

    void Start()
    {
        score = 0;
    }

    private void AddFishCoin(Fish fish)
    {
        StartCoroutine(UpdateScore(score, fish.Points + score, 1f));
        score += fish.Points;
        addScoreText.text = "+" + fish.Points.ToString("n1");
    }

    IEnumerator UpdateScore(float oldScore, float newScore, float timeToUpdate)
    {
        
        yield return new WaitForSeconds(TimingInfo.FishPulledSeconds + TimingInfo.FishLingerSeconds + TimingInfo.FishReturnSeconds);

        StartCoroutine(AddTextFadeinout(timeToUpdate));     // Show how many points gained in total

        for(float step = 0; step <= 1; step += Time.deltaTime / timeToUpdate)       // Rolling number up
        {
            scoreText.text = Mathf.Lerp(oldScore, newScore, step).ToString("n1");
            yield return null;
        }

        scoreText.text = newScore.ToString("n1");
    }

    IEnumerator AddTextFadeinout(float timeToUpdate)
    {
        float quarterTime = timeToUpdate / 4f;

        /* Fade in for 1/4 time
        for(float step = 0; step <= 1; step += Time.deltaTime / quarterTime)
        {
            addScoreText.color = new Color(addScoreText.color.r, addScoreText.color.g, addScoreText.color.b, Mathf.Lerp(0f, 1f, step));       // oh ok i had this from 0 - 255 but i guess it goes from 0 - 1 so lerp is pointless oops
            yield return null;
        }*/
        addScoreText.color = new Color(addScoreText.color.r, addScoreText.color.g, addScoreText.color.b, 1);

        // wait for 2/4 time
        yield return new WaitForSeconds(timeToUpdate);

        // Fade out for 1/4 time
        for (float step = 0; step <= 1; step += Time.deltaTime / quarterTime)
        {
            addScoreText.color = new Color(addScoreText.color.r, addScoreText.color.g, addScoreText.color.g, Mathf.Lerp(1f, 0f, step));
            yield return null;
        }

        //addScoreText.color -= new Color(0, 0, 0, 1);
    }
}
