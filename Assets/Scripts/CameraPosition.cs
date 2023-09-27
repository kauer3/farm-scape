using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    public Transform player;

    void Update()
    {
        if (transform.position.y < 4.58 || transform.position.x < 8.84)
        {
            transform.position = new Vector3(Mathf.Max(transform.position.x, 8.84f), Mathf.Max(transform.position.y, 4.58f), -10);
        }
    }
}
