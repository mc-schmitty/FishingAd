using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FishGame
{
    struct ListNode
    {
        public readonly Fish fish;
        public readonly WantedNode element;

        public ListNode(Fish newfish, WantedNode newnode) : this()
        {
            this.fish = newfish;
            this.element = newnode;
        }
    }

    public class WantedList : MonoBehaviour
    {
        [SerializeField][Range(0, 15)]
        private int maxListSize = 0;
        [SerializeField]
        private float nodeWidth = 30f;
        [SerializeField]
        private GameObject prefabNode;
        [SerializeField]
        private GameObject header;

        private List<ListNode> wantedList;

        void Awake()
        {
            wantedList = new();
        }

        private void Start()
        {
            header.SetActive(false);
        }

        private void OnEnable()
        {
            FishBounty.FishShooting += OnFishShot;
            FishingRod.FishCaught += OnFishCaught;
        }

        private void OnDisable()
        {
            FishBounty.FishShooting -= OnFishShot;
            FishingRod.FishCaught -= OnFishCaught;
        }

        public void AddNode(Fish wfish)
        {
            // find fish position
            int listindex = FindSortedIndex(wfish.Bounty);
            if(maxListSize > 0 && wantedList.Count >= maxListSize)      // We have reached the max list size
            {
                if(listindex == wantedList.Count)           // What we're adding is the smallest, so just dont
                    return;
                else
                {
                    int lastIndex = wantedList.Count - 1;               // Otherwise, quietly remove the last node
                    wantedList[lastIndex].element.gameObject.SetActive(false);
                    Destroy(wantedList[lastIndex].element.gameObject);
                    wantedList.RemoveAt(lastIndex);
                }
            }

            // Initialize node
            WantedNode newnode = GameObject.Instantiate(prefabNode, transform.position + IndexToCanvasPosition(wantedList.Count), transform.rotation, transform).GetComponent<WantedNode>();
            newnode.AddFish(wfish);
            ListNode newListNode = new(wfish, newnode);
            // insert listnode
            wantedList.Insert(listindex, newListNode);
            // apply visual effects to move lower items down
            newnode.MoveNode(IndexToCanvasPosition(listindex), false);
            for(int i = listindex+1; i < wantedList.Count; i++)
            {
                wantedList[i].element.MoveNode(IndexToCanvasPosition(i), false);
            }
        }

        public int RemoveNode(Fish wfish)
        {
            int nodeIndex = FindNodeIndexByFish(wfish);
            if (nodeIndex < 0)      // break if fish not found
                return -1;
            // Remove node
            wantedList[nodeIndex].element.RemoveNode();
            wantedList.RemoveAt(nodeIndex);

            // Update position of lower nodes
            for (int i = nodeIndex; i < wantedList.Count; i++)
            {
                wantedList[i].element.MoveNode(IndexToCanvasPosition(i), true);
            }

            return nodeIndex;
        }

        public void UpdateNode(Fish wfish, float scoreinc)
        {
            int nodeIndex = FindNodeIndexByFish(wfish);
            if (nodeIndex < 0)      // if node doesnt exist, add node
            {
                AddNode(wfish);
                return;
            }
            // does exist, update bounty
            ListNode tempNode = wantedList[nodeIndex];
            tempNode.element.UpdateBounty(scoreinc);
            int newNodeIndex = FindSortedIndex(wfish.Bounty);
            if (nodeIndex <= newNodeIndex)
                return;     // if node hasn't changed position, leave it here
            // otherwise, shift it up and all other elements down
            wantedList.RemoveAt(nodeIndex);
            wantedList.Insert(newNodeIndex, tempNode);  // Change the order of the node in the list
            // Now move the list items
            tempNode.element.MoveNode(IndexToCanvasPosition(newNodeIndex), false);
            // lower nodes
            for(int i = newNodeIndex + 1; i <= nodeIndex; i++)
            {
                wantedList[i].element.MoveNode(IndexToCanvasPosition(i), false);
            }
        }


        // This is the function called via delegates
        private void OnFishShot(Fish fish, float pointsLost)
        {
            float bountyScore = pointsLost * 1.5f;          // Apply current bounty modifier (subject to change)
            StartCoroutine(IOnFishShot(fish, bountyScore));
        }

        IEnumerator IOnFishShot(Fish fish, float bountyGained)
        {
            yield return new WaitForSeconds(TimingInfo.FishShootDelaySeconds);
            UpdateNode(fish, bountyGained);
            if (!header.activeInHierarchy)
                header.SetActive(true);
        }

        // This is the function called via delegates
        private void OnFishCaught(Fish fish)
        {
            if(fish.Bounty > 0)
            {
                StartCoroutine(IOnFishCaught(fish));
            }
        }

        IEnumerator IOnFishCaught(Fish fish)
        {
            // Wait for fish to be fully deposited
            yield return new WaitForSeconds(TimingInfo.FishPulledSeconds + TimingInfo.FishLingerSeconds + TimingInfo.FishLingerBountyBonusSeconds + TimingInfo.FishReturnSeconds);
            int success = RemoveNode(fish);
            // only continue if fish existed (which is should, since it had a bounty but)
            if(success >= 0)
            {
                // Wait for fish to be crossed out in list
                yield return new WaitForSeconds(1f);
                if (wantedList.Count == 0)
                    header.SetActive(false);
            }

        }


        // Linearly search through list, find index such that fish is sorted from high -> low
        private int FindSortedIndex(float bounty)
        {
            for(int i = 0; i < wantedList.Count; i++)
            {
                if(bounty > wantedList[i].fish.Bounty)
                {
                    return i;
                }
            }

            return wantedList.Count;
        }

        // Return node location
        private Vector3 IndexToCanvasPosition(int index)
        {
            return new Vector3(0, -nodeWidth * index, 0);
        }

        private int FindNodeIndexByFish(Fish fish)
        {
            for(int i = 0; i < wantedList.Count; i++)
            {
                if (fish.Equals(wantedList[i].fish))
                    return i;
            }

            return -1;
        }
    }
}

