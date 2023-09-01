using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimingInfo : MonoBehaviour
{
    // So idk how to properly do this, but im trying static fields with a singleton setting it
    // It avoids setting values in a scriptable object and assigning the object to each instance that uses the timings
    // However, it still allows editing the values inside the unity editor (although idk why i made the values static and not just the actual singleton)

    public static float FishPulledSeconds;
    [SerializeField]
    private float fishPulledSeconds = 0.7f;

    public static float FishLingerSeconds;
    [SerializeField]
    private float fishLingerSeconds = 1.5f;

    public static float FishReturnSeconds;
    [SerializeField]
    private float fishReturnSeconds = 0.5f;

    private static TimingInfo tsto;

    private void Awake()
    {
        if (tsto != null)
            return;

        tsto = this;
        FishPulledSeconds = fishPulledSeconds;
        FishLingerSeconds = fishLingerSeconds;
        FishReturnSeconds = fishReturnSeconds;
    }

}
