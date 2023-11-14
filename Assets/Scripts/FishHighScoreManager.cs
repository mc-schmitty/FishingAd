using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FishHistory
{
    public class FishHighScoreManager : MonoBehaviour
    {
        public static FishHighScoreManager Manager;

        // Record Holding functionality
        [SerializeField]
        private FishHistoryListNode lengthRecordNode;
        [SerializeField]
        private UIFish lengthRecordFish;
        [SerializeField]
        private FishHistoryListNode scoreRecordNode;
        [SerializeField]
        private UIFish scoreRecordFish;
        [SerializeField]
        private FishHistoryListNode bountyRecordNode;
        [SerializeField]
        private UIFish bountyRecordFish;

        // Stats screen items
        [SerializeField]
        private TextMeshProUGUI totalFishCaughtText;
        private int totalFishCaught;
        [SerializeField]
        private TextMeshProUGUI totalNormalScoreText;
        private float totalNormalScore;
        [SerializeField]
        private TextMeshProUGUI totalBountiesClaimedText;
        private int totalBountiesClaimed;
        [SerializeField]
        private TextMeshProUGUI totalBountyScoreText;
        private float totalBountyScore;
        [SerializeField]
        private TextMeshProUGUI totalPointsLostByFishText;
        private float totalPointsLostByFish;
        [SerializeField]
        private TextMeshProUGUI totalCombinedScoreText;
        private float totalCombinedScore;

        // Fish Catch UI Stuff
        [SerializeField]
        private GameObject newText;
        [SerializeField]
        private GameObject newHighscoreText;

        private void Awake()
        {
            if (Manager == null)
                Manager = this;
            else
                this.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            FishingRod.FishCaught += IncrementFishCounter;
            FishBounty.FishShotHit += IncrementPointLossCounter;
        }

        private void OnDisable()
        {
            FishingRod.FishCaught -= IncrementFishCounter;
            FishBounty.FishShotHit -= IncrementPointLossCounter;
        }

        // Test if any records are broken, and add them if true
        // Currently called by FishHistory, dont need to call itself
        public bool TestNewRecord(FishHistory fh)
        {
            bool newRecordFlag = false;

            // Test Length
            if (lengthRecordNode.FishData == null || lengthRecordNode.FishData.Length < fh.Length)
            {
                lengthRecordNode.FishData = fh;
                lengthRecordFish.LoadFish(fh);
                newRecordFlag = true;
            }

            // Test Score
            if (scoreRecordNode.FishData == null || scoreRecordNode.FishData.Points < fh.Points)
            {
                scoreRecordNode.FishData = fh;
                scoreRecordFish.LoadFish(fh);
                newRecordFlag = true;
            }

            // Test Bounty
            if (fh.Bounty > 0 && (bountyRecordNode.FishData == null || bountyRecordNode.FishData.Bounty < fh.Bounty))
            {
                bountyRecordNode.FishData = fh;
                bountyRecordFish.LoadFish(fh);
                newRecordFlag = true;
            }

            if (newRecordFlag)
                StartCoroutine(EnableHighscoreNotification(fh.Bounty > 0));

            return newRecordFlag;
        }

        private void UpdateStatText(float value, TextMeshProUGUI statText, string dec)
        {
            statText.text = $"{value.ToString(dec)}";
        }
        private void UpdateStatText(int value, TextMeshProUGUI statText)
        {
            statText.text = $"{value}";
        }

        // Update stats involved in catching a fish
        private void IncrementFishCounter(Fish fish)
        {
            totalFishCaught++;                      // update internal values
            totalNormalScore += fish.Points;
            totalCombinedScore += fish.Points + fish.Bounty;

            UpdateStatText(totalFishCaught, totalFishCaughtText);               // update text values
            UpdateStatText(totalNormalScore, totalNormalScoreText, "n2");
            UpdateStatText(totalCombinedScore, totalCombinedScoreText, "n2");

            if(fish.Bounty > 0)     // Only update if fish has bounty
            {
                totalBountyScore += fish.Bounty;
                totalBountiesClaimed++;

                UpdateStatText(totalBountyScore, totalBountyScoreText, "n2");
                UpdateStatText(totalBountiesClaimed, totalBountiesClaimedText);
            }
            
        }
        private void IncrementPointLossCounter(Fish fish, float pointsLost)
        {
            totalPointsLostByFish += pointsLost;

            UpdateStatText(totalPointsLostByFish, totalPointsLostByFishText, "n2");
        }

        private IEnumerator EnableHighscoreNotification(bool bounty)
        {
            newHighscoreText.SetActive(true);       // Have "New Highscore!" text appear in next fish catch 

            yield return new WaitForSeconds(TimingInfo.FishPulledSeconds + TimingInfo.FishLingerSeconds + (bounty ? TimingInfo.FishLingerBountyBonusSeconds : 0) + TimingInfo.FishReturnSeconds);
            // Fish has been stashed away
            newText.SetActive(true);
            newHighscoreText.SetActive(false);  // Disable newhighscore for next fish
        }
    }
}

