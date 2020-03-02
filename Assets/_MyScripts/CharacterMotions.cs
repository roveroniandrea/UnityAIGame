using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CharacterMotions : MonoBehaviour
{
    Rigidbody2D rigid;
    SpriteRenderer sprite;
    Vector2 previousVelocity;

    [Header("Movement")]
    public float speed = 5f;
    public float maxHorizontalSpeed = 10f;
    public bool contrastHMovement = true;
    public float contrastStrength = 1f;

    [Header("Jumping")]
    public float jumpForce = 5.0f;
    public float jumpCooldown = 0.2f;
    public float jumpReadyIn = 0f;

    [Header("Dashing")]
    public float dashForce = 15f;
    public float dashCooldown = 2f;
    public float dashReadyIn = 0f;

    [Header("Punching")]
    public float punchForce = 5f;
    public float punchRange = 0.5f;
    public float punchCooldown = 0.5f;
    public float punchReadyIn = 0f;

    [Header("Rapid down")]
    public float rapidDownStrength = 4f;
    public float rapidDownAOF = 1.5f;
    public float rapidDownAOFStrength = 10f;
    bool isGoingRapidDown = false;

    [Header("Hits")]
    public ParticleSystem hitParticles;
    public int maxDamage = 100;
    public float minHitToDamage = 10f;
    public int damage = 0;
    UIdamage uIdamage;

    [Header("Other")]
    public ParticleSystem explosion;
    Animator animator;
    public string team = "Enemy";

    private void Start() {
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        uIdamage = FindObjectOfType<UIdamagePlacement>().CreateUIdamage(maxDamage);
        animator = GetComponent<Animator>();
    }

    private void Update() {
        if(dashReadyIn > 0f) {
            dashReadyIn -= Time.deltaTime;
        }
        if(punchReadyIn > 0f) {
            punchReadyIn -= Time.deltaTime;
        }
        if(jumpReadyIn > 0f) {
            jumpReadyIn -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.GetComponent<OutOfMap>() != null) {
            Instantiate(explosion, transform.position, Quaternion.Euler(-90, 0, 0));
            Destroy(gameObject);
        }
    }

    public void Jump() {
        if (jumpReadyIn <=0f && IsGrounded(includeCharacters: true)) {
            jumpReadyIn = jumpCooldown;
            rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            if(animator != null) {
                animator.SetBool("Jumping", true);
            }
        }
    }

    public void HorizontalMove(float input) {
        bool differentSigns = Mathf.Sign(rigid.velocity.x) != Mathf.Sign(input);
        if (differentSigns || Mathf.Abs(rigid.velocity.x) < maxHorizontalSpeed) {
            rigid.AddForce(Vector2.right * input * speed, ForceMode2D.Impulse);
        }
        if (animator != null) {
            animator.SetBool("Walking", true);
        }
        sprite.flipX = input < 0f;
    }

    public void Dash() {
        if(dashReadyIn <= 0f) {
            //dash nella direzione del flipped
            int direction = sprite.flipX ? -1 : 1;
            rigid.AddForce(new Vector2(direction, 0) * dashForce, ForceMode2D.Impulse);
            dashReadyIn = dashCooldown;
            if (animator != null) {
                animator.SetTrigger("Dash");
            }
        }
    }

    public void Punch() {
        if(punchReadyIn <= 0f) {
            int direction = sprite.flipX ? -1 : 1;
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.right * direction, punchRange);
            foreach (RaycastHit2D hit in hits) {
                CharacterMotions other = hit.transform.GetComponent<CharacterMotions>();
                if (other != null) {
                    other.Hitted(punchForce, Vector2.right * direction);
                }
            }
            punchReadyIn = punchCooldown;
            if (animator != null) {
                animator.SetTrigger("Punch");
            }
        }
    }

    public void ContrastMovement() {
        if (contrastHMovement && (dashCooldown - dashReadyIn >= 0.2f)) {
            rigid.AddForce(Vector2.left * rigid.velocity.x * contrastStrength, ForceMode2D.Force);
        }
        if (animator != null) {
            animator.SetBool("Walking", false);
        }

        if (Mathf.Abs(rigid.velocity.x) > 0.1) {
            sprite.flipX = rigid.velocity.x < 0;
        }
    }

    private void FixedUpdate() {
        previousVelocity = rigid.velocity;

        if (isGoingRapidDown) {
            rigid.AddForce(Vector2.down * rapidDownStrength, ForceMode2D.Impulse);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        CharacterMotions other = collision.gameObject.GetComponent<CharacterMotions>();
        if (other != null) {
            //Debug.Log(name + ": " + collision.relativeVelocity + "|" +  previousVelocity);
            float resultingMagnitude = (collision.relativeVelocity + previousVelocity).magnitude;
            if (resultingMagnitude >= minHitToDamage) {
                //Debug.Log(name + " prendo danno " + (collision.relativeVelocity + previousVelocity).magnitude);
                Hitted(resultingMagnitude, Vector2.zero);
            }
            if (isGoingRapidDown) {
                EndRapidDown(false);
            }
        }
        else {
            if (collision.gameObject.GetComponent<Platform>() && isGoingRapidDown) {
                EndRapidDown(true);
            }
        }
        if(rigid.velocity.y >= -0.1f) {
            if (animator != null) {
                animator.SetBool("Jumping", false);
            }
        }
    }

    public void Hitted(float amount, Vector2 preciseDirection) {
        Instantiate(hitParticles, transform.position, Quaternion.identity);
        Vector2 forceDirection = preciseDirection;
        if(forceDirection == Vector2.zero) {
            forceDirection = rigid.velocity.normalized;
        }
        rigid.AddForce(forceDirection * (1 + damage / 100f) * amount, ForceMode2D.Impulse);
        damage += Mathf.RoundToInt(amount);
        damage = Mathf.Clamp(damage, 0, maxDamage);
        uIdamage.UpdateDamage(damage, maxDamage);
    }

    public void StartRapidDown() {
        if (!isGoingRapidDown) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f);
            if(!hit || hit.transform.GetComponent<Platform>() == null) {
                //Debug.Log("rapid down");
                isGoingRapidDown = true;
            }
        }
    }

    void EndRapidDown(bool dealDamage) {
        isGoingRapidDown = false;
        if (dealDamage) {
            Collider2D[] overlapColliders = Physics2D.OverlapCircleAll(transform.position, rapidDownAOF);
            foreach (Collider2D coll in overlapColliders) {
                CharacterMotions other = coll.GetComponent<CharacterMotions>();
                if (coll.gameObject != gameObject && other) {
                    Vector2 positionDifference = coll.transform.position - transform.position;
                    Vector2 forceDirection = positionDifference * 1f / positionDifference.magnitude;
                    //Debug.Log("end rapid down " + forceDirection + "m:" + forceDirection.magnitude);
                    other.Hitted(rapidDownAOFStrength, forceDirection);
                }
            }
        }
    }

    bool IsGrounded(bool includeCharacters = false) {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, GetComponent<BoxCollider2D>().size.y * 0.7f);
        if (hit && hit.transform.GetComponent<Platform>()) {
            return true;
        }
        if(includeCharacters && hit && hit.transform.GetComponent<CharacterMotions>()) {
            return true;
        }
        return false;
    }
}
