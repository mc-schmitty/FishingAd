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
    }

    IEnumerator UpdateScore(float oldScore, float newScore, float timeToUpdate)
    {
        
        yield return new WaitForSeconds(0.7f + 1.5f);

        for(float step = 0; step <= 1; step += Time.deltaTime / timeToUpdate)
        {
            scoreText.text = Mathf.Lerp(oldScore, newScore, step).ToString("n2");
            yield return null;
        }

        scoreText.text = newScore.ToString("n2");
    }
}
