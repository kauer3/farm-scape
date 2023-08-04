using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boost : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collidedObject)
    {
        collidedObject.GetComponent<Rigidbody2D>().AddRelativeForce(Vector2.right * 35f, ForceMode2D.Impulse);
    }
}
