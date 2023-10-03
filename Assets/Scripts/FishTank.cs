using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FishTank : MonoBehaviour
{
    public static FishTank fishTank;
    public static event Action<float> FishMissEarly;
    public static event Action<float> FishMissLate;
    public static event Action<bool> TriggerFishFrenzy;

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
    [SerializeField]
    private Transform waterLevel;

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
    private float lateMissWindow = 1.5f;
    private float missTimer;

    [SerializeField]
    private float wanderInitialDist;
    [SerializeField]
    private Vector2 wanderRandomDistRange;
    [SerializeField]
    private Vector2 swimSpeedModRandomRange;
    [SerializeField]
    private Vector2 wiggleSpeedModRandomRange;

    [Tooltip("How many fish needed to catch before triggering a frenzy")]
    public int fishToTriggerFrenzy = 10;
    [SerializeField]
    [Tooltip("How long would it take to trigger a frenzy without catching a fish")]
    private float fishFrenzyBuildupSeconds = 120f;
    [SerializeField]
    private float fishFrenzyDurationSeconds = 20f;
    [SerializeField]
    private float fishFrenzyTimeDivider = 2f;
    private float fishFrenzyDiv = 1f;
    private float fishFrenzyHalfDiv = 1f;
    [SerializeField]
    private float fishFrenzySpeedBoost = 1f;
    public float fishFrenzyMeter;
    private float fishFrenzyTimer;
    [SerializeField]
    private float rodRecastTime = 3.8f;

    [SerializeField]
    private Fish interestedFish;
    [SerializeField]
    private TankState state;
    [SerializeField]
    private double timeAtNextEvent;

    // debug list
    [SerializeField]
    private bool debugMode = false;
    [SerializeField]
    private Fish[] fishList;

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
        fishFrenzyMeter = 0;        // Reset fish frenzy meter
        fishFrenzyDiv = 1;

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
        missTimer = Mathf.Max(0, missTimer - Time.deltaTime);
        UpdateFishFrenzy();

        // debug 
        if(debugMode)
            fishList = fishPool.ToArray();
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
            // message that says fish was caught is in FishingRod
            timeAtNextEvent = Time.fixedTime + stateCaughtFish();
            fishFrenzyMeter += (fishFrenzyTimer == 0 ? 1f : 0.25f);      // increase fish frenzy meter by 1, decreased if frenzy active
            return outp;
        }
        else if(missTimer > 0)
        {
            // Tried to catch fish too late
            FishMissLate?.Invoke(lateMissWindow - missTimer);    // float: how many seconds late was the catch
        }
        else if(interestedFish != null)
        {
            // Tried to catch fish too early (also repeat comparison but it makes code clearer)
            FishMissEarly?.Invoke((float)timeAtNextEvent - Time.fixedTime);
        }

        timeAtNextEvent = Time.fixedTime + stateScareFish();
        return null;
    }

    // Remove and return a random fish that is not interested
    // Might remove the random portion to somehwete else later
    public Fish RemoveRandomFish()
    {
        List<Fish> randPool = new List<Fish>(fishPool);         // Filter out interested fish
        randPool.Remove(interestedFish);
        Fish randChosen =  randPool[Random.Range(0, randPool.Count)];       // Choose one randomly (todo: less random-based choice)

        fishPool.Remove(randChosen);            // Remove fish from actual pool, prevent it from being interested
        return randChosen;
    }

    public void AddFish(Fish newfish)
    {
        fishPool.Add(newfish);
    }

    private Fish InitializeNewFish()
    {
        if (fishPool.Count >= maxPoolSize)
            return null;

        Fish newFish = GameObject.Instantiate(fishPrefab, transform.position + Random.insideUnitSphere * 2, transform.rotation, transform).GetComponent<Fish>();
        if (newFish.transform.position.y + 0.05f >= waterLevel.position.y)      // Push fish down a bit if it spawns too high
            newFish.transform.position += Vector3.down;
        // So rn im not sure if start will be called at instantiation, or after this function finishes. I guess ill find out and update this later
        // Edit: ok oh god oh fuck Start() is called later this kind of screws up some stuff. I dont think i can use Awake tho if i want to be able to disable scripts
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

        //newFish.GetComponent<FishColor>().UnderWater = true;            // let fish know its underwater  (ok so weird behaviour? this actually sets the fishes og color to the water color)
                                                                          // (Therefore Start() on the component script only runs after this function is finished running)

        return newFish;
    }

    // Trying some weird state-based thing
    private void CheckUpdateState()
    {
        if (Time.fixedTime > timeAtNextEvent)
        {
            float newDelay = 0;
            TankState tempState = state;
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
            Debug.Log($"Next event in {newDelay} seconds. ({tempState}->{state})");
            timeAtNextEvent = Time.fixedTime + newDelay;
        }
    }

    // Deals with fish frenzy and stuff
    private void UpdateFishFrenzy()
    {
        // buildup and startfrenzy
        if(fishFrenzyTimer <= 0)
        {
            fishFrenzyMeter += fishToTriggerFrenzy / fishFrenzyBuildupSeconds * Time.deltaTime;         // Slowly buildup fishFrenzyMeter
            if (fishFrenzyMeter >= fishToTriggerFrenzy)             // Fishmeter full now
            {
                fishFrenzyMeter = 0;
                fishFrenzyDiv = fishFrenzyTimeDivider;
                fishFrenzyHalfDiv = fishFrenzyTimeDivider / 2;
                fishFrenzyTimer = fishFrenzyDurationSeconds;
                if(state == TankState.Empty && timeAtNextEvent - Time.fixedTime > 4)        // accelerate the first catch in fish frenzy
                    timeAtNextEvent = Time.fixedTime + 4;
                TriggerFishFrenzy?.Invoke(true);
            }

        }
        // count down and stop frenzy
        else
        {
            fishFrenzyTimer -= Time.deltaTime;
            if(fishFrenzyTimer <= 0)
            {
                fishFrenzyTimer = 0;
                fishFrenzyDiv = 1;
                fishFrenzyHalfDiv = 1;
                TriggerFishFrenzy?.Invoke(false);
            }
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
        interestedFish.GetComponent<FishMovement>().StartMoveToLocation(bobberHookTransform.position, fishFrenzyTimer > 0 ? fishFrenzySpeedBoost : 0);
        fakeoutCount = 0;          // reset bob fakeouts
        return Random.Range(timeRangeBecomeCatchable.x / fishFrenzyHalfDiv, timeRangeBecomeCatchable.y / fishFrenzyHalfDiv);
    }

    // Interested fish becomes catchable
    private float stateBecomeCatchable()
    {
        // failsafe to push a fish to the hook if its too slow
        if (interestedFish.GetComponent<FishMovement>().IsMoving())
        {
            interestedFish.transform.position = bobberHookTransform.position;
            Debug.Log($"Teleported fish to {bobberHookTransform.position}");
        }

        if (Random.value > fakeoutChance / (fakeoutCount + 1) || fakeoutCount >= maxCatchFakeouts || fishFrenzyTimer > 0)     // avoid fakeouts if frenzying
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
            return Random.Range(timeRangeRepeatBecomeCatchable.x / fishFrenzyHalfDiv, timeRangeRepeatBecomeCatchable.y / fishFrenzyHalfDiv);        // go to regular delay as if fish is staying after miss
        }
    }

    // Interested fish stops being catchable, either stays or runs away
    private float stateStopCatchable()
    {
        // catchable fish missed, start visual timer
        missTimer = lateMissWindow;

        // have chance fish returns to being interested and chance it flees
        if (Random.value > leaveHookChance)
        {
            state = TankState.Interested;

            return Random.Range(timeRangeRepeatBecomeCatchable.x / fishFrenzyHalfDiv, timeRangeRepeatBecomeCatchable.y / fishFrenzyHalfDiv);  
        }
        else
        {
            state = TankState.Empty;
            interestedFish.GetComponent<FishMovement>().ResumeWander();
            interestedFish = null;
            return Random.Range(timeRangeChooseInterestedFailure.x / fishFrenzyDiv, timeRangeChooseInterestedFailure.y / fishFrenzyDiv);
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
        return Mathf.Max(Random.Range(timeRangeChooseInterestedFailure.x / fishFrenzyDiv, timeRangeChooseInterestedFailure.y / fishFrenzyDiv), rodRecastTime);
    }


    // Fish caught, kindly let it know
    private float stateCaughtFish()
    {
        state = TankState.Empty;
        interestedFish.GetComponent<FishMovement>().AttatchToBobber(bobberHookTransform);
        interestedFish = null;
        return Mathf.Max(Random.Range(timeRangeChooseInterestedSuccess.x / fishFrenzyDiv, timeRangeChooseInterestedSuccess.y / fishFrenzyDiv), rodRecastTime);
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
