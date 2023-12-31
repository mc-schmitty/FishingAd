using FishHistory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FishHistory
{
    public class UIFish : MonoBehaviour
    {
        [SerializeField]
        private Image fishImage;
        [SerializeField]
        private float scalingFactor = 1f;
        [SerializeField]
        private bool interactable = true;

        private FishHistoryListNode previousNode;

        private void OnEnable()
        {
            FishHistoryListNode.NodePressed += HistoryButtonClicked;
        }

        private void OnDisable()
        {
            FishHistoryListNode.NodePressed -= HistoryButtonClicked;
        }

        // Load fish into frame
        private void HistoryButtonClicked(FishHistoryListNode node, FishHistory fishData)
        {
            if (!interactable)      // Can make fish container static
                return;

            // Make previous history item light up again
            if(previousNode != null)
            {
                previousNode.MakeInteractable(true);
            }
            previousNode = node;

            // Now fill in the fish data
            LoadFish(fishData);
        }

        public void LoadFish(FishHistory fishData)
        {
            fishImage.sprite = fishData.Sprite;

            // Get height of fish in cm
            float height = (fishImage.sprite.rect.height / fishImage.sprite.rect.width) * fishData.Length;

            // prob do some screen scaling stuff here, ill do that later
            fishImage.rectTransform.sizeDelta = new Vector2(fishData.Length * scalingFactor, height * scalingFactor);

            // Later animate fish (currently this is done automatically via attached FishMovement)
        }

        public void ClearFish()
        {
            fishImage.sprite = null;

            // Make previous history item light up again
            if (previousNode != null)
            {
                previousNode.MakeInteractable(true);
            }

            fishImage.rectTransform.sizeDelta = Vector2.zero;
        }
    }
}


