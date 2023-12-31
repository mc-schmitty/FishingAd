using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishMovement : MonoBehaviour
{
    public bool doWiggle;
    public float wiggleSpeed = 180f;

    public float swimSpeed = 5f;
    public Vector3 wanderPoint1;
    public Vector3 wanderPoint2;

    [SerializeField]
    private bool dirRight;
    [SerializeField]
    private float maxRotAngle = 30f;
    private SpriteRenderer sr;
    private Coroutine wanderCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        if(TryGetComponent<SpriteRenderer>(out sr))         // Only make fish move if it actually has a sprite renderer
            ResumeWander();
    }

    // Update is called once per frame
    void Update()
    {
        DoWiggle();
    }


    // I've made the glitchiest swimming animation and its great
    private void DoWiggle()
    {
        if (doWiggle)
        {
            float rotMult = wiggleSpeed * Time.deltaTime * (dirRight ? -1 : 1);
            transform.Rotate(Vector3.up * rotMult);

            if (Vector3.Angle(transform.forward, Vector3.forward) >= maxRotAngle)
            {
                Vector3 v = transform.rotation.eulerAngles;
                v.y = maxRotAngle * (dirRight ? -1 : 1);
                transform.rotation = Quaternion.Euler(v.x, v.y, v.z);

                dirRight = !dirRight;
            }
        }
    }

    // Does the fish have a move order
    public bool IsMoving()
    {
        return wanderCoroutine != null;
    }

    // Start moving to specified position (cancels wander)
    public void StartMoveToLocation(Vector3 position)
    {
        StartMoveToLocation(position, 0);
    }
    public void StartMoveToLocation(Vector3 position, float extraSpeed)
    {
        if (wanderCoroutine != null)
            StopCoroutine(wanderCoroutine);
        wanderCoroutine = StartCoroutine(MoveToLocation(position, extraSpeed));
    }

    // Continue wandering between point 1 and point 2 (cancels previous movement)
    public void ResumeWander()
    {
        if(wanderCoroutine != null)
            StopCoroutine(wanderCoroutine);
        wanderCoroutine = StartCoroutine(MoveBetweenLocations(wanderPoint1, wanderPoint2));
    }

    public void AttatchToBobber(Transform bobberLocation)
    {
        transform.parent = bobberLocation;
        transform.Rotate(new Vector3(0, 0, -90));
        sr.flipX = false;
        if(wanderCoroutine != null)
            StopCoroutine(wanderCoroutine);
    }


    // Move to a singular location, then stop.
    private IEnumerator MoveToLocation(Vector3 pos, float bonusSpeed)
    {
        // Have fish face the right or left side depending on the position its moving to
        sr.flipX = (pos - transform.position).x > 0;
        sr.GetComponent<FishRadarPingEffect>().flipFishSprite(sr.flipX);

        while (Vector3.Distance(transform.position, pos) > 0.0001f)     // Used to use Mathf.Approximately but it doesnt work reliably
        {
            transform.position = Vector3.MoveTowards(transform.position, pos, (swimSpeed + bonusSpeed) * Time.deltaTime);
            yield return null;
        }
        wanderCoroutine = null;
    }

    // Move between two locations, updating coroutine variable
    private IEnumerator MoveBetweenLocations(Vector3 pos1, Vector3 pos2)
    {
        // Have fish face the right or left side depending on the position its moving to
        sr.flipX = (pos1 - transform.position).x > 0;
        sr.GetComponent<FishRadarPingEffect>().flipFishSprite(sr.flipX);        // Dont like FishMovement having direct connection to PingEffect, maybe change to FishTank Manager later

        while (Vector3.Distance(transform.position, pos1) > 0.0001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, pos1, (swimSpeed) * Time.deltaTime);
            yield return null;
        }

        // This causes a stack overflow if the wander points are too close lol
        // todo: remember what the keyword is to have the loop check after the block runs (can only call new coroutine once each frame)
        wanderCoroutine = StartCoroutine(MoveBetweenLocations(pos2, pos1));
    }
}
