using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class FishBounty : MonoBehaviour
{
    public static event Action<Fish, float> FishShooting;
    public static event Action<Fish, float> FishShotHit;
    public static event Action<Fish, float> FishShotBlock;

    public static FishBounty fishBounty;        // May or may not end up using this

    [SerializeField]
    private FishTank fishTank;
    [SerializeField]
    private Transform[] shootingPoints;
    [SerializeField]
    private ParticleSystem[] particles;
    [SerializeField]
    private AudioSource[] audioSources;

    [SerializeField]
    private float bountyMultiplier = 1.5f;
    [SerializeField]
    private float timeToNextShootEvent = 0;
    [SerializeField]
    private float baseShootEventTime = 30f;
    [SerializeField][Min(0)]
    private float maxFishAggression = 30f;
    [SerializeField]
    private float fishAggression;
    [SerializeField]
    private float fishSpeedBuff = 2f;

    private void Awake()
    {
        if (FishBounty.fishBounty == null)
            FishBounty.fishBounty = this;
        else
            this.enabled = false;
    }

    private void OnEnable()
    {
        FishingRod.FishCaught += IncreaseAggression;
    }

    private void OnDisable()
    {
        FishingRod.FishCaught -= IncreaseAggression;
    }

    private void Start()
    {
        if (fishTank == null) 
            fishTank = FishTank.fishTank;

        CalculateAggression();
    }

    private void Update()
    {
        if(timeToNextShootEvent > 0 && Time.fixedTime > timeToNextShootEvent)
        {
            StartCoroutine(MakeFishShoot());
            timeToNextShootEvent = 0;
        }
    }

    private void Shoot(Fish shooter, int particleIndex)
    {
        float shootPoints = shooter.Points / 10;        // Take a tenth of the shooters points into account
        float pointLoss = 10 + fishAggression + shootPoints;            // Subject to change, want it to be a smaller number 
        shooter.Bounty += pointLoss * bountyMultiplier;
        particles[particleIndex].Play();
        audioSources[particleIndex].Play();

        FishShooting?.Invoke(shooter, pointLoss);          // Invoke being shot

        // Now we wait for the shot result first
        if (FishShotShield.Manager != null)
            StartCoroutine(ShotResult(shooter, shootPoints, pointLoss));
        else        // Although if shields are disabled, return to regular behaviour
        {
            fishAggression = Mathf.Max(fishAggression - shootPoints, 0);                  // Once again number not confirmed, fish takes out some aggression
            CalculateAggression();                           // Try to see if fish will shoot again
        }
    }

    private IEnumerator ShotResult(Fish shooter, float angerPoints, float pointLoss)
    {
        yield return new WaitForSeconds(TimingInfo.FishShootDelaySeconds);
        if (!FishShotShield.Manager.IsBlocking(shooter.transform.position))
        {
            FishShotHit?.Invoke(shooter, pointLoss);
            fishAggression = Mathf.Max(fishAggression - angerPoints, 0);                  // fish takes out some aggression and everything is all good
            CalculateAggression();
        }
        else
        {
            FishShotBlock?.Invoke(shooter, pointLoss);
            //fishAggression = Mathf.Min(fishAggression + angerPoints, maxFishAggression);    // If you block a shot, fish get even angrier
                                                                                              // Ok so this line of code leads to fish going crazy and shooting you every second >:(
            CalculateAggression();
        }
    }

    private void CalculateAggression()
    {
        float rand = Random.value;
        //Debug.Log($"is {rand} >= {fishAggression / 5}? {rand >= fishAggression / 5}");
        if (rand >= fishAggression / 5)       // Do roll so see if we trigger event
            return;

        if(timeToNextShootEvent != 0)   // We currently have an occuring event, speed it up
        {
            timeToNextShootEvent -= fishAggression / 10;
        }
        else                            // Otherwise, trigger the event
        {
            timeToNextShootEvent = Time.fixedTime + baseShootEventTime - fishAggression;
        }
    }

    private void IncreaseAggression(Fish fish)
    {
        float incAmount = 1 + fish.Points / 50;
        fishAggression = Mathf.Clamp(fishAggression + incAmount, 0, maxFishAggression);
        CalculateAggression();
    }

    IEnumerator MakeFishShoot()
    {
        // get fish from fishlist
        Fish agroFish = fishTank.RemoveRandomFish();
        Debug.Log($"Gonna shoot with fish {agroFish.FishName}");
        FishMovement fm = agroFish.GetComponent<FishMovement>();
        FishRadarPingEffect pinger = agroFish.GetComponent<FishRadarPingEffect>();

        // Randomly choose shooting point
        int pointIndex = Random.Range(0, shootingPoints.Length);

        // Move fish towards shooting point
        Vector3 firstDir = (shootingPoints[pointIndex].position - agroFish.transform.position);
        firstDir.y = -1 * firstDir.y;       // reflect vector on shootingPoint plane
        Vector3 endPoint = shootingPoints[pointIndex].position + firstDir;        // this will be useful later
        fm.swimSpeed += fishSpeedBuff;
        fm.StartMoveToLocation(shootingPoints[pointIndex].position);
        yield return new WaitWhile(() => fm.IsMoving());

        // Fish has reached position, shoot
        Shoot(agroFish, pointIndex);
        yield return new WaitForSeconds(1f);  // Fish lingers for a moment
        pinger.PingRate += 0.1f;              // Enable fish pinging and up its ping rate
        pinger.Enable();

        // Now make fish swim to other point
        fm.StartMoveToLocation(endPoint);
        yield return new WaitWhile(() => fm.IsMoving());
        fm.swimSpeed -= fishSpeedBuff;

        // Ok return fish to regular behaviour
        fm.ResumeWander();
        fishTank.AddFish(agroFish);
    }
}
