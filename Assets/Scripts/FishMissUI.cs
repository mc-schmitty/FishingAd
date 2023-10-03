using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FishMissUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI lateMissText;
    [SerializeField]
    private TextMeshProUGUI earlyMissText;
    [SerializeField]
    private Animator anim;

    private void OnEnable()
    {
        FishTank.FishMissEarly += EarlyMiss;
        FishTank.FishMissLate += LateMiss;
    }

    private void OnDisable()
    {
        FishTank.FishMissEarly -= EarlyMiss;
        FishTank.FishMissLate -= LateMiss;
    }

    private void EarlyMiss(float timing)
    {
        Debug.Log($"Missed early by {timing} seconds!");
        StartCoroutine(DisplayMiss(false));
    }

    private void LateMiss(float timing)
    {
        Debug.Log($"Missed late by {timing} seconds!");
        StartCoroutine(DisplayMiss(true));
    }

    IEnumerator DisplayMiss(bool late)
    {
        lateMissText.gameObject.SetActive(late);
        earlyMissText.gameObject.SetActive(!late);

        anim.SetTrigger("bounceIn");
        yield return new WaitForSeconds(TimingInfo.FishLingerSeconds);
        anim.SetTrigger("bounceIn");

    }
}
