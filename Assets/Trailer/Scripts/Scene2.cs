using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene2 : MonoBehaviour
{
    public GameObject chicken;
    float time;
    bool wait = true;
    int animNum = 0;

    // Update is called once per frame
    void Update()
    {
        if (!wait)
        {
            time += Time.deltaTime;
        }
        if(!chicken.GetComponent<Animation>().isPlaying)
        {
            wait = false;
        }
        if (!chicken.GetComponent<Animation>().isPlaying && animNum == 0 && time > 1)
        {
            chicken.GetComponent<Animation>().Play("Scene2_chickenrun3");
            animNum++;
            time = 0;
            wait = true;
        }
        if(!chicken.GetComponent<Animation>().isPlaying && animNum == 1 && time > 1)
        {
            chicken.GetComponent<Animation>().Play("Scene2_chickenrun2");
            animNum++;
            time = 0;
            wait = true;
        }
    }
}
