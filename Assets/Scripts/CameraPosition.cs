using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    public Transform player;

    void Update()
    {
        if (transform.position.y < 4.7 || transform.position.x < 8.8)
        {
            transform.position = new Vector3(Mathf.Max(player.position.x, 8.8f), Mathf.Max(player.position.y, 4.7f), 0);
        }
        else
        {
            transform.position = new Vector3(player.position.x, player.position.y, 0);
        }
    }
}
