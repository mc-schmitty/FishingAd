using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FishHistory
{
    [Serializable]
    public class FishHistory
    {
        public readonly string Name;
        public readonly Sprite Sprite;
        public readonly float Length;
        public readonly float Points;
        public readonly float Bounty;

        public FishHistory(Fish fishObj)
        {
            Name = fishObj.FishName;
            Sprite = fishObj.GetComponent<SpriteRenderer>().sprite;         // Possibly rework this to just grab FishSO? need to see how data is stored
            Length = fishObj.Length;
            Points = fishObj.Points;
            Bounty = fishObj.Bounty;
        }
    }

    public class FishHistoryManager : MonoBehaviour
    {
        public static FishHistoryManager Manager;

        [SerializeField]
        private RectTransform nodeParent;
        [SerializeField]
        private FishHistoryListNode nodePrefab;
        [SerializeField]
        private int maxHistoryNodes = 50;   // Currently does not cap out internal history list storage, just visual elements

        private List<FishHistory> historyList;

        private void Awake()
        {
            if (Manager == null)
                Manager = this;
            else
                this.gameObject.SetActive(false);
        }

        private void Start()
        {
            historyList = new();
        }

        private void OnEnable()
        {
            FishingRod.FishCaught += AddFishToHistory;
        }

        private void OnDisable()
        {
            FishingRod.FishCaught -= AddFishToHistory;
        }

        private bool LoadFishHistory()
        {
            return false;   // todo
        }

        private void AddFishToHistory(Fish fish)
        {
            // Add fish to history list
            FishHistory fh = new(fish);
            historyList.Add(fh);

            // Update ui history 
            FishHistoryListNode newNode;
            if(historyList.Count > maxHistoryNodes)
            {
                newNode = nodeParent.GetChild(0).GetComponent<FishHistoryListNode>();       // Reuse existing nodes
                newNode.transform.SetSiblingIndex(maxHistoryNodes - 1);                     // set first node to last node (moves it to top of list)
                newNode.MakeInteractable(true);
            }
            else 
            {
                newNode = GameObject.Instantiate<FishHistoryListNode>(nodePrefab, nodeParent);
            }
            newNode.FishData = fh;

            // Check if record is new high score (and add it if it is)
            newNode.SetNewRecord(FishHighScoreManager.Manager.TestNewRecord(fh));
        }

    }
}


