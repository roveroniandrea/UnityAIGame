using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMotions : MonoBehaviour
{
    Rigidbody2D rigid;
    Pavimento ground;
    SpriteRenderer sprite;
    [Header("Movement")]
    public float speed = 5f;
    public float maxHorizontalSpeed = 10f;
    public float jumpForce = 5.0f;
    public float dashForce = 15f;
    public bool contrastMovement = true;
    public float contrastStrength = 1f;

    [Header("Other")]
    public ParticleSystem explosion;

    private void Start() {
        rigid = GetComponent<Rigidbody2D>();
        //ne trova uno solo
        ground = FindObjectOfType<Pavimento>();
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Update() {
        if(Mathf.Abs(rigid.velocity.x) > 0.01) {
            sprite.flipX = rigid.velocity.x < 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.GetComponent<FuoriMappa>() != null) {
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

    public void ContrastMovement() {
        if (contrastMovement) {
            rigid.AddForce(Vector2.left * rigid.velocity.x * contrastStrength, ForceMode2D.Force);
        }
    }
}
