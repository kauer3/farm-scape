using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    private Camera _cam;

    private void Start()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        if ((rb == null || rb.velocity.x < 1) && transform.position.x < _cam.transform.position.x - 20)
        {
            Destroy(gameObject);
        }
    }
}
