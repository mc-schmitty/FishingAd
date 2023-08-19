using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastBobberTest : MonoBehaviour
{
    public Rigidbody bobber;
    public Camera mainCamera;
    public Vector3 offset;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 2000f))
            {
                MoveBobber(hit.point);
            }

        }
    }

    // Update is called once per frame
    void MoveBobber(Vector3 hitPosition)
    {
        bobber.position = hitPosition + offset;
        bobber.velocity = Vector3.zero;
    }
}
