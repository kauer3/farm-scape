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
    float startPosX, startPosY, length;
    // Start is called before the first frame update
    void Start()
    {
        startPosX = cam.transform.position.x;
        startPosY = cam.transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    private void Update()
    {
        float temp = (cam.transform.position.x * (1 - parralaxX));
        float distX = (cam.transform.position.x * parralaxX);
        float distY = (cam.transform.position.y * parralaxY);

        transform.position = new Vector3(startPosX + distX, startPosY + distY, transform.position.z);

        if (temp > startPosX + length) {
            startPosX += length;
            if (backgroundSprites.Any())
            {
                Random randSprites = new Random();
                GetComponent<SpriteRenderer>().sprite = backgroundSprites[randSprites.Next(0, backgroundSprites.Count)];
                length = GetComponent<SpriteRenderer>().bounds.size.x;
            }
        }   
    }
}
