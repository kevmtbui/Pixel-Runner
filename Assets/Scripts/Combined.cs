//Arrow.cs
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

// CoinPickup.cs

using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] AudioClip coinPickupSFX;
    [SerializeField] int pointsForCoinPickup = 100;

    bool wasCollected = false;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && !wasCollected)
        {
            wasCollected = true;
            FindObjectOfType<GameSession>().AddToScore(pointsForCoinPickup);
            AudioSource.PlayClipAtPoint(coinPickupSFX, Camera.main.transform.position);
            Destroy(gameObject);
        }
    }
}

// Enemy Movement.cs 
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

// Game Session.cs 

using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSession : MonoBehaviour
{
    [System.Obsolete]

    [SerializeField] int playerLives = 3;
    [SerializeField] int score = 0;

    [SerializeField] TextMeshProUGUI livesText;
    [SerializeField] TextMeshProUGUI scoreText;

    [Obsolete]
    void Awake()
    {
        int numGameSesssions = FindObjectsOfType<GameSession>().Length;
        if (numGameSesssions > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        livesText.text = playerLives.ToString();   
        scoreText.text = score.ToString();   
    }

    [Obsolete]
    public void ProcessPlayerDeath()
    {
        if (playerLives > 1)
        {
            TakeLife();
        }
        else
        {
            ResetGameSession();
        }
    }

    public void AddToScore(int pointsToAdd)
    {
        score += pointsToAdd;
        scoreText.text = score.ToString(); 
    }

    [Obsolete]
    private void TakeLife()
    {
        playerLives--;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex); 
        livesText.text = playerLives.ToString();   
    }

    [Obsolete]
    void ResetGameSession()
    {
        FindObjectOfType<ScenePersist>().ResetScenePersist();
        SceneManager.LoadScene(0);
        Destroy(gameObject);
    }
}

//LevelExit.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [SerializeField] float time = 1f;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            StartCoroutine(LoadNextLevel());
        }
        
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSecondsRealtime(time);
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0;
        }

        FindObjectOfType<ScenePersist>().ResetScenePersist();
        SceneManager.LoadScene(nextSceneIndex);
    }
}

//Player Movement.cs 

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

//Scene Persist.cs

using UnityEngine;

public class ScenePersist : MonoBehaviour
{
    void Awake()
    {
        int numScenePersists = FindObjectsOfType<ScenePersist>().Length;
        if (numScenePersists > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public void ResetScenePersist()
    {
        Destroy(gameObject);
    }
}




