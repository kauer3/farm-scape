using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hay : MonoBehaviour
{
    [SerializeField] Rigidbody2D _hay;
    [SerializeField] ParticleSystem _hayParticles;

    private void FixedUpdate()
    {
        if (_hay.velocity.magnitude > 5 && _hayParticles.isStopped)
        {
            _hayParticles.Play();
        }
        else if (_hay.velocity.magnitude < 1.5 && _hayParticles.isPlaying)
        {
            _hayParticles.Stop();
        }
    }
}
