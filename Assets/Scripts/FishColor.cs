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


    private void Start()
    {
        if (sprite == null)
            sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)                             // double check to see if sprite is there (kinda unnecessary)
        {
            ogColor = sprite.color;
            UnderWater = transform.position.y < waterLevel.y;   // also update the sprite on the first frame (make this a func maybe?)
        }
        else
            this.enabled = false;
    }

    private void Update()
    {
        UnderWater = transform.position.y < waterLevel.y;
    }
}
