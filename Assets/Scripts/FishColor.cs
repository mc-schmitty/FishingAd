using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FishColor : MonoBehaviour
{
    [SerializeField]
    private Color waterColor;
    private Color ogColor;
    [SerializeField]
    private SpriteRenderer sprite;
    [SerializeField]
    private Vector3 waterLevel;

    private bool underWater;
    public bool UnderWater
    {
        get { return underWater; }
        set
        {
            underWater = value;
            if (value)
                sprite.color = waterColor;
            else
                sprite.color = ogColor;

        }
    }

    private bool spawning;


    private void Start()
    {
        if (TryGetComponent<SpriteRenderer>(out sprite))
        {
            ogColor = sprite.color;
            UnderWater = transform.position.y < waterLevel.y;   // also update the sprite on the first frame (make this a func maybe?)

            StartCoroutine(SpawnInEffect(1f));
        }
        else
            this.enabled = false;
    }

    private void Update()
    {
        if(!spawning)
            UnderWater = transform.position.y < waterLevel.y;
    }

    // This happens after update so even though im overriding it every frame i think it works out?
    IEnumerator SpawnInEffect(float dur)
    {
        float timer = 0;
        float targetAlpha = sprite.color.a;
        spawning = true;

        while(timer < dur)
        {
            // Make fish fade in considering current timer duration and intended alpha 
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, Mathf.Lerp(0, targetAlpha, Mathf.InverseLerp(0, dur, timer)));
            timer += Time.deltaTime;
            yield return null;
        }

        // normally set color here, but update takes care of that
        spawning = false;
    }
}
