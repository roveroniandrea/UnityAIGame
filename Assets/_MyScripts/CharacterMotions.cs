using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CharacterMotions : MonoBehaviour
{
    Rigidbody2D rigid;
    Platform ground;
    SpriteRenderer sprite;
    [Header("Movement")]
    public float speed = 5f;
    public float maxHorizontalSpeed = 10f;
    public float jumpForce = 5.0f;
    public float dashForce = 15f;
    public float punchForce = 5f;
    public float punchRange = 0.5f;
    public bool contrastMovement = true;
    public float contrastStrength = 1f;
    Vector2 previousVelocity;

    [Header("Hits")]
    public ParticleSystem hitParticles;
    public int maxDamage = 100;
    public float minHitToDamage = 10f;
    public int damage = 0;
    UIdamage uIdamage;

    [Header("Other")]
    public ParticleSystem explosion;

    private void Start() {
        rigid = GetComponent<Rigidbody2D>();
        //ne trova uno solo
        ground = FindObjectOfType<Platform>();
        sprite = GetComponent<SpriteRenderer>();
        uIdamage = FindObjectOfType<UIdamagePlacement>().CreateUIdamage(maxDamage);
    }

    private void Update() {
        if(Mathf.Abs(rigid.velocity.x) > 0.01) {
            sprite.flipX = rigid.velocity.x < 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.GetComponent<OutOfMap>() != null) {
            Instantiate(explosion, transform.position, Quaternion.Euler(-90, 0, 0));
            Destroy(gameObject);
        }
    }

    public void Jump() {
        if (rigid.IsTouching(ground.GetComponent<Collider2D>())) {
            rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    public void HorizontalMove(float input) {
        bool differentSigns = Mathf.Sign(rigid.velocity.x) != Mathf.Sign(input);
        if (differentSigns || Mathf.Abs(rigid.velocity.x) < maxHorizontalSpeed) {
            rigid.AddForce(Vector2.right * input * speed, ForceMode2D.Impulse);
        }
    }

    public void Dash() {
        //dash nella direzione del flipped
        int direction = sprite.flipX ? -1 : 1;
        rigid.AddForce(new Vector2(direction, 0) * dashForce, ForceMode2D.Impulse);
    }

    public void Punch() {
        int direction = sprite.flipX ? -1 : 1;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.right * direction, punchRange);
        foreach (RaycastHit2D hit in hits) {
            CharacterMotions other = hit.transform.GetComponent<CharacterMotions>();
            if (other != null) {
                other.Hitted(punchForce, direction);
            }
        }

    }

    public void ContrastMovement() {
        if (contrastMovement) {
            rigid.AddForce(Vector2.left * rigid.velocity.x * contrastStrength, ForceMode2D.Force);
        }
    }

    private void FixedUpdate() {
        previousVelocity = rigid.velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        CharacterMotions other = collision.gameObject.GetComponent<CharacterMotions>();
        if (other != null) {
            //Debug.Log(name + ": " + collision.relativeVelocity + "|" +  previousVelocity);
            float resultingMagnitude = (collision.relativeVelocity + previousVelocity).magnitude;
            if (resultingMagnitude >= minHitToDamage) {
                //Debug.Log(name + " prendo danno " + (collision.relativeVelocity + previousVelocity).magnitude);
                Hitted(resultingMagnitude);
            }
        }
    }

    public void Hitted(float amount, int punchDirection = 0) {
        Instantiate(hitParticles, transform.position, Quaternion.identity);
        Vector2 forceDirection = punchDirection == 0 ? rigid.velocity.normalized : Vector2.right * punchDirection;
        rigid.AddForce(forceDirection * (1 + damage / 100f) * amount, ForceMode2D.Impulse);
        damage += Mathf.RoundToInt(amount);
        damage = Mathf.Clamp(damage, 0, maxDamage);
        uIdamage.UpdateDamage(damage, maxDamage);
    }
}
