using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    CharacterMotions characterMotions;
    [Header("AI settings")]
    public string team = "Enemy";
    public bool use16Raycasts = false;
    public float rayDistance = 20f;

    [Header("Particles")]
    public ParticleSystem hitParticles;
    public float relativeHitToSpawnParticles = 10f;

    private void Start() {
        characterMotions = GetComponent<CharacterMotions>();
    }

    private void FixedUpdate() {
        int numOfRaycasts = use16Raycasts ? 16 : 8;
        float raycastAngle = 360f / numOfRaycasts;
        List<float> inputResult = new List<float>();

        for(int i = 0; i < numOfRaycasts; i++) {
            float radians = raycastAngle * i * Mathf.Deg2Rad;
            Vector2 rayDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            List<RaycastHit2D> hits = new List<RaycastHit2D>(Physics2D.RaycastAll(transform.position, rayDirection, rayDistance));
            hits.Sort(CompareHitsByDistance);
            float platformDistance = 0;
            float outOfMapDistance = 0;
            foreach (RaycastHit2D hit in hits) {
                if(platformDistance == 0 && hit.collider.GetComponent<Platform>() != null) {
                    platformDistance = hit.distance;
                }
                if (outOfMapDistance == 0 && hit.collider.GetComponent<OutOfMap>() != null) {
                    outOfMapDistance = hit.distance;
                }
            }
            inputResult.Add(1f - platformDistance / rayDistance);
            Debug.DrawLine(transform.position, transform.position + new Vector3(rayDirection.x, rayDirection.y, 0) * platformDistance, Color.black);
            inputResult.Add(1f - outOfMapDistance / rayDistance);
            Debug.DrawLine(transform.position, transform.position + new Vector3(rayDirection.x, rayDirection.y, 0) * outOfMapDistance, Color.red);
        }
        //TODO:
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if(collision.gameObject.GetComponent<CharacterMotions>() != null) {
            Vector2 relativeHit = collision.relativeVelocity;
            if(relativeHit.magnitude > relativeHitToSpawnParticles) {
                Instantiate(hitParticles, collision.GetContact(0).point, Quaternion.identity);
            }
        }
    }

    int CompareHitsByDistance(RaycastHit2D hit1, RaycastHit2D hit2) {
        return (int)(hit1.distance - hit2.distance);
    }
}
