using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableObject : MonoBehaviour
{
    [SerializeField] float health;
    [SerializeField] float playerForce;
    [SerializeField] float force;
    [SerializeField] bool particlesOnCollision;
    [SerializeField] ParticleSystem particles;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Collider2D collider2d;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.relativeVelocity.magnitude > playerForce || collision.relativeVelocity.magnitude > force)
        {
            EmitParticles();
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
            EmitParticles();
        }
        else if (particlesOnCollision)
        {
            particles.Emit(Mathf.RoundToInt(damage/2.5f));
        }
    }

    private void EmitParticles()
    {
        Destroy(spriteRenderer);
        collider2d.enabled = false;
        ParticleSystem.MainModule main = particles.main;
        main.stopAction = ParticleSystemStopAction.Destroy;
        particles.Emit(125);
    }

}
