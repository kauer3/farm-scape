using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Rigidbody2D _player;
    private float _nextPickupPosition;
    private float _nextGroundObstaclePosition;
    private float _nextBalloonPosition;
    public GameObject[] _pickups;
    public GameObject[] _groundObstacles;
    public GameObject _balloon;

    private void Start()
    {
        _nextPickupPosition = Random.Range(15, 25);
        _nextGroundObstaclePosition = Random.Range(100, 1000);
        _nextBalloonPosition = Random.Range(20, 100);
    }

    private void Update()
    {
        ManagePickups();
        ManageObstacles();
        ManageBalloons();
    }

    void Spawn(GameObject[] objects, float yPosition, float xPosition)
    {
        Instantiate(objects[Random.Range(0, objects.Length)], new Vector2(transform.position.x + xPosition, yPosition), Quaternion.identity);
    }

    void ManagePickups()
    {
        if (transform.position.x > _nextPickupPosition)
        {
            Spawn(_pickups, Mathf.Max(transform.position.y + Random.Range(-5, 5), -2.2f), 20);
            _nextPickupPosition += Random.Range(Mathf.Clamp(2, _player.velocity.x * 0.35f, 30), Mathf.Clamp(30, _player.velocity.x * 1.5f, 100));
        }
    }

    void ManageObstacles()
    {
        if (transform.position.x > _nextGroundObstaclePosition)
        {
            Spawn(_groundObstacles, -4.18f, 100);
            _nextGroundObstaclePosition += Random.Range(30, 400);
        }
    }

    void ManageBalloons()
    {
        if (transform.position.x > _nextBalloonPosition)
        {
            Instantiate(_balloon, new Vector2(transform.position.x + 20, Mathf.Max(transform.position.y + Random.Range(-15, 5), -3.90f)), Quaternion.identity);
            _nextBalloonPosition += Random.Range(2, 100);
        }
    }
}
