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
    [Tooltip("Seconds until state changes from Empty to Interested the first time you cast.")]      // Default: 6 - 12
    private Vector2 timeRangeInitialChooseInterested;
    [SerializeField]
    [Tooltip("Seconds until state changes from Interested to Catchable.")]      // Default: 3 - 9
    private Vector2 timeRangeBecomeCatchable;
    [SerializeField]
    [Tooltip("Seconds until state changes from Catchable.")]        // Default: 0.5 - 1
    private Vector2 timeRangeCatchableWindow;
    [SerializeField]
    [Tooltip("Seconds until state changes from Interested to Catchable after fish stays at hook.")]     // Default: 1.5 - 4
    private Vector2 timeRangeRepeatBecomeCatchable;
    [SerializeField]
    [Tooltip("Seconds until state changes from Empty to Interested after failing to catch a fish.")]        // Default: 6 - 15
    private Vector2 timeRangeChooseInterestedFailure;
    [SerializeField]
    [Tooltip("Seconds until state changes from Empty to Interested after successfully catching a fish")]        // Default: 6 - 15
    private Vector2 timeRangeChooseInterestedSuccess;

    [SerializeField]
    private float leaveHookChance = 0.6f;
    [SerializeField]
    private float fakeoutChance = 0.6f;
    [SerializeField]
    private int maxCatchFakeouts = 2;
    private int fakeoutCount;

    [SerializeField]
    private float wanderInitialDist;
    [SerializeField]
    private Vector2 wanderRandomDistRange;
    [SerializeField]
    private Vector2 swimSpeedModRandomRange;
    [SerializeField]
    private Vector2 wiggleSpeedModRandomRange;


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
            InitializeNewFish();
        }
        StartCoroutine(PopulateTank());
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
        timeAtNextEvent = Time.fixedTime + Random.Range(timeRangeInitialChooseInterested.x, timeRangeInitialChooseInterested.y);
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

    private Fish InitializeNewFish()
    {
        if (fishPool.Count >= maxPoolSize)
            return null;

        Fish newFish = GameObject.Instantiate(fishPrefab, transform.position + Random.insideUnitSphere * 2, transform.rotation, transform).GetComponent<Fish>();
        // So rn im not sure if start will be called at instantiation, or after this function finishes. I guess ill find out and update this later
        newFish.ScaleMod = ScaleMod;
        newFish.SizeUnits = SizeUnits;
        newFish.Initialize(loadedFishSO[Random.Range(0, loadedFishSO.Length)]);
        newFish.name = newFish.FishName + newFish.GetInstanceID();
        fishPool.Add(newFish);

        FishMovement fm = newFish.GetComponent<FishMovement>();
        int invert = Random.value >= 0.5 ? 1 : -1;      // Randomly invert the starting positions
        fm.wanderPoint1 = fm.transform.position - (Vector3.right * invert) * (wanderInitialDist + Random.Range(wanderRandomDistRange.x, wanderRandomDistRange.y));
        fm.wanderPoint2 = fm.transform.position - (Vector3.left * invert) * (wanderInitialDist + Random.Range(wanderRandomDistRange.x, wanderRandomDistRange.y));
        fm.swimSpeed += Random.Range(swimSpeedModRandomRange.x, swimSpeedModRandomRange.y);      // Lot of random calls here per fish
        fm.wiggleSpeed += Random.Range(wiggleSpeedModRandomRange.x, wiggleSpeedModRandomRange.y);

        return newFish;
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
        fakeoutCount = 0;          // reset bob fakeouts
        return Random.Range(timeRangeBecomeCatchable.x, timeRangeBecomeCatchable.y);
    }

    // Interested fish becomes catchable
    private float stateBecomeCatchable()
    {
        if(Random.value > fakeoutChance || fakeoutCount >= maxCatchFakeouts)
        {
            state = TankState.Catchable;
            bobScript.DoBob(true);
            return Random.Range(timeRangeCatchableWindow.x, timeRangeCatchableWindow.y);
        }
        else
        {
            state = TankState.Interested;
            bobScript.DoBob(false);             // fake bob occurs
            fakeoutCount++;
            return Random.Range(timeRangeRepeatBecomeCatchable.x, timeRangeRepeatBecomeCatchable.y);        // go to regular delay as if fish is staying after miss
        }
    }

    // Interested fish stops being catchable, either stays or runs away
    private float stateStopCatchable()
    {
        // have chance fish returns to being interested and chance it flees
        if(Random.value > leaveHookChance)
        {
            state = TankState.Interested;

            return Random.Range(timeRangeRepeatBecomeCatchable.x, timeRangeRepeatBecomeCatchable.y);  
        }
        else
        {
            state = TankState.Empty;
            interestedFish.GetComponent<FishMovement>().ResumeWander();
            interestedFish = null;
            return Random.Range(timeRangeChooseInterestedFailure.x, timeRangeChooseInterestedFailure.y);
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
        return Random.Range(timeRangeChooseInterestedFailure.x, timeRangeChooseInterestedFailure.y);
    }


    // Fish caught, kindly let it know
    private float stateCaughtFish()
    {
        state = TankState.Empty;
        interestedFish.GetComponent<FishMovement>().AttatchToBobber(bobberHookTransform);
        interestedFish = null;
        return Random.Range(timeRangeChooseInterestedSuccess.x, timeRangeChooseInterestedSuccess.y);
    }


    // Looping, adding new fish until it reaches max
    private IEnumerator PopulateTank()
    {
        float secondsUntilPopulate;
        while (true)
        {
            secondsUntilPopulate = Mathf.InverseLerp(0, maxPoolSize, fishPool.Count) * 29 + 1;      // funny magic numbers for now, todo: add variables
            yield return new WaitForSeconds(secondsUntilPopulate);
            InitializeNewFish();
        } 
    }
}
