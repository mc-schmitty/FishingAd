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


    private void Start()
    {
        // Don't want the particle effects staying on the bobber, but i like keeping them inside the bobber to clean up the scene
        bigSplash.transform.parent = transform.parent;
        smallSplash.transform.parent = transform.parent;
    }


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
        {
            bigSplash.transform.position = transform.position;
            smallSplash.transform.position = transform.position;
            DoBob(true);
        }
            
    }


}
