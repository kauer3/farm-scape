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
    public CinemachineVirtualCamera _vcam;
    private CinemachineFramingTransposer _composer;
    private CinemachineBasicMultiChannelPerlin _noise;

    public TMP_Text _distanceIndicator;
    public TMP_Text _altituteIndicator;
    public Image _skateIndicator;
    public Image _springIndicator;
    private Vector2 _lastPosition;

    // private bool onSkateboard = false;
    private float _flapTimeLength = 0.05f;
    private bool _flapping = false;

    private float _skateBoostLength = 1;

    private bool _skatePickup = false;
    private bool _bouncyPickup = false;

    private bool _executingEggPropulsion = false;
    private float _eggExpelDelay = .35f;

    private float _rocketPropulsionTimeLength = 1.5f;

    private float _speedBoostTimeLength = .3f;
    private float _speedBoostTimer = .3f;
    // private float _speedBoost = 0.5f;

    private float _lastVelocityMagnitude = 0;
    private Vector2 _lastVelocity;

    public Rigidbody2D _player;
    private Rigidbody2D _activeSkateboard = null;
    private Rigidbody2D _instantiatedSkateboard = null;
    public GameObject _skateboard;
    public GameObject _egg;

    public ParticleSystem _psOnCollision;
    public ParticleSystem _psOnMovement;
    private ParticleSystem.MainModule _psOnCollisionMain;
    private ParticleSystem.MainModule _psOnMovementMain;

    public PhysicsMaterial2D _defaultPhysics;
    //public PhysicsMaterial2D bouncyPhysics;
    public PhysicsMaterial2D _onSkatePhysics;
    public float _speed;
    public float _drag;
    private bool _launched = false;

    void Awake()
    {
        _composer = _vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        _noise = _vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _psOnCollisionMain = _psOnCollision.main;
        _psOnMovementMain = _psOnMovement.main;
    }

    void Update()
    {
        ManageFlightInput();
        UpdatePositionIndicator();
        ManageCameraShake();
        
        // Zoom out camera when _player is moving fast
        //if (_player.velocity.magnitude > 20)
        //{
            //composer.m_CameraDistance = Mathf.Clamp(_player.velocity.magnitude / 10, 10, 20);
        //}
        //else if (composer.m_CameraDistance != 27)
        //{
            //composer.m_CameraDistance = 27;
        //}
    }

    void FixedUpdate()
    {
        //ManageSkate();
        ManageSpeedBoost();
        RotateTorwardsMovement();
        ManageParticlesOnMove();

        if ((_bouncyPickup || _skatePickup) && _player.position.y < -3.5)
        {
            _lastVelocity = _player.velocity;
        }

        // Create linear drag only on the horizonta axis
        //Vector2 drag = new Vector2(_player.velocity.x * -0.1f, 0);
        //_player.AddForce(drag, ForceMode2D.Force);
    }

    private void ManageFlightInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!_launched)
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
            //if (_player.velocity.y < 0)
            //{
                //_player.AddForce(direction, ForceMode2D.Force);
            //}
            //else
            //{
                //_player.AddRelativeForce(direction, ForceMode2D.Force);
            //}
            //Debug.Log("Going down");
        //}
    }

    private IEnumerator ExecuteSkateBoost(float velocity, Rigidbody2D skateboardRb)
    {
        float _timeElapsed = 0;
        float _horizontalForce;
        float _verticalForce = 0;
        bool _reachedVerticalPeak = false;

        while (_timeElapsed < _skateBoostLength)
        {
            float t = _timeElapsed / _skateBoostLength;
            _horizontalForce = Mathf.Lerp(0, velocity, t);

            if (!_reachedVerticalPeak)
            {
                if (_player.position.y < -3.5f)
                {
                    _verticalForce = Mathf.InverseLerp(-3.3f, -3.85f, _player.position.y) * velocity * 6;
                }
                else
                {
                    _verticalForce = 0;
                    _reachedVerticalPeak = true;
                }
            }

            _player.AddRelativeForce(new Vector2(_horizontalForce, _verticalForce), ForceMode2D.Force);
            skateboardRb.AddRelativeForce(Vector2.right * _horizontalForce * 0.4f, ForceMode2D.Force);
            _timeElapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    private void ManageSpeedBoost()
    {
        //float force;
        //Vector2 forceVector;
        if (_speedBoostTimer < _speedBoostTimeLength)
        {
            //force = Mathf.Clamp(5, (200 - _player.velocity.magnitude * 2) * rocketPropulsion, 300);
            //forceVector = new Vector2(force, 0);
            // Debug.Log("Force: " + force);
            Vector2 forceVector = Vector2.right * 500;
            _player.AddRelativeForce(forceVector, ForceMode2D.Force);
            if (_activeSkateboard)
            {
                _activeSkateboard.AddRelativeForce(forceVector * 0.4f, ForceMode2D.Force);
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
        _flapping = true;
        float _timeElapsed = 0;
        float _lerpedValue;
        float _flapStrength = _player.velocity.y < 0 ? _player.velocity.magnitude * 28 + _player.velocity.x * 8 : _player.velocity.x * 40;
        _flapStrength = Mathf.Min(_flapStrength, 1000);

        _player.drag = 3;
        while (_timeElapsed < _flapTimeLength)
        {
            float t = _timeElapsed / _flapTimeLength;
            _lerpedValue = Mathf.Lerp(_flapStrength, _flapStrength / 2, t);
            _player.drag = Mathf.SmoothStep(3, _drag, t);
            _player.AddRelativeForce(new Vector2(_player.velocity.x * 0.05f, _lerpedValue), ForceMode2D.Force);
            _timeElapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        _player.drag = _drag;
        _flapping = false;
    }

    private IEnumerator ExecuteRocketPropulsion()
    {
        float _timeElapsed = 0;
        float _timeByVelocity = 0;
        float _force;
        float _step;

        while (_timeByVelocity < _rocketPropulsionTimeLength)
        {
            _step = _timeByVelocity / _rocketPropulsionTimeLength;
            _force = Mathf.SmoothStep(500 - _player.velocity.magnitude * 4, 100, _step);
            _player.AddRelativeForce(Vector2.right * _force, ForceMode2D.Force);
            if (_activeSkateboard)
            {
                _activeSkateboard.AddRelativeForce(Vector2.right * _force * 0.4f, ForceMode2D.Force);
            }
            _timeElapsed += Time.fixedDeltaTime;
            _timeByVelocity = _timeElapsed + Mathf.InverseLerp(0, 100, _player.velocity.magnitude);
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
        while (_executingEggPropulsion)
        {
            yield return new WaitForSeconds(0.1f);
        }

        _executingEggPropulsion = true;
        for (int i = 0; i < 4; i++)
        {
            _player.AddForce(Vector2.right * 35, ForceMode2D.Impulse);
            ExpelEgg();
            yield return new WaitForSeconds(_eggExpelDelay);
        }
        _executingEggPropulsion = false;
    }

    private void RotateTorwardsMovement()
    {
        if (_activeSkateboard == null && _player.velocity.magnitude > 0)
        {
            float angle = Mathf.Atan2(_player.velocity.y, _player.velocity.x) * Mathf.Rad2Deg;
            _player.MoveRotation(angle + 5 * Time.fixedDeltaTime);
        }
    }

    private void AnimateRotateForward(float angle, float incrementSpeed)
    {
        _player.MoveRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle), incrementSpeed));
    }

    public void Launch(Vector2 direction)
    {
        _player.bodyType = RigidbodyType2D.Dynamic;
        _player.AddRelativeForce(direction, ForceMode2D.Impulse);
        //EmitParticles(100);
        _psOnMovement.Play();
        _launched = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (_bouncyPickup)
            {
                _bouncyPickup = false;
                _springIndicator.enabled = false;
                _player.velocity = new Vector2(Mathf.Abs(collision.relativeVelocity.x), Mathf.Abs(collision.relativeVelocity.y)) * 1.5f;
            }
            else if (_skatePickup)
            {
                InstantiateSkateboard((_lastVelocity.magnitude + _lastVelocity.x * 5) / 6);
                _skatePickup = false;
                _skateIndicator.enabled = false;
            }
            else if (_player.velocity.magnitude > 4)
            {
                PreventShortBouncing();
                if (!_flapping && _player.drag < 1.2f)
                {
                    _player.drag = 1.2f;
                }
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
            if (_player.velocity.magnitude < 0.1)
            {
                GameOver();
            }
            else
            {
                _player.drag = Mathf.MoveTowards(_player.drag, 2.5f, 0.1f * Time.fixedDeltaTime);
                //Debug.Log(_player.drag);
                PreventShortBouncing();
            }
            // else if (_player.sharedMaterial == skatePhysics)
            // {
                // _player.freezeRotation = true;
            // }
            //Debug.Log("Player on ground!");
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && !_flapping && _player.drag > _drag)
        {
            _player.drag = _drag;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;
        if (obj.CompareTag("Skateboard"))
        {
            _player.sharedMaterial = _onSkatePhysics;
            _activeSkateboard = collision.gameObject.GetComponent<Rigidbody2D>();
            float _newVelocity = Mathf.Max(_player.velocity.magnitude, _activeSkateboard.velocity.magnitude) * 1.1f;
            _activeSkateboard.velocity = new Vector2(_newVelocity, _activeSkateboard.velocity.y);
            _player.velocity = new Vector2(_newVelocity, _player.velocity.y);
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
                _skatePickup = true;
                _skateIndicator.enabled = true;
            }
            else if (obj.CompareTag("Bouncy"))
            {
                _bouncyPickup = true;
                _springIndicator.enabled = true;
                //_player.sharedMaterial = bouncyPhysics;
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
            // if (_activeSkateboard)
            // {
            _activeSkateboard = null;
            // }
            // if (_instantiatedSkateboard)
            // {
            // _instantiatedSkateboard = null;
            // }
            //check collision gameobject's velocity
            Rigidbody2D collisionRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (collisionRb.velocity.x > _player.velocity.x)
            {
                _player.velocity = new Vector2(collisionRb.velocity.magnitude, _player.velocity.y);
            }
            else
            {
                _player.sharedMaterial = _defaultPhysics;
            }
            // _player.freezeRotation = false;
            // Debug.Log("Player left skateboard!");
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Skateboard"))
        {
            if (_player.velocity.magnitude < 0.1)
            {
                GameOver();
            }
            else if (_player.rotation != 0)
            {
                AnimateRotateForward(0, 150 * Time.fixedDeltaTime);
            }
        }
    }

    void PreventShortBouncing()
    {
        //Debug.Log("Velocity Y: " + _player.velocity.y);
        if (Mathf.Abs(_player.velocity.y) < 5)
        {
            _player.velocity = new Vector2(_player.velocity.x, 0);
        }
    }

    void InstantiateSkateboard(float velocity)
    {
        _player.velocity = new Vector2(velocity, 0);
        Vector2 skatePos = new Vector2(_player.position.x, -4.46f);
        Vector2 skateVel = new Vector2(velocity, 0);
        GameObject newSkateboard = Instantiate(_skateboard, skatePos, Quaternion.identity);
        _instantiatedSkateboard = newSkateboard.GetComponent<Rigidbody2D>();
        _instantiatedSkateboard.velocity = skateVel;
        StartCoroutine(ExecuteSkateBoost((velocity + 100) * 0.35f, _instantiatedSkateboard));
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

        // _player.AddForce(Vector2.up * force, ForceMode2D.Force);

        GameObject expelledEgg = Instantiate(_egg, transform.position + position, transform.rotation * Quaternion.Euler(0, 0, 90));
        // Debug.Log(expelledEgg.transform.rotation);
        expelledEgg.GetComponent<Rigidbody2D>().AddRelativeForce(Vector2.up * 4f, ForceMode2D.Impulse);
    }

    void ManageCameraShake()
    {
        // Debug.Log("Vertical velocity: " + _player.velocity.y + ", Horizontal velocity: " + _player.velocity.x + ", Magnitude: " + _player.velocity.magnitude);
        if (_player.velocity.magnitude > 20)
        {
            float noiseIntensity = Mathf.Clamp(_player.velocity.magnitude / 20 - 1, 0, 2.5f);
            if (Mathf.Abs(_noise.m_AmplitudeGain - noiseIntensity) > 0.1)
            {
                _noise.m_AmplitudeGain = noiseIntensity;
                // Debug.Log("Noise Intensity: " + noiseIntensity);
            }
        }
        else if (_noise.m_AmplitudeGain > 0)
        {
            _noise.m_AmplitudeGain = 0;
        }
    }

    private void EmitParticles(float force)
    {
        _psOnCollisionMain.startSpeedMultiplier = force;
        _psOnCollision.Emit(Mathf.RoundToInt(force * 0.3f));
    }

    private void ManageParticlesOnMove()
    {
        if (_player.velocity.magnitude > _lastVelocityMagnitude + 3 || _player.velocity.magnitude < _lastVelocityMagnitude - 3)
        {
            ParticleSystem.EmissionModule emission = _psOnMovement.emission;
            emission.rateOverTime = _player.velocity.magnitude * 0.002f;
            _lastVelocityMagnitude = _player.velocity.magnitude;
            //Debug.Log("Emission rate: " + emission.rateOverTime.constant + ", new speed: " + lastSpeed);
        }
    }

    void UpdatePositionIndicator()
    {
        Vector2 newPosition = Vector2Int.RoundToInt(new Vector2(transform.position.x + 3.25f, transform.position.y + 3.86f) / 4);
        if (newPosition != _lastPosition)
        {
            _lastPosition = newPosition;
            _distanceIndicator.text = "Distance: " + newPosition.x + "m";
            _altituteIndicator.text = "Altitude: " + newPosition.y + "m";
        }
    }

    void GameOver()
    {
        // _player.velocity = Vector2.zero;
        // _player.MoveRotation(0f);
        _player.bodyType = RigidbodyType2D.Static;
        _psOnMovement.Stop();
        Debug.Log("Game Over!");
        // EditorApplication.isPlaying = false;
    }
}
