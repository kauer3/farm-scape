using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using Cinemachine;

public class Player : MonoBehaviour
{
    public CinemachineVirtualCamera vcam;
    private CinemachineFramingTransposer composer;
    private CinemachineBasicMultiChannelPerlin noise;

    public TMP_Text distanceIndicator;
    public TMP_Text altitudeIndicator;
    private Vector2 position;

    // private bool onSkateboard = false;
    private float flapTimeLength = 0.05f;
    private float flapTimer = 0.05f;
    private Vector2 flapDirection;
    private bool flapping = false;

    private float skateBoostLength = 1;
    private float skateBoostTimer = 1;
    private Vector2 skateBoost;

    private bool skatePickup = false;
    private bool bouncyPickup = false;

    private bool executingEggPropulsion = false;
    //private int eggCounter;
    //private float eggTimer = 0f;
    private float eggExpelDelay = .35f;
    //private float eggPropulsionTimer = 1.25f;
    //private float eggPropulsionTimeLength = 1.25f;

    private float rocketPropulsionTimeLength = 1.5f;

    private float speedBoostTimeLength = .3f;
    private float speedBoostTimer = .3f;
    // private float speedBoost = 0.5f;

    private float lastSpeed = 0;

    public Rigidbody2D player;
    private Rigidbody2D activeSkateboard = null;
    private Rigidbody2D instantiatedSkateboard = null;
    public SpriteRenderer playerSprite;
    public GameObject skateboard;
    public GameObject egg;

    [SerializeField] ParticleSystem psOnCollision;
    [SerializeField] ParticleSystem psOnMovement;
    private ParticleSystem.MainModule psOnCollisionMain;
    private ParticleSystem.MainModule psOnMovementMain;

    public PhysicsMaterial2D defaultPhysics;
    public PhysicsMaterial2D bouncyPhysics;
    public PhysicsMaterial2D onSkatePhysics;
    private float speed = 200;
    private bool launched = false;

    void Awake()
    {
        composer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        psOnCollisionMain = psOnCollision.main;
        psOnMovementMain = psOnMovement.main;
    }

    void Update()
    {
        ManageFlightInput();
        UpdatePositionIndicator();
        //ManageCameraShake();
        
        // Zoom out camera when player is moving fast
        //if (player.velocity.magnitude > 20)
        //{
            //composer.m_CameraDistance = Mathf.Clamp(player.velocity.magnitude / 10, 10, 20);
        //}
        //else if (composer.m_CameraDistance != 27)
        //{
            //composer.m_CameraDistance = 27;
        //}
    }

    void FixedUpdate()
    {
        //ManageFlight();
        ManageSkate();
        ManageSpeedBoost();
        //ManageEggPropulsion();
        RotateTorwardsMovement();
        ManageParticlesOnMove();

        // Create linear drag only on the horizonta axis
        //Vector2 drag = new Vector2(player.velocity.x * -0.1f, 0);
        //player.AddForce(drag, ForceMode2D.Force);
    }

