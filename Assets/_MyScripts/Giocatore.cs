using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Giocatore : MonoBehaviour
{
    public float speed = 5f;
    public float jump = 5.0f;
    public float dash = 15f;
    Rigidbody2D rigid;
    Pavimento pavimento;
    public ParticleSystem esplosione;

    private void Start() {
        rigid = GetComponent<Rigidbody2D>();
        pavimento = FindObjectOfType<Pavimento>();
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        transform.position += new Vector3(x, 0, 0) * speed * Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space)) {
            Salta();
        }
        if (Input.GetKeyDown(KeyCode.P)) {
            rigid.AddForce(Vector3.right * dash, ForceMode2D.Impulse);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.GetComponent<FuoriMappa>() != null) {
            Instantiate(esplosione, transform.position, Quaternion.Euler(-90, 0, 0));
            Destroy(gameObject);
        }
    }

    public void Salta() {
        if (rigid.IsTouching(pavimento.GetComponent<Collider2D>())) {
            rigid.AddForce(Vector3.up * jump, ForceMode2D.Impulse);
        }
    }
}
