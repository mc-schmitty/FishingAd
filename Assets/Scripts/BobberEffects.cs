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
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip smallSplashSound;
    [SerializeField]
    private AudioClip largeSplashSound;


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
            audioSource.clip = largeSplashSound;
            audioSource.Play();
        }
        else
        {
            smallSplash.Play();
            anim.SetTrigger("smallBob");
            audioSource.clip = smallSplashSound;
            audioSource.Play();
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
