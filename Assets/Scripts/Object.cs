using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if ((rb == null || rb.velocity.x < 1) && transform.position.x < cam.transform.position.x - 20)
        {
            Destroy(gameObject);
        }
    }
}
