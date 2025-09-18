using System;
using System.Collections;
using JetBrains.Annotations;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] float runSpeed = 10f;
    [SerializeField] float jumpSpeed = 10f;
    [SerializeField] float climbSpeed = 3f;
    [SerializeField] float maxBounceSpeed = 1.5f;
    [SerializeField] Vector2 deathKick = new Vector2(10f, 10f);

    [SerializeField] float reloadTime = 0.417f; 

    [Header("Game Objects")]
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] GameObject bow;

    float defaultGravity;
    bool hasClimbed;
    bool isShooting;

    bool playerHasHorizontalSpeed = false;
    Vector2 moveInput;
    Rigidbody2D myRigidbody;
    CapsuleCollider2D myBodyCollider;

    BoxCollider2D myFeetCollider;
    Animator myAnimator;

    bool isAlive = true;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFeetCollider = GetComponent<BoxCollider2D>();
        defaultGravity = myRigidbody.gravityScale;
    }

    void Update()
    {
        if (!isAlive) { return; };
        Run();
        CheckIsRunning();
        FlipSprite();
        ClimbLadder();
        SetMaxBounceSpeed();
        Die();

    }

    void OnAttack(InputValue value)
    {
        if (isShooting) { return; }
        ;
        isShooting = true;
        myAnimator.SetBool("IsShooting", isShooting);
        StartCoroutine(ResetShootingAfterDelay());
    }

    IEnumerator ResetShootingAfterDelay()
    {
        yield return new WaitForSeconds(reloadTime); 
        Instantiate(arrowPrefab, bow.transform.position, transform.rotation);
        isShooting = false;
        myAnimator.SetBool("IsShooting", isShooting);
    }

    void OnMove(InputValue value)
    {
        if (!isAlive) { return; }
        ;
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (!isAlive) { return; }
        ;
        if (!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground"))) { return; }

        if (value.isPressed)
        {
            myRigidbody.linearVelocityY += jumpSpeed;
        }
    }

    void CheckIsRunning()
    {
        playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.linearVelocityX) > Mathf.Epsilon;
    }

    void Run()
    {
        float playerVelocityX = moveInput.x * runSpeed;
        myRigidbody.linearVelocityX = playerVelocityX;

        myAnimator.SetBool("IsRunning", playerHasHorizontalSpeed);

    }

    void FlipSprite()
    {
        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody.linearVelocityX), 1f);
        }
    }

    void ClimbLadder()
    {
        if (!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing")))
        {
            myRigidbody.gravityScale = defaultGravity;
            hasClimbed = false;
            myAnimator.SetBool("IsClimbing", false);
            return;
        }

        bool playerIsClimbing = Mathf.Abs(moveInput.y) > Mathf.Epsilon;

        if (playerIsClimbing)
        {
            hasClimbed = true;
        }

        if (hasClimbed)
        {
            myRigidbody.gravityScale = 0;
            float climbVelocity = moveInput.y * climbSpeed;
            myRigidbody.linearVelocityY = climbVelocity;
            myAnimator.SetBool("IsClimbing", playerIsClimbing);
        }
    }

    void SetMaxBounceSpeed()
    {
        if (myRigidbody.linearVelocityY > jumpSpeed * maxBounceSpeed)
        {
            myRigidbody.linearVelocityY = (float)(jumpSpeed * maxBounceSpeed);
        }
    }

    [Obsolete]
    void Die()
    {
        if (myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemy")) || myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Hazards")))
        {
            isAlive = false;
            myAnimator.SetTrigger("Dying");
            myRigidbody.linearVelocity = deathKick;
            FindObjectOfType<GameSession>().ProcessPlayerDeath();
        }
    }
    
}
