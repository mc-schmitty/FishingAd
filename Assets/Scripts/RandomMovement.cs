using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovement : MonoBehaviour
{
    [SerializeField]
    private Transform[] jiggleJiggleSkin;
    [SerializeField]
    private float duration = 0.4f;
    [SerializeField]
    private float multiplier = 0.2f;

    private bool set;
    private Vector3[] setPos;

    private void OnEnable()
    {
        FishBounty.ShotByFish += TriggerJiggle;
    }

    private void OnDisable()
    {
        FishBounty.ShotByFish -= TriggerJiggle;
    }

    // Failsafe if jiggle effect doesn't restore item back to normal, call once to set and later to reset
    public void RestoreRod()
    {
        if (!set)   // Not set, record positions
        {
            setPos = new Vector3[jiggleJiggleSkin.Length];

            for(int i = 0; i < setPos.Length; i++)
            {
                setPos[i] = jiggleJiggleSkin[i].position;
            }
            set = true;
        }
        else
        {
            // Restore positions after set
            for(int i = 0; i < jiggleJiggleSkin.Length; i++)
            {
                jiggleJiggleSkin[i].position = setPos[i];
            }
        }
    }

    private void TriggerJiggle(Fish fish, float amount)
    {
        StartCoroutine(JigglePieces(duration));
    }

    // Jiggle all pieces, then restore them to their regular location
    IEnumerator JigglePieces(float duration)
    {
        float timer = 0;

        Vector3[] vList = new Vector3[jiggleJiggleSkin.Length];
        for(int i = 0; i < jiggleJiggleSkin.Length; i++)
        {
            vList[i] = Vector3.zero;
        }

        yield return new WaitForSeconds(TimingInfo.FishShootDelaySeconds);

        while(timer < duration)
        {
            for(int i = 0; i < jiggleJiggleSkin.Length; i++)
            {
                jiggleJiggleSkin[i].position -= vList[i];       // undo previous rand
                vList[i] = Random.insideUnitSphere * multiplier;
                jiggleJiggleSkin[i].position += vList[i];
            }
            timer += Time.deltaTime;
            yield return null;
        }

        // restore to original position
        for(int i = 0; i < jiggleJiggleSkin.Length; i++)
        {
            jiggleJiggleSkin[i].position -= vList[i];
        }
    }
}
