using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TransformLineRenderer : MonoBehaviour
{
    public Transform[] Transforms;
    private LineRenderer lr;
    private Vector3[] positions;

    private Vector3 prevPos;      // Cursed thing to prevent disabled gameObjects

    // Represents a static LineRenderer that follows transforms instead of points.
    // I don't think adding transforms realtime will work, but this is just quick code.
    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        ResetPositionSize();

        prevPos = transform.position;
    }

    void Update()
    {
        // update size if it was changed
        if(Transforms.Length != positions.Length)
            ResetPositionSize();

        // Initial check for transform positions


        for(int i = 0; i < Transforms.Length; i++)
        {
            if (Transforms[i].gameObject.activeInHierarchy)
            {
                positions[i] = Transforms[i].position;
                prevPos = Transforms[i].position;
            }
            else
            {
                // ok so crackpot theory but if its null just set it to the last non-null point
                // this breaks at the endpoints but lets just pretend that it works ok
                positions[i] = prevPos;
            }

        }

        lr.positionCount = positions.Length;
        lr.SetPositions(positions);
        
    }

    // some null checking but idk if itll be enough
    void ResetPositionSize()
    {
        if(Transforms != null)
            positions = new Vector3[Transforms.Length];
    }
}
