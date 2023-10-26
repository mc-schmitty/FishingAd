using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishRadarPingEffect : MonoBehaviour
{
    [SerializeField]
    private float pingRate = 1;
    public float PingRate
    {
        get
        {
            return pingRate;
        }
        set
        {
            pingRate = value;
        }
    }

    [SerializeField]
    private SpriteRenderer pingSprite;
    [SerializeField]
    private Sprite startPingSprite;
    [SerializeField]
    private Sprite endPingSprite;
    [SerializeField]
    private GameObject maskObject;      // Sprite Masks are bad for performance and having one per fish is especially bad, but until I make a custom shader it will have to do
                                        // Custom shader needs to only affect current fish sprite and not any other (unlike current implementation)

    [SerializeField]
    private float maxPingScale = 4;
    [SerializeField]
    private bool enablePing = false;

    private float pingTimer;
    private Coroutine pingRoutine;

    void Update()
    {
        if (!enablePing)
            return;

        pingTimer += Time.deltaTime;
        if(pingRate > 0 && pingTimer > (1 / pingRate))
        {
            if(pingRoutine!= null)
                StopCoroutine(pingRoutine);
            pingRoutine = StartCoroutine(DoPing());
            pingTimer = 0;
        }
    }

    public void flipFishSprite(bool flip)
    {
        maskObject.transform.localScale = new Vector3(flip ? -1 : 1, 1, 1);
    }

    // Get fish's current sprite and update the sprite mask to it
    public void UpdateSprite()
    {
        SpriteRenderer localSR = GetComponent<SpriteRenderer>();
        SpriteMask mask = maskObject.GetComponent<SpriteMask>();

        mask.sprite = localSR.sprite;
    }

    /// <summary>
    /// Enable pinging effect and mask.
    /// </summary>
    public void Enable()
    {
        maskObject.SetActive(true);
        enablePing = true;
    }

    /// <summary>
    /// Disable pinging effect and mask.
    /// </summary>
    public void Disable()
    {
        maskObject.SetActive(false);
        enablePing = false;
    }

    IEnumerator DoPing()
    {
        float timer = 0;
        pingSprite.transform.localScale = Vector3.zero;
        Vector3 maxScale = Vector3.one * maxPingScale;
        float maxDuration = (1 / pingRate) * 0.9f;

        float startDuration = maxDuration / 5;
        pingSprite.sprite = startPingSprite;            // 20% of ping is the starting smaller circle
        while(timer < startDuration)
        {
            timer += Time.deltaTime;
            pingSprite.transform.localScale = Vector3.Lerp(Vector3.zero, maxScale, Mathf.InverseLerp(0, maxDuration, timer));
            yield return null;
        }

        pingSprite.sprite = endPingSprite;              // Rest of ping is the ending sprite
        while (timer < maxDuration)
        {
            timer += Time.deltaTime;
            pingSprite.transform.localScale = Vector3.Lerp(Vector3.zero, maxScale, Mathf.InverseLerp(0, maxDuration, timer));
            yield return null;
        }

        pingSprite.transform.localScale = Vector3.zero;
    }
}
