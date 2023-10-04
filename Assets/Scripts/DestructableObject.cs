using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableObject : MonoBehaviour
{
    [SerializeField] float health;
    [SerializeField] float playerForce;
    [SerializeField] float force;
    [SerializeField] bool particlesOnCollision;
    [SerializeField] float particlesSpeedMultiplier;
    [SerializeField] int particlesQuantity;
    [SerializeField] ParticleSystem particles;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Collider2D collider2d;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.relativeVelocity.magnitude > playerForce || collision.relativeVelocity.magnitude > force)
        {
            EmitParticles(collision.relativeVelocity.magnitude);
        }
        else if (health > 0)
        {
            CheckHealth(collision.relativeVelocity.magnitude);
        }
    }

    private void CheckHealth(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            EmitParticles(damage);
        }
        else if (particlesOnCollision)
        {
            particles.Emit(Mathf.RoundToInt(damage/1.5f));
        }
    }

    public void EmitParticles(float force)
    {
        Destroy(spriteRenderer);
        collider2d.enabled = false;
        ParticleSystem.MainModule main = particles.main;
        main.startSpeedMultiplier = force * particlesSpeedMultiplier;
        main.stopAction = ParticleSystemStopAction.Destroy;
        particles.Emit(particlesQuantity);
    }
}
