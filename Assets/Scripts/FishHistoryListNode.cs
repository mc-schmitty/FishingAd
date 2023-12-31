using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FishHistory
{
    public class FishHistoryListNode : MonoBehaviour
    {
        public static event Action<FishHistoryListNode, FishHistory> NodePressed;

        private FishHistory fishData;
        public FishHistory FishData
        {
            set
            {
                fishData = value;
                UpdateNode();
            }
            get
            {
                return fishData;
            }
        }

        [SerializeField]
        private TextMeshProUGUI fishNameText;
        [SerializeField]
        private TextMeshProUGUI fishSizeText;
        [SerializeField]
        private TextMeshProUGUI fishScoreText;
        [SerializeField]
        private TextMeshProUGUI fishBountyText;
        [SerializeField]
        private Button button;
        [SerializeField]
        private GameObject newRecordImg;


        public void ButtonClicked()
        {
            if (fishData == null)
                return;

            // Tell listeners that associated ui button has been pressed
            NodePressed?.Invoke(this, fishData);
            button.interactable = false;
        }

        public void MakeInteractable(bool set)
        {
            button.interactable = set;
        }

        public void SetNewRecord(bool set)
        {
            newRecordImg.SetActive(set);
        }

        private void UpdateNode()
        {
            fishNameText.text = fishData.Name;
            fishSizeText.text = $"{fishData.Length:n2} cm";
            fishScoreText.text = $"{fishData.Points:n1} pts";
            fishBountyText.text = $"+{fishData.Bounty:n0} bty";
        }

        
    }
}


