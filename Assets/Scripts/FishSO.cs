using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Assets/Resources/Fish/NewFish", menuName ="Fish")]
public class FishSO : ScriptableObject
{
    public string fishName;
    public Sprite fishSprite;
    public float minSize;   // in cm
    public float maxSize;   // also in cm
    public int basePoints;
}
