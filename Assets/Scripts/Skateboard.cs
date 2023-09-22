using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skateboard : MonoBehaviour
{
    private Camera cam;
    public Rigidbody2D skateRb;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (skateRb.velocity.magnitude < 1 && transform.position.x < cam.transform.position.x - 10)
        {
            Destroy(gameObject);
        }
    }
}
