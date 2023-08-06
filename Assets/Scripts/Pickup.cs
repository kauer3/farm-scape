using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }
    private void Update()
    {
        if (transform.position.x < cam.transform.position.x - 10)
        {
            Destroy(gameObject);
        }
    }
}
