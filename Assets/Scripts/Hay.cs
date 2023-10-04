using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hay : MonoBehaviour
{
    [SerializeField] Rigidbody2D hay;
    [SerializeField] ParticleSystem hayParticles;

    // private void Start()
    // {
        // hayParticles.Play();
    // }

    private void FixedUpdate()
    {
        if (hay.velocity.magnitude > 5 && hayParticles.isStopped)
        {
            hayParticles.Play();
            Debug.Log("Particles playing");
        }
        else if (hay.velocity.magnitude < 1.5 && hayParticles.isPlaying)
        {
            hayParticles.Stop();
            Debug.Log("Particles stopped");
        }
    }
}
