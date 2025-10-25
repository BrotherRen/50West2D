using UnityEngine;

public class SimpleBouncingTire : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float offScreenX = -15f;
    
    [Header("Bouncing")]
    public float bounceForce = 8f;
    public float bounceHeight = 2f;
    public bool usePhysicsBounce = true;
    public bool useSimpleBounce = false;
    
    [Header("References")]
    public ObjectPool objectPool;
    
    private ObjectPool pool;
    private Vector3 startPosition;
    private Rigidbody2D rb;
    private float bounceTimer = 0f;
    private float bounceInterval = 1f;
    
    // Initialize is called by the ObjectPool when an object is taken from the pool
    public void Initialize(ObjectPool objectPool)
    {
        pool = objectPool;
    }

    // OnEnable is called every time the GameObject is activated (either initially or from the pool)
    void OnEnable()
    {
        startPosition = transform.position;
        bounceTimer = 0f;
        
        // Get components
        rb = GetComponent<Rigidbody2D>();
        
        // Use a coroutine to ensure Rigidbody2D is properly initialized
        StartCoroutine(InitializePhysics());
        
        Debug.Log($"[SimpleBouncingTire] Tire {gameObject.name} ENABLED at position {transform.position}");
    }
    
    private System.Collections.IEnumerator InitializePhysics()
    {
        // Wait one frame to ensure Rigidbody2D is fully initialized
        yield return null;
        
        // Set up physics bouncing
        if (usePhysicsBounce && rb != null)
        {
            rb.velocity = new Vector2(-moveSpeed, 0); // Start moving left
            Debug.Log($"[SimpleBouncingTire] Physics initialized for tire {gameObject.name}, velocity: {rb.velocity}");
        }
        else if (rb == null)
        {
            Debug.LogWarning($"[SimpleBouncingTire] No Rigidbody2D found on tire {gameObject.name}");
        }
    }
    
    void Start()
    {
        // Only run Start() if not initialized by pool (for standalone objects)
        if (pool == null)
        {
            startPosition = transform.position;
            rb = GetComponent<Rigidbody2D>();
            
            if (usePhysicsBounce && rb != null)
            {
                rb.velocity = new Vector2(-moveSpeed, 0);
            }
            
            Debug.Log("SimpleBouncingTire started!");
        }
    }
    
    void Update()
    {
        bounceTimer += Time.deltaTime;
        
        // Fallback: Ensure Rigidbody2D has proper velocity if it's not moving
        if (usePhysicsBounce && rb != null && rb.velocity.x > -1f)
        {
            Debug.Log($"[SimpleBouncingTire] Fallback: Setting velocity for tire {gameObject.name}");
            rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);
        }
        
        if (useSimpleBounce)
        {
            // Simple sine wave bouncing
            Vector3 newPosition = transform.position;
            newPosition.x -= moveSpeed * Time.deltaTime;
            newPosition.y = startPosition.y + Mathf.Sin(Time.time * bounceForce) * bounceHeight;
            transform.position = newPosition;
        }
        else if (!usePhysicsBounce)
        {
            // Just move left
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
        }
        else if (usePhysicsBounce)
        {
            // Physics-based sine wave bouncing
            Vector3 newPosition = transform.position;
            newPosition.x -= moveSpeed * Time.deltaTime;
            newPosition.y = startPosition.y + Mathf.Sin(Time.time * bounceForce) * bounceHeight;
            transform.position = newPosition;
        }
        
        // Check if the object has moved far enough off-screen to the left
        if (transform.position.x <= offScreenX)
        {
            Debug.Log($"[SimpleBouncingTire] Tire {gameObject.name} returned to pool: Off-screen at X={transform.position.x}");
            ReturnToPool();
        }
    }
    
    void FixedUpdate()
    {
        if (usePhysicsBounce && rb != null)
        {
            Vector2 velocity = rb.velocity;
            velocity.x = -moveSpeed;
            rb.velocity = velocity;
            
            // Debug log every few frames to check if velocity is being maintained
            if (Time.fixedTime % 1f < Time.fixedDeltaTime)
            {
                Debug.Log($"[SimpleBouncingTire] Tire {gameObject.name} velocity: {rb.velocity}");
            }
        }
    }
    
    void Bounce()
    {
        if (rb != null)
        {
            Debug.Log($"[SimpleBouncingTire] Adding bounce force {bounceForce} to tire {gameObject.name}");
            rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogWarning($"[SimpleBouncingTire] No Rigidbody2D found on tire {gameObject.name}");
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            BusController player = other.GetComponent<BusController>();
            if (player != null)
            {
                player.TakeDamage(1);
            }
            Debug.Log($"[SimpleBouncingTire] Tire {gameObject.name} damaged Player ({other.gameObject.name})");
            ReturnToPool();
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
                Debug.LogWarning($"[SimpleBouncingTire] Tire {gameObject.name} has no pool reference. Destroying object to prevent leak.");
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
}

