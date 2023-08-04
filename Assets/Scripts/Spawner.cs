using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    // public GameObject boost;
    // public GameObject skate;
    public float spawnTime = 1f;
    private float timer = 0f;
    public GameObject[] pickups;
    // Start is called before the first frame update
    void Awake()
    {

    }

    void Start()
    {

    }

    // Spawn a square every second
    private void Update()
    {
        if (timer < spawnTime)
        {
            timer += Time.deltaTime;
        }
        else
        {
            Spawn(pickups[Random.Range(0, pickups.Length)]);
            timer = 0f;
        }
    }

    void Spawn(GameObject collectable)
    {
        Instantiate(collectable, new Vector2(transform.position.x + 10, transform.position.y + Random.Range(-5, 5)), Quaternion.identity);
    }
}
