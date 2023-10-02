using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hay : MonoBehaviour
{
    [SerializeField] Rigidbody2D hay;
    [SerializeField] ParticleSystem hayParticles;

    // Check gameobject's speed and emit particles accordingly
    private void FixedUpdate()
    {
        if (hay.velocity.magnitude > 5 && hayParticles.isStopped)
        {
            hayParticles.Play();
            Debug.Log("Particles playing");
        }
        else if (hay.velocity.magnitude < 1 && hayParticles.isPlaying)
        {
            hayParticles.Stop();
            Debug.Log("Particles stopped");
        }
    }
}
