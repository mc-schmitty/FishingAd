using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FishTank : MonoBehaviour
{
    public static FishTank fishTank;

    // Collection that holds fish
    // Should populate and depopulate
    // For now, choose a random fish from pool and set availability
    // trigger "tells" that a fish is available
    public int maxPoolSize = 20;
    public int initialPoolSize = 5;

    public float ScaleMod = 1;
    public float SizeUnits = 0.01f;
    [SerializeField]
    private GameObject fishPrefab;
    [SerializeField]
    private Transform bobberHookTransform;
    private BobberEffects bobScript;


    private FishSO[] loadedFishSO;
    private List<Fish> fishPool;

    [SerializeField]
    private Fish interestedFish;
    [SerializeField]
    private TankState state;
    [SerializeField]
    private double timeAtNextEvent;

    private enum TankState
    {
        Empty,
        Interested,
        Catchable,
        Disabled
    }


    private void Awake()
    {
        if (FishTank.fishTank == null)
            FishTank.fishTank = this;
        else
            this.enabled = false;
    }

    private void Start()
    {
        // Load scriptableobjects
        loadedFishSO = Resources.LoadAll<FishSO>("Fish");
        fishPool = new List<Fish>();
        bobScript = bobberHookTransform.GetComponentInParent<BobberEffects>();

        // Initialize starting fish
        for (int n = 0; n < initialPoolSize; n++)
        {
            Fish newFish = GameObject.Instantiate(fishPrefab, transform.position + Random.insideUnitSphere * 2, transform.rotation, transform).GetComponent<Fish>();
            // So rn im not sure if start will be called at instantiation, or after this function finishes. I guess ill find out and update this later
            newFish.ScaleMod = ScaleMod;
            newFish.SizeUnits = SizeUnits;
            newFish.Initialize(loadedFishSO[Random.Range(0, loadedFishSO.Length)]);
            newFish.name = newFish.FishName + newFish.GetInstanceID();
            fishPool.Add(newFish);

            FishMovement fm = newFish.GetComponent<FishMovement>();
            int invert = Random.value >= 0.5 ? 1 : -1;      // Randomly invert the starting positions
            fm.wanderPoint1 = fm.transform.position - (Vector3.right * invert) * (5.0f + Random.Range(-0.3f, 0.3f));
            fm.wanderPoint2 = fm.transform.position - (Vector3.left * invert) * (5.0f + Random.Range(-0.3f, 0.3f));
            fm.swimSpeed += Random.Range(-0.2f, 0.4f);      // Lot of random calls here per fish
            fm.wiggleSpeed += Random.Range(-30, 30);
        }

        state = TankState.Disabled;
    }

    private void Update()
    {
        // loop to occasionally make fish active and/or catchable
        CheckUpdateState();

    }

    // Just a way to start fish states
    public void StartFishStates()
    {
        timeAtNextEvent = Time.fixedTime + Random.Range(6f, 12f);
        state = TankState.Empty;
    }

    // Funny little thematic function: try to catch a fish, returns fish caught or return null if nothing caught
    // I thought it would be funny to make a FishNotFound Exception so you need to surround this with a try-catch but thats kinda dumb
    public Fish TryCatch()
    {
        if (state == TankState.Catchable && interestedFish != null)
        {
            Fish outp = interestedFish;
            fishPool.Remove(outp);

            // manybe send a message that a fish was caught
            timeAtNextEvent = Time.fixedTime + stateCaughtFish();
            return outp;
        }

        timeAtNextEvent = Time.fixedTime + stateScareFish();
        return null;
    }

    // Trying some weird state-based thing
    private void CheckUpdateState()
    {
        if (Time.fixedTime > timeAtNextEvent)
        {
            float newDelay = 0;

            switch(state){
                case TankState.Disabled:
                    newDelay = 1f;
                    break;
                case TankState.Empty:
                    // Choose a fish to become interested
                    newDelay = stateChooseInterested();
                    break;
                case TankState.Interested:
                    // Fish becomes catchable
                    newDelay = stateBecomeCatchable();
                    break;
                case TankState.Catchable:
                    // Fish either leaves or goes back to being interested
                    newDelay = stateStopCatchable();
                    break;
            }
            
            timeAtNextEvent = Time.fixedTime + newDelay;
        }
    }


    /* State change functions all return the time in seconds to transition to the next state */
    // Choose a fish to become interested in bait
    private float stateChooseInterested()
    {
        if(fishPool.Count == 0)
            return 5f;              // Pool is empty, try again in X seconds (ill refactor the magic numbers later)

        state = TankState.Interested;
        interestedFish = fishPool[Random.Range(0, fishPool.Count)];        // Choose random fish (might be expanded on later)
        interestedFish.GetComponent<FishMovement>().StartMoveToLocation(bobberHookTransform.position);
        return Random.Range(3f, 9f);
    }

    // Interested fish becomes catchable
    private float stateBecomeCatchable()
    {
        state = TankState.Catchable;
        bobScript.DoBob(true);
        return Random.Range(0.5f, 1f);
    }

    // Interested fish stops being catchable, either stays or runs away
    private float stateStopCatchable()
    {
        // have chance fish returns to being interested and chance it flees
        if(Random.value > 0.6)
        {
            state = TankState.Interested;

            return Random.Range(1.5f, 4f);  
        }
        else
        {
            state = TankState.Empty;
            interestedFish.GetComponent<FishMovement>().ResumeWander();
            interestedFish = null;
            return Random.Range(6f, 15f);
        }
    }

    // Fish no longer interested, runs away
    private float stateScareFish()
    {
        state = TankState.Empty;
        if(interestedFish != null)
        {
            interestedFish.GetComponent<FishMovement>().ResumeWander();
            interestedFish = null;
        }
        return Random.Range(6f, 15f);
    }


    // Fish caught, kindly let it know
    private float stateCaughtFish()
    {
        state = TankState.Empty;
        interestedFish.GetComponent<FishMovement>().AttatchToBobber(bobberHookTransform);
        interestedFish = null;
        return Random.Range(6f, 15f);
    }
}
