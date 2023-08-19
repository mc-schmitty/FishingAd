using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{
    // These will be replaced by a manager later
    public float ScaleMod = 1f;
    public float SizeUnits = 0.01f;     // Because size is in cm, not m
    // ----------------------------------------

    public string FishName;
    public float Size;
    public float Points;

    [SerializeField]
    private FishSO fishStats;
    [SerializeField]
    private SpriteRenderer sr;
    private float baseScale = 1;
    

    private void Start()
    {
        if(sr == null)
            sr = GetComponent<SpriteRenderer>();
        if (fishStats != null)
            Initialize(fishStats);
    }
    public void Initialize(FishSO fishSO)
    {
        fishStats = fishSO;
        FishName = fishStats.fishName;
        sr.sprite = fishStats.fishSprite;
        Size = Random.Range(fishStats.minSize, fishStats.maxSize);
        ScaleSize();

        // Point formula tbd, current idea is base + bonus
        // bonus = base/2 * (1 + x)
        // x = 0% - 100% of how close to max size fish is
        /* So a tiny fish of size min with base 10 points would get you: 10 + 5*(1+0%) = 15 points
           Meanwhile a large fish of size min with base 50 points would score: 50 + 25 * (1 + 100%) = 100 points
           Medium at half: 25 + 12.5 * (1 + 50%) = idk prob around 25 + 19 ~= 44?
           idk how i like this basically the base points are the biggest deal and the size bonus isnt when its usually the other way in fishing */
        Points = fishStats.basePoints + fishStats.basePoints * 2 * (Mathf.InverseLerp(fishStats.minSize, fishStats.maxSize, Size));
    }

    private void ScaleSize()
    {
        // Scale size from sprite to real-world size in cm
        float ppu = fishStats.fishSprite.pixelsPerUnit;
        float width = fishStats.fishSprite.rect.width;
        // Get the number of units the fish sprite take up ingame
        float inGameSize = width / ppu;
        // Calculate the scale required to modify the ingamesize to the rolled irl fish size (oops also got to convert size to m from cm)
        float scaling = (Size * SizeUnits) / inGameSize;

        baseScale = scaling;
        // Adjust the scale by this amount * global scale modifier
        transform.localScale = Vector3.one * (scaling * ScaleMod);
    }
}
