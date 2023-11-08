using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene1_firstTransitionm : MonoBehaviour
{
    public GameObject cam;
    public GameObject chicken;
    public GameObject cannonFire;
    bool animDone = false;
    float time;

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time > 4 && !animDone)
        {
            chicken.GetComponent<Animation>().Play();
            cannonFire.SetActive(true);
            time = 0;
            animDone = true;
        }
        if (cannonFire.activeSelf && time > 0.2)
        {
            cannonFire.SetActive(false);
        }
        if(animDone && time > 1)
        {
            SceneManager.LoadSceneAsync("Trailer_scene2");
        }
    }
}
