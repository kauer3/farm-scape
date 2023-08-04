using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Rigidbody2D player;
    public float speed = 10f;
    // public float rotationSpeed = 1000f;
    private bool launched = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Make player face direction of movement
    void Update()
    {

    }

    void FixedUpdate()
    {
        //Debug.Log("Vertical velocity: " + player.velocity.y + ", Horizontal velocity: " + player.velocity.x + ", Magnitude: " + player.velocity.magnitude);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!launched)
            {
                player.bodyType = RigidbodyType2D.Dynamic;
                Launch(Vector2.right * speed);
                launched = true;
            }
            else
            {
                //float Vel = (player.velocity.x + player.velocity.y)/ 2;
                //Vector2 upDiagonal = new Vector2(player.velocity.magnitude, player.velocity.magnitude).normalized;
                // Debug.Log("velX: " + player.velocity.x + ", velY: " + player.velocity.y + ", vel: " + Vel + ", normalized: " + upDiagonal);
                // Debug.Log("upDiagonal: " + upDiagonal);

                //Launch(Vector2.up * Mathf.Clamp(player.velocity.x / 4, 0f, 7f));
                float rawFlapStrength = player.velocity.y < 0 ? player.velocity.magnitude : player.velocity.x;
                float clippedFlapStrength = Mathf.Min(rawFlapStrength / 4, 7f);
                Launch(Vector2.up * clippedFlapStrength);
            }
        }

        if (player.velocity != Vector2.zero)
        {
            // rotate the player to face the direction it is moving
            float angle = Mathf.Atan2(player.velocity.y, player.velocity.x) * Mathf.Rad2Deg;
            player.MoveRotation(angle);

        }
    }

    // launch player in direction of (0, -0.64)
    public void Launch(Vector2 direction)
    {
        player.AddRelativeForce(direction, ForceMode2D.Impulse);
        //Debug.Log("force up: " + direction);
    }

    // get the horizontal velocity when colliding with the ground
    void OnCollisionEnter2D(Collision2D collision)
    {
        // if (collision.gameObject.CompareTag("Ground"))
        // {
            // if (player.rotation > -50 && player.rotation < 90)
            // {
                // player.velocity = new Vector2(player.velocity.x, 0);
            // }
            // else
            // {
                // // player.velocity = Vector2.zero;
                // player.MoveRotation(0f);
                // player.bodyType = RigidbodyType2D.Static;
            // }
        // }
    }
}
