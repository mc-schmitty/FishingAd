using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FishingRod : MonoBehaviour
{
    public bool IsCast
    { get; private set; }

    public bool repeatCast = true;

    [SerializeField]
    private Animator rodAnim;
    [SerializeField]
    private Animator reelAnim;

    [SerializeField]
    private Rigidbody bobber;
    [SerializeField]
    private Vector3 bobberCastPoint;
    [SerializeField]
    private Vector3 bobberCastForce;
    [SerializeField]
    private Vector3 bobberPullForce;

    private float delay;
    private bool firstCastFlag;

    void Start()
    {
        IsCast = false;
        firstCastFlag = true;
    }

    // Temp update
    private void Update()
    {
        
        if (delay <= 0 && Input.GetMouseButtonDown(0))
        {
            if (IsCast)
                PullLine();
            else
                CastLine();
        }

        delay = Mathf.Max(delay - Time.deltaTime, 0);
    }

    private void PullLine()
    {
        IsCast = false;
        bobber.useGravity = true;
        bobber.AddForce(bobberPullForce, ForceMode.Impulse);
        rodAnim.SetTrigger("PullBack");

        // Try to catch fish
        Fish caughtFish = FishTank.fishTank.TryCatch();
        int success = caughtFish == null ? FishCatchFail() : FishCatchSuccess(caughtFish);
        delay += 1f + success;
        Invoke("StowBobber", 1f);
        if(success == 1)
            StartCoroutine(FishStowCoroutine(caughtFish));
    }

    // This starts the cast animation
    public void CastLine()
    {
        IsCast = true;
        bobber.gameObject.SetActive(true);
        bobber.position = bobberCastPoint;
        rodAnim.SetTrigger("CastOff");
        delay += 2f;
        Invoke("CastBobber", 0.17f);

        if (firstCastFlag)
        {
            firstCastFlag = false;
            FishTank.fishTank.StartFishStates();
        }
    }

    // This is called after the cast animation starts
    private void CastBobber()
    {
        bobber.useGravity = true;
        bobber.velocity = Vector3.zero;
        bobber.AddForce(bobberCastForce, ForceMode.Impulse);
    }

    private void StowBobber()
    {
        bobber.useGravity = false;
        bobber.velocity = Vector3.zero;
        bobber.gameObject.SetActive(false);

        if (repeatCast)
        {
            Invoke("CastLine", 0.1f);
        }
    }

    private void StartReel()
    {
        reelAnim.SetBool("Reeling", true);
    }

    private void StopReel()
    {
        reelAnim.SetBool("Reeling", false);
    }


    private int FishCatchSuccess(Fish fish)
    {
        Debug.Log("Caught a " + fish.FishName + " and scored " + fish.Points + " points!");
        //fish.gameObject.SetActive(false);
        return 1;
    }

    private int FishCatchFail()
    {
        return 0;
    }

    private IEnumerator FishStowCoroutine(Fish caughtFish)
    {
        yield return new WaitForSeconds(0.7f);

        // todo: expand on fish catching, prob send the fish to heaven or smth
        caughtFish.transform.parent = null;
        caughtFish.transform.rotation = Quaternion.identity;
        FishMovement fm = caughtFish.GetComponent<FishMovement>();
        fm.doWiggle = false;
        GameObject.Destroy(caughtFish.gameObject, 3f);
        //caughtFish.gameObject.SetActive(false);
    }

}