    private void ManageFlightInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!launched)
            {
                Launch(Vector2.right * speed);
            }
            else
            {
                //float rawFlapStrength = player.velocity.y < 0 ? player.velocity.magnitude * 17.5f + player.velocity.x * 5 : player.velocity.x * 25;
                //float clippedFlapStrength = Mathf.Min(rawFlapStrength, 500f);
                //flapDirection = Vector2.up * clippedFlapStrength;
                //flapTimer = 0f;
                StopCoroutine(ExecuteFlap());
                StartCoroutine(ExecuteFlap());
                // Debug.Log("Flap direction: " + flapDirection);
            }
        }
        //else if (Input.GetKey(KeyCode.LeftShift))
        //{
            //flapTimer = flapTimeLength;
            //Vector2 direction = new Vector2(0, -50) * Time.fixedDeltaTime;
            //if (player.velocity.y < 0)
            //{
                //player.AddForce(direction, ForceMode2D.Force);
            //}
            //else
            //{
                //player.AddRelativeForce(direction, ForceMode2D.Force);
            //}
            //Debug.Log("Going down");
        //}
    }

    private void ManageFlight()
    {
        if (flapTimer < flapTimeLength)
        {
            player.AddRelativeForce(flapDirection, ForceMode2D.Force);
            flapTimer += Time.deltaTime;
        }
    }

    private void ManageSkate()
    {
        if (skateBoostTimer < skateBoostLength)
        {
            Vector2 boost = skateBoost * (skateBoostLength - skateBoostTimer);
            player.AddForce(boost * 1.3f, ForceMode2D.Force);
            instantiatedSkateboard.AddForce(boost, ForceMode2D.Force);
            skateBoostTimer += Time.deltaTime;
        }
        else if (instantiatedSkateboard)
        {
            instantiatedSkateboard = null;
        }
    }

    private void ManageSpeedBoost()
    {
        //float force;
        //Vector2 forceVector;
        if (speedBoostTimer < speedBoostTimeLength)
        {
            //force = Mathf.Clamp(5, (200 - player.velocity.magnitude * 2) * rocketPropulsion, 300);
            //forceVector = new Vector2(force, 0);
            // Debug.Log("Force: " + force);
            Vector2 forceVector = Vector2.right * 500;
            player.AddRelativeForce(forceVector, ForceMode2D.Force);
            if (activeSkateboard)
            {
                activeSkateboard.AddRelativeForce(forceVector, ForceMode2D.Force);
            }
            speedBoostTimer += Time.deltaTime;
            //rocketPropulsion += Time.deltaTime;
            //Debug.Log(speedBoostTimer);
        }
        //else
        //{
            //rocketPropulsion = 0.5f;
        //}
    }

    private IEnumerator ExecuteFlap()
    {
        float timeElapsed = 0;
        float lerpedValue;
        float horizontalVelocity = Mathf.InverseLerp(0, player.velocity.magnitude, player.velocity.x); 
        float rawFlapStrength = player.velocity.y < 0 ? player.velocity.magnitude * 28 + player.velocity.x * 8 : player.velocity.x * 40;
        float clippedFlapStrength = Mathf.Min(rawFlapStrength, 1000);
        //Debug.Log("Horizontal velocity: " + horizontalVelocity);
        Debug.Log("Clipped flap strength: " + clippedFlapStrength);

        flapping = true;
        player.drag = 2;
        while (timeElapsed < flapTimeLength)
        {
            float t = timeElapsed / flapTimeLength;
            lerpedValue = Mathf.Lerp(clippedFlapStrength, clippedFlapStrength / 2, t);
            player.drag = Mathf.SmoothStep(2, 0.1f, t);
            player.AddRelativeForce(new Vector2(player.velocity.x * 0.05f, lerpedValue), ForceMode2D.Force);
            timeElapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        player.drag = 0.1f;
        flapping = false;
    }

    private IEnumerator ExecuteRocketPropulsion()
    {
        float timeElapsed = 0;
        float timeAndVelocity = 0;
        float lerpedValue;
        int iterations = 0;

        while (timeAndVelocity < rocketPropulsionTimeLength)
        {
            float t = timeAndVelocity / rocketPropulsionTimeLength;
            lerpedValue = Mathf.SmoothStep(500 - player.velocity.magnitude * 4, 100, t);
            //Debug.Log("Lerped value: " + lerpedValue);
            player.AddRelativeForce(Vector2.right * lerpedValue, ForceMode2D.Force);
            if (activeSkateboard)
            {
                activeSkateboard.AddRelativeForce(Vector2.right * lerpedValue, ForceMode2D.Force);
            }
            timeElapsed += Time.fixedDeltaTime;
            timeAndVelocity = timeElapsed + Mathf.InverseLerp(0, 100, player.velocity.magnitude);
            iterations++;
            yield return new WaitForFixedUpdate();
        }
        //Debug.Log("Iterations: " + iterations);
    }

    //private IEnumerator ExecuteSpeedBoostPropulsion()
    //{
        //float time = 0;
        //while (time < speedBoostTimeLength)
        //{

            //time += Time.fixedDeltaTime;
            //yield return null;
        //}
    //}

    private IEnumerator ExecuteEggPropulsion()
    {
        while (executingEggPropulsion)
        {
            yield return new WaitForSeconds(0.1f);
        }

        executingEggPropulsion = true;
        for (int i = 0; i < 4; i++)
        {
            player.AddForce(Vector2.right * 35, ForceMode2D.Impulse);
            ExpelEgg();
            yield return new WaitForSeconds(eggExpelDelay);
        }
        executingEggPropulsion = false;
    }

    private void RotateTorwardsMovement()
    {
        if (activeSkateboard == null && player.velocity.magnitude > 0 && player.position.y > -3.91)
        {
            float angle = Mathf.Atan2(player.velocity.y, player.velocity.x) * Mathf.Rad2Deg;
            player.MoveRotation(angle + 5 * Time.fixedDeltaTime);
        }
    }

    private void AnimateRotateForward(float angle, float incrementSpeed)
    {
        player.MoveRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle), incrementSpeed));
    }

    public void Launch(Vector2 direction)
    {
        player.bodyType = RigidbodyType2D.Dynamic;
        player.AddRelativeForce(direction, ForceMode2D.Impulse);
        //EmitParticles(100);
        psOnMovement.Play();
        launched = true;
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
                //Vector2 force = Vector2.right * player.velocity.x * 20;
                //player.AddForce(force, ForceMode2D.Impulse);
                // Check velocity after bounce
            }
            else if (player.rotation > -55 && player.rotation < 90 && player.velocity.magnitude >= 5)
            {
                Debug.Log("Player touched ground!");
                if (skatePickup)
                {
                    InstantiateSkateboard(player.velocity.magnitude);
                    skatePickup = false;
                }
                else if (!flapping && player.drag < 1)
                {
                    player.drag = 1;
                }
            }
            else
            {
                GameOver();
            }
        }
        EmitParticles(collision.gameObject.CompareTag("Balloon") ? collision.relativeVelocity.magnitude * 0.3f : collision.relativeVelocity.magnitude);

        // Check if collision was horizontal
        //if (Mathf.Abs(collision.relativeVelocity.x) > Mathf.Abs(collision.relativeVelocity.y))
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
            Debug.Log("Player on ground!");
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && !flapping && player.drag > 0.1f)
        {
            player.drag = 0.1f;
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
            // check is player or skateboard is faster
            // add veloctity difference as force to slowest object
            float speedDifference = Mathf.Abs(player.velocity.x - activeSkateboard.velocity.x);
            if (player.velocity.magnitude > activeSkateboard.velocity.magnitude)
            {
                activeSkateboard.velocity = new Vector2(player.velocity.magnitude + speedDifference, activeSkateboard.velocity.y);
            }
            else
            {
                player.velocity = new Vector2(activeSkateboard.velocity.magnitude + speedDifference, player.velocity.y);
            }
        }
        else
        {
            if (obj.CompareTag("Boost"))
            {
                StartCoroutine(ExecuteRocketPropulsion());
                Destroy(obj.transform.parent.gameObject);
                return;
            }
            else if (obj.CompareTag("Speed Boost"))
            {
                speedBoostTimer = 0;
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
                // eggCounter = 0;
                // ExpelEgg();
                // eggPropulsionTimer = 0;
                // Start Egg Propulsion Coroutine
                StartCoroutine(ExecuteEggPropulsion());
            }

            Destroy(obj);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Skateboard"))
        {
            // onSkateboard = false;
            // if (activeSkateboard)
            // {
            activeSkateboard = null;
            // }
            // if (instantiatedSkateboard)
            // {
            // instantiatedSkateboard = null;
            // }
            //check collision gameobject's velocity
            Rigidbody2D collisionRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (collisionRb.velocity.x > player.velocity.x)
            {
                player.velocity = new Vector2(collisionRb.velocity.magnitude, player.velocity.y);
            }
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
            // Debug.Log("Player left skateboard!");
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

    void InstantiateSkateboard(float velocity)
    {
        player.velocity = new Vector2(velocity, 3f);
        Vector2 skatePos = new Vector2(player.position.x, -4.46f);
        Vector2 skateVel = new Vector2(velocity, 0);
        GameObject newSkateboard = Instantiate(skateboard, skatePos, Quaternion.identity);
        instantiatedSkateboard = newSkateboard.GetComponent<Rigidbody2D>();
        instantiatedSkateboard.velocity = skateVel;
        skateBoostTimer = 0;
        skateBoost = new Vector2((velocity + 100) * 0.35f, 0);
    }

    private void ExpelEgg()
    {
        // Debug.Log(transform.rotation[2]);
        Vector3 position = new Vector3(-0.05f, -0.35f, 0);
        // float force = 5f;
        // float angle = 90f * Mathf.Deg2Rad;
        // float xComponent = Mathf.Cos(angle) * force;
        // float yComponent = Mathf.Sin(angle) * force;
        // Vector2 forceApplied = new Vector2(xComponent, yComponent);

        // player.AddForce(Vector2.up * force, ForceMode2D.Force);

        GameObject expelledEgg = Instantiate(egg, transform.position + position, transform.rotation * Quaternion.Euler(0, 0, 90));
        // Debug.Log(expelledEgg.transform.rotation);
        expelledEgg.GetComponent<Rigidbody2D>().AddRelativeForce(Vector2.up * 4f, ForceMode2D.Impulse);
    }

    void ManageCameraShake()
    {
        // Debug.Log("Vertical velocity: " + player.velocity.y + ", Horizontal velocity: " + player.velocity.x + ", Magnitude: " + player.velocity.magnitude);
        if (player.velocity.magnitude > 20)
        {
            float noiseIntensity = Mathf.Clamp(player.velocity.magnitude / 20 - 1, 0, 2.5f);
            if (Mathf.Abs(noise.m_AmplitudeGain - noiseIntensity) > 0.1)
            {
                noise.m_AmplitudeGain = noiseIntensity;
                // Debug.Log("Noise Intensity: " + noiseIntensity);
            }
        }
        else if (noise.m_AmplitudeGain > 0)
        {
            noise.m_AmplitudeGain = 0;
        }
    }

    private void EmitParticles(float force)
    {
        psOnCollisionMain.startSpeedMultiplier = force;
        psOnCollision.Emit(Mathf.RoundToInt(force * 0.3f));
    }

    private void ManageParticlesOnMove()
    {
        if (player.velocity.magnitude > lastSpeed + 3 || player.velocity.magnitude < lastSpeed - 3)
        {
            ParticleSystem.EmissionModule emission = psOnMovement.emission;
            emission.rateOverTime = player.velocity.magnitude * 0.002f;
            lastSpeed = player.velocity.magnitude;
            //Debug.Log("Emission rate: " + emission.rateOverTime.constant + ", new speed: " + lastSpeed);
        }
    }

    void UpdatePositionIndicator()
    {
        Vector2 newPosition = Vector2Int.RoundToInt(new Vector2(transform.position.x + 3.25f, transform.position.y + 3.86f) / 4);
        if (newPosition != position)
        {
            position = newPosition;
            distanceIndicator.text = "Distance: " + newPosition.x + "m";
            altitudeIndicator.text = "Altitude: " + newPosition.y + "m";
        }
    }

    void GameOver()
    {
        // player.velocity = Vector2.zero;
        // player.MoveRotation(0f);
        player.bodyType = RigidbodyType2D.Static;
        psOnMovement.Stop();
        Debug.Log("Game Over!");
        // EditorApplication.isPlaying = false;
    }
}
