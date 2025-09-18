using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] Rigidbody2D myRigidBody;
    [SerializeField] float bulletSpeed = 1f;
    PlayerMovement player;
    float xSpeed;

    [System.Obsolete]
    void Start()
    {
        player = FindObjectOfType<PlayerMovement>();
        xSpeed = player.transform.localScale.x * bulletSpeed;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(xSpeed);
        transform.localScale = scale;
    }

    void Update()
    {
        myRigidBody.linearVelocityX = xSpeed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Enemy")
        {
            Destroy(other.gameObject);
        }
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);       
    }

}
