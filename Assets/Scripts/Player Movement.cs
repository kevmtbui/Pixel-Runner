using System;
using JetBrains.Annotations;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] float runSpeed = 10f;
    [SerializeField] float jumpSpeed = 10f;
    [SerializeField] float climbSpeed = 3f;

    float defaultGravity;
    bool hasClimbed;
    
    bool playerHasHorizontalSpeed = false;
    Vector2 moveInput;
    Rigidbody2D myRigidbody;
    CapsuleCollider2D myCapsuleCollider2D;
    Animator myAnimator;
 
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myCapsuleCollider2D = GetComponent<CapsuleCollider2D>();
        defaultGravity = myRigidbody.gravityScale;
    }

    void Update()
    {
        Run();
        CheckIsRunning(); 
        FlipSprite();   
        ClimbLadder();
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if(!myCapsuleCollider2D.IsTouchingLayers(LayerMask.GetMask("Ground"))) {return;}

        if(value.isPressed)
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
        if(playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2 (Mathf.Sign(myRigidbody.linearVelocityX), 1f);
        }  
    }

    void ClimbLadder()
    {
        if(!myCapsuleCollider2D.IsTouchingLayers(LayerMask.GetMask("Climbing"))) 
        {
            myRigidbody.gravityScale = defaultGravity; 
            hasClimbed = false;
            myAnimator.SetBool("IsClimbing", false);
            return;
        }

        bool playerIsClimbing = Mathf.Abs(moveInput.y) > Mathf.Epsilon;
        
        if(playerIsClimbing)
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


}
