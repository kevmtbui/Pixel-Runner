using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1f;

    Rigidbody2D myRigidbody;
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        myRigidbody.linearVelocityX = moveSpeed;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        moveSpeed *= -1;
        FlipEnemyFacing();
    }

    void FlipEnemyFacing()
    {
        transform.localScale = new Vector2(Mathf.Sign(moveSpeed), 1f);
    }
}
