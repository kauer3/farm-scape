using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

public class BackgroundParallax : MonoBehaviour
{
    public List<Sprite> backgroundSprites;
    public float parralaxX, parralaxY;
    public GameObject cam;
    float length;
    Vector3 lastCam;
    // Start is called before the first frame update
    void Start()
    {
        lastCam = cam.transform.position;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    private void LateUpdate()
    { 
        Vector3 camMovement = cam.transform.position - lastCam;
        Vector3 newPos = new Vector3(transform.position.x + (camMovement.x * parralaxX), transform.position.y + (camMovement.y * parralaxY));

        transform.position = newPos;

        lastCam = cam.transform.position;
        
        if(cam.transform.position.x > transform.position.x + (length*2))
        {
            if (backgroundSprites.Any())
            {
                Random rand = new Random();
                GetComponent<SpriteRenderer>().sprite = backgroundSprites[rand.Next(0, backgroundSprites.Count)];
                length = GetComponent<SpriteRenderer>().bounds.size.x;
            }
            transform.position = new Vector3(transform.position.x + (length*3), transform.position.y);
        }
    }
}
