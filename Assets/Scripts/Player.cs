using System.Collections;
using TMPro;
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
    private float _distance;

    private float _flapTimeLength = 0.05f;
    private bool _flapping = false;
    public AudioSource cluckSound;
    public AudioSource cannonSound;
    public AudioSource boostSound;
    public AudioSource fireworkSound;
    public AudioSource eggSound;
    public AudioSource eggPopSound;
    public AudioSource haySound;

    private float _skateBoostLength = 1;

    private bool _skatePickup = false;
    private bool _bouncyPickup = false;

    private bool _executingEggPropulsion = false;
    private float _eggExpelDelay = .35f;

    private float _rocketPropulsionTimeLength = 1.5f;

    private float _speedBoostTimeLength = .3f;

    private float _lastVelocityMagnitude = 0;
    private Vector2 _lastVelocity;

    public Rigidbody2D _player;
    private Rigidbody2D _activeSkateboard = null;
    private Rigidbody2D _instantiatedSkateboard = null;
    public GameObject _skateboard;
    public GameObject _egg;
    public GameObject _startElement;

    public ParticleSystem _psOnCollision;
    public ParticleSystem _psOnMovement;
    private ParticleSystem.MainModule _psOnCollisionMain;
    private ParticleSystem.MainModule _psOnMovementMain;

    public PhysicsMaterial2D _defaultPhysics;
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
        //ManageCameraShake();
        
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
        RotateTorwardsMovement();
        ManageParticlesOnMove();
        StoreVelocity();
    }

    private IEnumerator ExecuteFlap()
    {
        _flapping = true;
        float _timeElapsed = 0;
        float _lerpedValue;
        float _flapStrength = _player.velocity.y < 0 ? _player.velocity.magnitude * 28 + _player.velocity.x * 8 : _player.velocity.x * 40;
        _flapStrength = Mathf.Min(_flapStrength, 1000);
        //float dragOnFlap = 2.5f + Mathf.InverseLerp(300, 0, _flapStrength) * 2.5f;

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
                if (_player.position.y < -3.6f)
                {
                    _verticalForce = Mathf.InverseLerp(-3.5f, -3.85f, _player.position.y) * (velocity + 50) * 6;
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

    private IEnumerator ExecuteSpeedBoost()
    {
        float _timeElapsed = 0;
        float _force;
        float _step;
        Vector2 _forceVector;

        while (_timeElapsed < _speedBoostTimeLength)
        {
            _step = _timeElapsed / _rocketPropulsionTimeLength;
            _force = Mathf.Lerp(1000, 0, _step);
            _forceVector = Vector2.right * _force;
            _player.AddRelativeForce(_forceVector, ForceMode2D.Force);
            if (_activeSkateboard)
            {
                BoostSkateboard(_activeSkateboard, _forceVector);
            }
            else if (_instantiatedSkateboard)
            {
                BoostSkateboard(_instantiatedSkateboard, _forceVector);
            }
            _timeElapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator ExecuteRocketPropulsion()
    {
        float _timeElapsed = 0;
        float _timeByVelocity = 0;
        float _force;
        float _step;
        Vector2 _forceVector;

        while (_timeByVelocity < _rocketPropulsionTimeLength)
        {
            _step = _timeByVelocity / _rocketPropulsionTimeLength;
            _force = Mathf.SmoothStep(500 - _player.velocity.magnitude * 4, 100, _step);
            _forceVector = Vector2.right * _force;
            _player.AddRelativeForce(_forceVector, ForceMode2D.Force);
            if (_activeSkateboard)
            {
                BoostSkateboard(_activeSkateboard, _forceVector);
            }
            else if (_instantiatedSkateboard)
            {
                BoostSkateboard(_instantiatedSkateboard, _forceVector);
            }
            _timeElapsed += Time.fixedDeltaTime;
            _timeByVelocity = _timeElapsed + Mathf.InverseLerp(0, 100, _player.velocity.magnitude);
            yield return new WaitForFixedUpdate();
        }
    }

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
            eggPopSound.Play();
            yield return new WaitForSeconds(_eggExpelDelay);
        }
        _executingEggPropulsion = false;
    }

    private void ManageFlightInput()
    {
        cluckSound = GetComponent<AudioSource>();
        AudioClip cluck = cluckSound.clip;
        cannonSound = GameObject.Find("Canon").GetComponent<AudioSource>();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!_launched)
            {
                Launch(Vector2.right * _speed);

                cannonSound.Play();
            }
            else
            {
                StopCoroutine(ExecuteFlap());
                StartCoroutine(ExecuteFlap());

                cluckSound.PlayOneShot(cluck);
            }
        }
    }

    public void Launch(Vector2 direction)
    {
        _player.bodyType = RigidbodyType2D.Dynamic;
        _player.AddRelativeForce(direction, ForceMode2D.Impulse);
        //EmitParticles(100);
        _psOnMovement.Play();
        _startElement.SetActive(false);
        _launched = true;
    }

    private void BoostSkateboard(Rigidbody2D _skateboardRb, Vector2 _forceVector)
    {
          _skateboardRb.AddRelativeForce(_forceVector * 0.4f, ForceMode2D.Force);
    }

    private void RotateTorwardsMovement()
    {
        if (_activeSkateboard == null && _player.velocity.magnitude > 0)
        {
            float _angle = Mathf.Atan2(_player.velocity.y, _player.velocity.x) * Mathf.Rad2Deg;
            _player.MoveRotation(_angle + 5 * Time.fixedDeltaTime);
        }
    }

    private void AnimateRotateForward(float angle, float incrementSpeed)
    {
        _player.MoveRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle), incrementSpeed));
    }

    private void PreventShortBouncing()
    {
        if (Mathf.Abs(_player.velocity.y) < 5)
        {
            _player.velocity = new Vector2(_player.velocity.x, 0);
        }
    }

    private void InstantiateSkateboard(float velocity)
    {
        _player.velocity = new Vector2(velocity, 0);
        Vector2 skatePos = new Vector2(_player.position.x, -4.46f);
        Vector2 skateVel = new Vector2(velocity, 0);
        GameObject newSkateboard = Instantiate(_skateboard, skatePos, Quaternion.identity);
        _instantiatedSkateboard = newSkateboard.GetComponent<Rigidbody2D>();
        _instantiatedSkateboard.velocity = skateVel;
        StartCoroutine(ExecuteSkateBoost((velocity + 75) * 0.2f, _instantiatedSkateboard));
    }

    private void ExpelEgg()
    {
        Vector3 position = new Vector3(-0.05f, -0.35f, 0);
        GameObject expelledEgg = Instantiate(_egg, transform.position + position, transform.rotation * Quaternion.Euler(0, 0, 90));
        expelledEgg.GetComponent<Rigidbody2D>().AddRelativeForce(Vector2.up * 4f, ForceMode2D.Impulse);
    }

    private void ManageCameraShake()
    {
        if (_player.velocity.magnitude > 20)
        {
            float noiseIntensity = Mathf.Clamp(_player.velocity.magnitude / 20 - 1, 0, 2.5f);
            if (Mathf.Abs(_noise.m_AmplitudeGain - noiseIntensity) > 0.1)
            {
                _noise.m_AmplitudeGain = noiseIntensity;
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
        }
    }

    private void StoreVelocity()
    {
        if ((_bouncyPickup || _skatePickup) && _player.position.y < -3.5)
        {
            _lastVelocity = _player.velocity;
        }
    }

    private void UpdatePositionIndicator()
    {
        Vector2 newPosition = Vector2Int.RoundToInt(new Vector2(transform.position.x + 3.25f, transform.position.y + 3.86f) / 4);
        if (newPosition != _lastPosition)
        {
            _lastPosition = newPosition;
            _distanceIndicator.text = "Distance: " + newPosition.x + "m";
            _altituteIndicator.text = "Altitude: " + newPosition.y + "m";
            _distance = newPosition.x;
        }
    }

    private void GameOver()
    {
        _player.bodyType = RigidbodyType2D.Static;
        _psOnMovement.Stop();
        Debug.Log("Game Over!");
        GameObject.FindObjectOfType<UIManager>().score = (int)_distance;
        GameObject.FindObjectOfType<UIManager>().GameOverScreen();
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

    private void OnCollisionStay2D(Collision2D collision)
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
                PreventShortBouncing();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
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
            if (_instantiatedSkateboard) _instantiatedSkateboard = null;
            float _newVelocity = Mathf.Max(_player.velocity.magnitude, _activeSkateboard.velocity.magnitude) * 1.1f;
            _activeSkateboard.velocity = new Vector2(_newVelocity, _activeSkateboard.velocity.y);
            _player.velocity = new Vector2(_newVelocity, _player.velocity.y);
        }
        else
        {
            if (obj.CompareTag("Boost"))
            {
                StartCoroutine(ExecuteRocketPropulsion());
                fireworkSound.Play();
                Destroy(obj.transform.parent.gameObject);
                return;
            }
            else if (obj.CompareTag("Speed Boost"))
            {
                StartCoroutine(ExecuteSpeedBoost());
                boostSound.Play();
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
            }
            else if (obj.CompareTag("Egg Pickup"))
            {
                StartCoroutine(ExecuteEggPropulsion());
                eggSound.Play();
            }
            Destroy(obj);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Skateboard"))
        {
            _activeSkateboard = null;
            Rigidbody2D collisionRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (collisionRb.velocity.x > _player.velocity.x)
            {
                _player.velocity = new Vector2(collisionRb.velocity.magnitude, _player.velocity.y);
            }
            else
            {
                _player.sharedMaterial = _defaultPhysics;
            }
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
}
