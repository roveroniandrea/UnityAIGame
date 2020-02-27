using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public ParticleSystem hitParticles;
    public float relativeHitToSpawnParticles = 10f;
    CharacterMotions characterMotions;

    private void Start() {
        characterMotions = GetComponent<CharacterMotions>();
    }


    private void OnCollisionEnter2D(Collision2D collision) {
        if(collision.gameObject.GetComponent<CharacterMotions>() != null) {
            Vector2 relativeHit = collision.relativeVelocity;
            if(relativeHit.magnitude > relativeHitToSpawnParticles) {
                Instantiate(hitParticles, collision.GetContact(0).point, Quaternion.identity);
            }
        }
    }
}
