using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour
{
    public GameObject cam;
    float length;
    // Start is called before the first frame update
    void Start()
    {
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (cam.transform.position.x > transform.position.x + length) { transform.position = new Vector3(transform.position.x + length, transform.position.y); }
    }
}
