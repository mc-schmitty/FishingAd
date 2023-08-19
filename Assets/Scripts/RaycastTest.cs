using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastTest : MonoBehaviour
{
    public ParticleSystem particlePrefab;
    public Camera mainCamera;
    public Vector3 offset;


    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !particlePrefab.isPlaying)
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit, 2000f))
            {
                CreateSplash(hit.point);
            }
            
        }
    }

    private void CreateSplash(Vector3 pos)
    {
        particlePrefab.transform.position = pos + offset;
        particlePrefab.Play();
    }
}
