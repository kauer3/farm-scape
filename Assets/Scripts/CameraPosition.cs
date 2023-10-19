using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    public Transform _player;

    void Update()
    {
        if (transform.position.x < 22 || transform.position.y < 13)
        {
            transform.position = new Vector3(Mathf.Max(_player.position.x, 21), Mathf.Max(_player.position.y, 12.5f), 0);
        }
        else
        {
            transform.position = new Vector3(_player.position.x, _player.position.y, 0);
        }
    }
}
