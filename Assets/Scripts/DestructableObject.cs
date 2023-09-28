using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableObject : MonoBehaviour
{
    public float playerForce;
    public float force;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision force: " + collision.relativeVelocity.magnitude);
        if (collision.relativeVelocity.magnitude > playerForce && collision.gameObject.CompareTag("Player") || collision.relativeVelocity.magnitude > force)
        {
            Destroy(gameObject);
        }
    }
}
