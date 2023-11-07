using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DestructableObject : MonoBehaviour
{
    [SerializeField] float _health;
    [SerializeField] float _playerForce;
    [SerializeField] float _force;
    [SerializeField] bool _particlesOnCollision;
    [SerializeField] float _particlesSpeedMultiplier;
    [SerializeField] int _particlesQuantity;
    [SerializeField] ParticleSystem _particles;
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] Collider2D _collider2d;

    private AudioSource destroySound;

    private void OnCollisionEnter2D(Collision2D _collision)
    {
        Debug.Log(_collision.relativeVelocity.magnitude);
        if (_collision.gameObject.CompareTag("Player") && _collision.relativeVelocity.magnitude > _playerForce || _collision.relativeVelocity.magnitude > _force)
        {
            EmitParticles(_collision.relativeVelocity.magnitude);
            destroySound = GetComponent<AudioSource>();
            destroySound.Play();
        }
        else if (_health > 0)
        {
            CheckHealth(_collision.relativeVelocity.magnitude);
        }
    }

    private void CheckHealth(float _damage)
    {
        _health -= _damage;
        if (_health <= 0)
        {
            EmitParticles(_damage);
        }
        else if (_particlesOnCollision)
        {
            _particles.Emit(Mathf.RoundToInt(_damage/1.5f));
        }
    }

    public void EmitParticles(float _particlesForce)
    {
        Destroy(_spriteRenderer);
        _collider2d.enabled = false;
        ParticleSystem.MainModule _main = _particles.main;
        _main.startSpeedMultiplier = _particlesForce * _particlesSpeedMultiplier;
        _main.stopAction = ParticleSystemStopAction.Destroy;
        _particles.Emit(_particlesQuantity);
    }
}
