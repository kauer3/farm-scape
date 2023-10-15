using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public CinemachineVirtualCamera vcam;
    private CinemachineFramingTransposer composer;
    private CinemachineBasicMultiChannelPerlin noise;

    public TMP_Text distanceIndicator;
    public TMP_Text altitudeIndicator;
    public Image skateIndicator;
    public Image springIndicator;
    private Vector2 position;

    // private bool onSkateboard = false;
    private float flapTimeLength = 0.05f;
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

    private float _speedBoostTimeLength = .3f;
    private float _speedBoostTimer = .3f;
    // private float _speedBoost = 0.5f;

    private float lastVelocityMagnitude = 0;
    private Vector2 lastVelocity;

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
    //public PhysicsMaterial2D bouncyPhysics;
    public PhysicsMaterial2D onSkatePhysics;
    public float _speed;
    public float _drag;
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
        ManageSkate();
        ManageSpeedBoost();
        RotateTorwardsMovement();
        ManageParticlesOnMove();

        if ((bouncyPickup || skatePickup) && player.position.y < -3.5)
        {
            lastVelocity = player.velocity;
        }

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
                Launch(Vector2.right * _speed);
            }
            else
            {
                StopCoroutine(ExecuteFlap());
                StartCoroutine(ExecuteFlap());
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

    private IEnumerator ExecuteSkateBoost(float velocity)
    {
        float timeElapsed = 0;
        float lerpedValue;
        float flapStrength = player.velocity.y < 0 ? player.velocity.magnitude * 28 + player.velocity.x * 8 : player.velocity.x * 40;
        flapStrength = Mathf.Min(flapStrength, 1000);

        while (timeElapsed < flapTimeLength)
        {
            float t = timeElapsed / flapTimeLength;
            lerpedValue = Mathf.Lerp(flapStrength, flapStrength / 2, t);
            player.drag = Mathf.SmoothStep(3, _drag, t);
            player.AddRelativeForce(new Vector2(player.velocity.x * 0.05f, lerpedValue), ForceMode2D.Force);
            timeElapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
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
        if (_speedBoostTimer < _speedBoostTimeLength)
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
            _speedBoostTimer += Time.deltaTime;
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
        flapping = true;
        float timeElapsed = 0;
        float lerpedValue;
        float flapStrength = player.velocity.y < 0 ? player.velocity.magnitude * 28 + player.velocity.x * 8 : player.velocity.x * 40;
        flapStrength = Mathf.Min(flapStrength, 1000);

        player.drag = 3;
        while (timeElapsed < flapTimeLength)
        {
            float t = timeElapsed / flapTimeLength;
            lerpedValue = Mathf.Lerp(flapStrength, flapStrength / 2, t);
            player.drag = Mathf.SmoothStep(3, _drag, t);
            player.AddRelativeForce(new Vector2(player.velocity.x * 0.05f, lerpedValue), ForceMode2D.Force);
            timeElapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        player.drag = _drag;
        flapping = false;
    }

    private IEnumerator ExecuteRocketPropulsion()
    {
        float _timeElapsed = 0;
        float _timeByVelocity = 0;
        float _force;
        float _step;

        while (_timeByVelocity < rocketPropulsionTimeLength)
        {
            _step = _timeByVelocity / rocketPropulsionTimeLength;
            _force = Mathf.SmoothStep(500 - player.velocity.magnitude * 4, 100, _step);
            player.AddRelativeForce(Vector2.right * _force, ForceMode2D.Force);
            if (activeSkateboard)
            {
                activeSkateboard.AddRelativeForce(Vector2.right * _force, ForceMode2D.Force);
            }
            _timeElapsed += Time.fixedDeltaTime;
            _timeByVelocity = _timeElapsed + Mathf.InverseLerp(0, 100, player.velocity.magnitude);
            yield return new WaitForFixedUpdate();
        }
    }

    //private IEnumerator ExecuteSpeedBoostPropulsion()
    //{
        //float time = 0;
        //while (time < _speedBoostTimeLength)
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
        if (activeSkateboard == null && player.velocity.magnitude > 0)
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
                springIndicator.enabled = false;
                player.velocity = new Vector2(Mathf.Abs(collision.relativeVelocity.x), Mathf.Abs(collision.relativeVelocity.y)) * 1.5f;
            }
            else if (skatePickup)
            {
                InstantiateSkateboard((lastVelocity.magnitude + lastVelocity.x * 5) / 6);
                skatePickup = false;
                skateIndicator.enabled = false;
            }
            else if (player.velocity.magnitude >= 2 && !flapping && player.drag < 1.2f)
            {
                Debug.Log("Increasing drag!");
                player.drag = 1.2f;
            }
            else
            {
                GameOver();
            }
        }
        EmitParticles(collision.gameObject.CompareTag("Balloon") ? collision.relativeVelocity.magnitude * 0.3f : collision.relativeVelocity.magnitude);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (player.velocity.magnitude < 0.1)
            {
                GameOver();
            }
            else
            {
                player.drag = Mathf.MoveTowards(player.drag, 2.5f, 0.1f * Time.fixedDeltaTime);

                Debug.Log(player.drag);
            }
            // else if (player.sharedMaterial == skatePhysics)
            // {
                // player.freezeRotation = true;
            // }
            //Debug.Log("Player on ground!");
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && !flapping && player.drag > _drag)
        {
            player.drag = _drag;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;
        if (obj.CompareTag("Skateboard"))
        {
            player.sharedMaterial = onSkatePhysics;
            activeSkateboard = collision.gameObject.GetComponent<Rigidbody2D>();
            float _newVelocity = Mathf.Max(player.velocity.magnitude, activeSkateboard.velocity.magnitude) * 1.1f;
            activeSkateboard.velocity = new Vector2(_newVelocity, activeSkateboard.velocity.y);
            player.velocity = new Vector2(_newVelocity, player.velocity.y);
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
                _speedBoostTimer = 0;
            }
            else if (obj.CompareTag("Skate"))
            {
                skatePickup = true;
                skateIndicator.enabled = true;
            }
            else if (obj.CompareTag("Bouncy"))
            {
                bouncyPickup = true;
                springIndicator.enabled = true;
                //player.sharedMaterial = bouncyPhysics;
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
            else
            {
                player.sharedMaterial = defaultPhysics;
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
        player.velocity = new Vector2(velocity + 3, 3);
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
        if (player.velocity.magnitude > lastVelocityMagnitude + 3 || player.velocity.magnitude < lastVelocityMagnitude - 3)
        {
            ParticleSystem.EmissionModule emission = psOnMovement.emission;
            emission.rateOverTime = player.velocity.magnitude * 0.002f;
            lastVelocityMagnitude = player.velocity.magnitude;
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
