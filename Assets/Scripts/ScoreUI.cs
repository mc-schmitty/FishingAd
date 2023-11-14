using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    public float score;
    [SerializeField]
    private TextMeshProUGUI scoreText;
    [SerializeField]
    private TextMeshProUGUI addScoreText;
    [SerializeField]
    private TextMeshProUGUI removeScoreText;
    [SerializeField]
    private TextMeshProUGUI bountyScoreText;

    private void OnEnable()
    {
        FishingRod.FishCaught += AddFishCoin;
        FishBounty.FishShotHit += LoseFishCoin;
    }

    private void OnDisable()
    {
        FishingRod.FishCaught -= AddFishCoin;
        FishBounty.FishShotHit -= LoseFishCoin;
    }

    void Start()
    {
        score = 0;
    }

    private void AddFishCoin(Fish fish)
    {
        StartCoroutine(UpdateScore(score, fish.Points + fish.Bounty + score, 1f, fish.Bounty > 0));
        score += fish.Points + fish.Bounty;
        addScoreText.text = "+" + fish.Points.ToString("n1");
        bountyScoreText.text = $"+{fish.Bounty:n0} Bounty";
    }

    private void LoseFishCoin(Fish fish, float amount)
    {
        StartCoroutine(UpdateScore(score, Mathf.Max(score - amount, 0), 2f, false));
        score -= amount;
        removeScoreText.text = "- " + amount.ToString("n0");
    }

    IEnumerator UpdateScore(float oldScore, float newScore, float timeToUpdate, bool bounty)
    {
        if (newScore > oldScore)
        {
            yield return new WaitForSeconds(TimingInfo.FishPulledSeconds + TimingInfo.FishLingerSeconds + (bounty ? TimingInfo.FishLingerBountyBonusSeconds : 0) + TimingInfo.FishReturnSeconds);

            StartCoroutine(AddTextFadeinout(timeToUpdate, addScoreText));     // Show how many points gained in total
            if (bounty)
                StartCoroutine(AddTextFadeinout(timeToUpdate, bountyScoreText));
        }
        else if (oldScore > newScore)
        {
            //yield return new WaitForSeconds(TimingInfo.FishShootDelaySeconds);          // add timing later

            StartCoroutine(AddTextFadeinout(timeToUpdate, removeScoreText));
        }

        for (float step = 0; step <= 1; step += Time.deltaTime / timeToUpdate)       // Rolling number up
        {
            scoreText.text = Mathf.Lerp(oldScore, newScore, step).ToString("n0");
            yield return null;
        }

        scoreText.text = newScore.ToString("n0");
    }

    IEnumerator AddTextFadeinout(float timeToUpdate, TextMeshProUGUI textGui)
    {
        float quarterTime = timeToUpdate / 4f;

        /* Fade in for 1/4 time
        for(float step = 0; step <= 1; step += Time.deltaTime / quarterTime)
        {
            addScoreText.color = new Color(addScoreText.color.r, addScoreText.color.g, addScoreText.color.b, Mathf.Lerp(0f, 1f, step));       // oh ok i had this from 0 - 255 but i guess it goes from 0 - 1 so lerp is pointless oops
            yield return null;
        }*/
        textGui.gameObject.SetActive(true);
        textGui.color = new Color(textGui.color.r, textGui.color.g, textGui.color.b, 1);

        // wait for 2/4 time
        yield return new WaitForSeconds(timeToUpdate);

        // Fade out for 1/4 time
        for (float step = 0; step <= 1; step += Time.deltaTime / quarterTime)
        {
            textGui.color = new Color(textGui.color.r, textGui.color.g, textGui.color.g, Mathf.Lerp(1f, 0f, step));
            yield return null;
        }

        textGui.color = new Color(textGui.color.r, textGui.color.g, textGui.color.b, 0);
        textGui.gameObject.SetActive(false);
    }
}
