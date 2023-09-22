using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTracker : MonoBehaviour
{
    public Transform player;

    void Update()
    {
        transform.position = new Vector3(Mathf.Max(player.position.x + 4f, 0), Mathf.Max(player.position.y, 0), -10);
    }
}
