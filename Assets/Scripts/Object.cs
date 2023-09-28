using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
{
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (transform.position.x < cam.transform.position.x - 20)
        {
            Destroy(gameObject);
        }
    }
}
