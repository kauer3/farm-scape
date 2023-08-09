using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

public class Player : MonoBehaviour
{
    // private bool onSkateboard = false;
    private bool skatePickup = false;
    private bool bouncyPickup = false;
    private bool expellingEggs = false;
    private int eggCounter = 0;
    private float eggTimer = 0f;
    private float eggExpelDelay = .5f;
    public Rigidbody2D player;
    public SpriteRenderer playerSprite;
    public GameObject skateboard;
    public GameObject egg;
    public PhysicsMaterial2D defaultPhysics;
    public PhysicsMaterial2D bouncyPhysics;
    public PhysicsMaterial2D onSkatePhysics;
    private Rigidbody2D activeSkateboard = null;
    public float speed = 10f;
    // private bool animateRotation = false;
    // private float targetAngle;
    // private bool flapping = false;
    // public float rotationSpeed = 1000f;
    private bool launched = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Make player face direction of movement
    void Update()
    {
        // Debug.Log("Vertical velocity: " + player.velocity.y + ", Horizontal velocity: " + player.velocity.x + ", Magnitude: " + player.velocity.magnitude);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // animateRotation = true;
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

        if (eggCounter > 0)
        {
            if (eggTimer < eggExpelDelay)
            {
                eggTimer += Time.deltaTime;
                Debug.Log("Egg timer: " + eggTimer);
            }
            else
            {
                Debug.Log("Time to expel egg");
                ExpelEgg();
                eggCounter--;
                eggTimer = 0f;
            }
        }
    }

    void FixedUpdate()
    {
        if (!expellingEggs && activeSkateboard == null && player.velocity.magnitude > 0 && player.position.y > -3.91)
        {
            float angle = Mathf.Atan2(player.velocity.y, player.velocity.x) * Mathf.Rad2Deg;
            player.MoveRotation(angle + 5 * Time.fixedDeltaTime);
        }
    }

    // void FixedUpdate()
    // {
        // if (activeSkateboard == null && player.velocity.magnitude > 0 && player.position.y > -3.91)
        // {
            // if (animateRotation)
            // {
                // float increment = 150 * Time.fixedDeltaTime;
                // targetAngle = GetDirectionAngle();
                // AnimateRotateForward(targetAngle, increment);
                // animateRotation = false;
                // flapping = true;
            // }
            // else if (!flapping || player.rotation == targetAngle)
            // {
                // flapping = false;
                // RotateForwardInstantly();
            // }
        // }
    // }

    // private float GetDirectionAngle()
    // {
        // return Mathf.Atan2(player.velocity.y, player.velocity.x) * Mathf.Rad2Deg;
    // }

    private void AnimateRotateForward(float angle, float incrementSpeed)
    {
        player.MoveRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle), incrementSpeed));
    }

    // private void RotateForwardInstantly()
    // {
        // player.rotation = GetDirectionAngle();
    // }

    public void Launch(Vector2 direction)
    {
        player.AddRelativeForce(direction, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (bouncyPickup)
            {
                bouncyPickup = false;
                player.sharedMaterial = defaultPhysics;
                playerSprite.color = new Color(1, 1, 1, 1);
            }
            else if (player.rotation > -55 && player.rotation < 90 && player.velocity.magnitude >= 2)
            {
                if (skatePickup)
                {
                    player.velocity = new Vector2(player.velocity.magnitude, 3f);
                    InstantiateSkateboard();
                    skatePickup = false;
                    Debug.Log("Instantiated Skateboard!");
                }
            }
            else
            {
                GameOver();
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (player.velocity.magnitude < 0.1)
            {
                GameOver();
            }
            // else if (player.sharedMaterial == skatePhysics)
            // {
                // player.freezeRotation = true;
            // }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;
        if (obj.CompareTag("Skateboard"))
        {
            // onSkateboard = true;
            player.sharedMaterial = onSkatePhysics;
            activeSkateboard = collision.gameObject.GetComponent<Rigidbody2D>();
            playerSprite.color = new Color(0.2235294f, 0.4156863f, 0.5490196f, 0.75f);
            Debug.Log("Player hoped on skateboard!");
        }
        else
        {
            if (obj.CompareTag("Boost"))
            {
                player.AddRelativeForce(Vector2.right * 35f, ForceMode2D.Impulse);
                if (activeSkateboard)
                {
                    activeSkateboard.AddRelativeForce(Vector2.right * 35f, ForceMode2D.Impulse);
                }
            }
            else if (obj.CompareTag("Skate"))
            {
                skatePickup = true;
            }
            else if (obj.CompareTag("Bouncy"))
            {
                bouncyPickup = true;
                player.sharedMaterial = bouncyPhysics;
                playerSprite.color = new Color(0.6705883f, 0.254902f, 0.7372549f, 0.75f);
            }
            else if (obj.CompareTag("Egg Pickup"))
            {
                Debug.Log("Picked up egg!");
                eggCounter = 5;
            }

            Destroy(obj);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Skateboard"))
        {
            // onSkateboard = false;
            activeSkateboard = null;
            if (bouncyPickup)
            {
                player.sharedMaterial = bouncyPhysics;
                playerSprite.color = new Color(0.6705883f, 0.254902f, 0.7372549f, 0.75f);
            }
            else
            {
                player.sharedMaterial = defaultPhysics;
                playerSprite.color = new Color(1, 1, 1, 1);
            }
            // player.freezeRotation = false;
            Debug.Log("Player left skateboard!");
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Skateboard"))
        {
            if (player.velocity.magnitude < 0.1)
            {
                GameOver();
            }
            else if (player.rotation != 0)
            {
                AnimateRotateForward(0, 150 * Time.fixedDeltaTime);
            }
        }
    }

    void InstantiateSkateboard()
    {
        Vector2 skatePos = new Vector2(player.position.x, -4.46f);
        Vector2 skateVel = new Vector2(player.velocity.magnitude, 0);
        GameObject newSkateboard = Instantiate(skateboard, skatePos, Quaternion.identity);
        // set newSkateboard horizontal velocity to be the same as the player's
        newSkateboard.GetComponent<Rigidbody2D>().velocity = skateVel;
    }

    private void ExpelEgg()
    {
        Debug.Log("Egg expelled!");
        Vector3 position = new Vector3(-0.05f, -0.6f, 0);
        float angle = 45f;
        float force = 5f;
        angle *= Mathf.Deg2Rad;
        float xComponent = Mathf.Cos(angle * Mathf.PI / 180) * force;
        float yComponent = Mathf.Sin(angle * Mathf.PI / 180) * force;
        Vector2 forceApplied = new Vector2(xComponent, yComponent);

        player.AddForce(forceApplied);
        GameObject expelledEgg = Instantiate(egg, position, Quaternion.Euler(0, 0, 90), transform);
        expelledEgg.gameObject.GetComponent<Rigidbody2D>().AddRelativeForce(Vector2.up * 5f, ForceMode2D.Impulse);
    }

    void GameOver()
    {
        // player.velocity = Vector2.zero;
        // player.MoveRotation(0f);
        player.bodyType = RigidbodyType2D.Static;
        Debug.Log("Game Over!");
        EditorApplication.isPlaying = false;
    }
}
