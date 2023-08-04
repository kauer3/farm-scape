using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject square;
    public float spawnTime = 1f;
    private float timer = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Spawn a square every second
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnTime)
        {
            Spawn();
            timer = 0f;
        }
    }

    void Spawn()
    {
        Instantiate(square, new Vector2(transform.position.x + 10, transform.position.y), Quaternion.identity);
    }
}
