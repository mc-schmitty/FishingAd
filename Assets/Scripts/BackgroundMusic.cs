using System.Collections;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    [SerializeField]
    private AudioSource bgmSource;
    [SerializeField]
    private Vector2 randDurationRange;
    private float maxVolume;
    private bool fadedIn;
    private double timeToNextUpdate;

    // Start is called before the first frame update
    void Start()
    {
        maxVolume = bgmSource.volume;
        bgmSource.volume = 0;
        fadedIn = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.fixedTime > timeToNextUpdate)
        {
            StartCoroutine(FadeMusic(!fadedIn, Random.Range(randDurationRange.x, randDurationRange.y)));
        }
    }

    // I shoudld prob create like a interface or smth for this because i repeat this code constantly
    IEnumerator FadeMusic(bool fadeIn, float duration)
    {
        timeToNextUpdate = Time.fixedTime + Random.Range(randDurationRange.x, randDurationRange.y) + duration;      // Set time to perform next action after fadeinout ends

        float timer = 0;
        while(timer < duration)
        {
            float invl = Mathf.InverseLerp(0, duration, timer);
            bgmSource.volume = Mathf.Lerp(0, maxVolume, fadeIn ? invl : 1 - invl);
            timer += Time.deltaTime;
            yield return null;
        }

        fadedIn = fadeIn;       // update faded in status
    }
}
