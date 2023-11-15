using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRadarPing : MonoBehaviour
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
    private Image pingImage;
    [SerializeField]
    private Sprite startPingSprite;
    [SerializeField]
    private Sprite endPingSprite;

    [SerializeField]
    private float maxPingScale = 1;
    [SerializeField]
    private bool enablePing = false;

    private float pingTimer;
    private Coroutine pingRoutine;

    private void Start()
    {
        pingImage.rectTransform.localScale = Vector3.zero;
    }

    void Update()
    {
        if (!enablePing)
            return;

        pingTimer += Time.deltaTime;
        if (pingRate > 0 && pingTimer > (1 / pingRate))
        {
            if (pingRoutine != null)
                StopCoroutine(pingRoutine);
            pingRoutine = StartCoroutine(DoPing());
            pingTimer = 0;
        }
    }

    public void Enable()
    {
        enablePing = true;
    }

    public void Disable()
    {
        enablePing = false;
    }

    IEnumerator DoPing()
    {
        float timer = 0;
        pingImage.transform.localScale = Vector3.zero;
        Vector3 maxScale = Vector3.one * maxPingScale;
        float maxDuration = (1 / pingRate) * 0.9f;

        float startDuration = maxDuration / 5;
        pingImage.sprite = startPingSprite;            // 20% of ping is the starting smaller circle
        while (timer < startDuration)
        {
            timer += Time.deltaTime;
            pingImage.transform.localScale = Vector3.Lerp(Vector3.zero, maxScale, Mathf.InverseLerp(0, maxDuration, timer));
            yield return null;
        }

        pingImage.sprite = endPingSprite;              // Rest of ping is the ending sprite
        while (timer < maxDuration)
        {
            timer += Time.deltaTime;
            pingImage.transform.localScale = Vector3.Lerp(Vector3.zero, maxScale, Mathf.InverseLerp(0, maxDuration, timer));
            yield return null;
        }

        pingImage.transform.localScale = Vector3.zero;
    }
}
