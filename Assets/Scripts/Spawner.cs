using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public float nextPickupPosition;
    public float nextGroundObstaclePosition;
    public GameObject[] pickups;
    public GameObject[] groundObstacles;

    private void Start()
    {
        nextPickupPosition = Random.Range(20, 30);
        nextGroundObstaclePosition = Random.Range(100, 1000);
    }

    private void Update()
    {
        ManagePickups();
        ManageObstacles();
    }

    void Spawn(GameObject[] objects, float yPosition)
    {
        Instantiate(objects[Random.Range(0, objects.Length)], new Vector2(transform.position.x + 20, yPosition), Quaternion.identity);
    }

    void ManagePickups()
    {
        if (transform.position.x > nextPickupPosition)
        {
            Spawn(pickups, Mathf.Max(transform.position.y + Random.Range(-5, 5), -3.90f));
            nextPickupPosition += Random.Range(3, 50);
        }
    }

    void ManageObstacles()
    {
        if (transform.position.x > nextGroundObstaclePosition)
        {
            Spawn(groundObstacles, -4.18f);
            nextGroundObstaclePosition += Random.Range(40, 500);
        }
    }
}
