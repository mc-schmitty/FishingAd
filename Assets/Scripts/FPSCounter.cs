using System.Collections;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI text;
    [SerializeField]
    private float updateRate = 0.25f;

    IEnumerator Start()
    {
        WaitForSecondsRealtime wfs = new(updateRate);       // kinda funny the one time i actually cache the waitforseconds is the time i might actually want to change it in realtime
        
        while (true)
        {
            yield return wfs;
            // simple fps script that may or may not be accurate
            float fps = 1f / Time.unscaledDeltaTime;
            text.text = fps.ToString("n2");
        }
    }
}
