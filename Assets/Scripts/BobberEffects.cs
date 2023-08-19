using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobberEffects : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem bigSplash;
    [SerializeField]
    private ParticleSystem smallSplash;
    [SerializeField]
    private Animator anim;

    public void DoBob(bool isBig)
    {
        if (isBig)
        {
            bigSplash.Play();
            anim.SetTrigger("bigBob");
        }
        else
        {
            smallSplash.Play();
            anim.SetTrigger("smallBob");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Water"))
            DoBob(true);
    }


}
