using UnityEngine;

public class BouncingTireController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float offScreenX = -15f;

    [Header("Bouncing Physics")]
    public float bounceForce = 8f;
    public float bounceInterval = 1f;
    public float bounceHeight = 2f;
    public bool usePhysicsBounce = true; // Use Rigidbody2D physics for realistic bouncing
    public bool useSimpleBounce = false; // Use simple sine wave bouncing

    [Header("References")]
    public ObjectPool objectPool;
    
    private ObjectPool pool;
    private float timeAlive;
    private float bounceTimer = 0f;
    private Vector3 startPosition;
    private Rigidbody2D rb;
    private TireRotator tireRotator;

    // Initialize is called by the ObjectPool when an object is taken from the pool
    public void Initialize(ObjectPool objectPool)
    {
        pool = objectPool;
    }

    // OnEnable is called every time the GameObject is activated
    void OnEnable()
    {
        timeAlive = 0f;
        bounceTimer = 0f;
        startPosition = transform.position;
        
        // Get components
        rb = GetComponent<Rigidbody2D>();
        tireRotator = GetComponent<TireRotator>();
        
        // Use a coroutine to ensure Rigidbody2D is properly initialized
        StartCoroutine(InitializePhysics());
        
        Debug.Log($"[BouncingTireController] Tire {gameObject.name} ENABLED at position {transform.position}");
    }
    
    private System.Collections.IEnumerator InitializePhysics()
    {
        // Wait one frame to ensure Rigidbody2D is fully initialized
        yield return null;
        
        // Set up physics bouncing
        if (usePhysicsBounce && rb != null)
        {
            rb.velocity = new Vector2(-moveSpeed, 0); // Start moving left
            Debug.Log($"[BouncingTireController] Physics initialized for tire {gameObject.name}, velocity: {rb.velocity}");
        }
        else if (rb == null)
        {
            Debug.LogWarning($"[BouncingTireController] No Rigidbody2D found on tire {gameObject.name}");
        }
    }

    void Update()
    {
        timeAlive += Time.deltaTime;
        bounceTimer += Time.deltaTime;

        if (useSimpleBounce)
        {
            // Simple sine wave bouncing
            Vector3 newPosition = transform.position;
            newPosition.x -= moveSpeed * Time.deltaTime; // Move left
            newPosition.y = startPosition.y + Mathf.Sin(timeAlive * bounceForce) * bounceHeight;
            transform.position = newPosition;
        }
        else if (!usePhysicsBounce)
        {
            // Just move left without bouncing
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
        }

        // Physics bouncing is handled in FixedUpdate
        if (usePhysicsBounce && bounceTimer >= bounceInterval)
        {
            Bounce();
            bounceTimer = 0f;
        }

        // Check if the object has moved far enough off-screen to the left
        if (transform.position.x <= offScreenX)
        {
            Debug.Log($"[BouncingTireController] Tire {gameObject.name} returned to pool: Off-screen at X={transform.position.x}");
            ReturnToPool();
        }
    }

    void FixedUpdate()
    {
        if (usePhysicsBounce && rb != null)
        {
            // Maintain leftward movement
            Vector2 velocity = rb.velocity;
            velocity.x = -moveSpeed;
            rb.velocity = velocity;
        }
    }

    void Bounce()
    {
        if (rb != null)
        {
            // Add upward force for bouncing
            rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
        }
    }

    // OnTriggerEnter2D is called when this object's trigger collider enters another collider
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the other collider has the "Player" tag
        if (other.CompareTag("Player"))
        {
            // Get the player's health system and damage them
            BusController player = other.GetComponent<BusController>();
            if (player != null)
            {
                player.TakeDamage(1);
            }
            
            Debug.Log($"[BouncingTireController] Tire {gameObject.name} damaged Player ({other.gameObject.name})");
            ReturnToPool(); // Return the object to the pool after collision
        }
    }

    // Public method to return the object to its pool
    public void ReturnToPool()
    {
        // IMPORTANT: Only proceed if the object is currently active.
        if (gameObject.activeSelf)
        {
            // Ensure the 'pool' reference is valid before attempting to return the object.
            if (pool != null)
            {
                pool.ReturnToPool(gameObject);
            }
            else
            {
                // If for some reason the pool reference is lost, destroy the object to prevent it from being orphaned
                Debug.LogWarning($"[BouncingTireController] Tire {gameObject.name} has no pool reference. Destroying object to prevent leak.");
                Destroy(gameObject);
            }
        }
    }

    // ResetState is called by the ObjectPool when an object is dequeued and prepared for reuse.
    public void ResetState()
    {
        // Reset any necessary logic like velocity, scale, alpha, etc.
        // The position is usually set by ObjectPool.GetObstacleFromPool, so no need to set it here.

        // Ensure the object's visual components are enabled
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        // Ensure the object's collider is enabled
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        // Reset Rigidbody2D if it exists - but DON'T zero velocity here
        // The velocity will be set properly in OnEnable()
        if (rb != null)
        {
            rb.angularVelocity = 0f; // Only reset angular velocity
            // Don't reset linear velocity here - let OnEnable handle it
        }
        
        // Reset bounce timer
        bounceTimer = 0f;
    }

    // Legacy method for compatibility - now just calls ReturnToPool
    public void DestroyObstacle()
    {
        ReturnToPool();
    }
}

