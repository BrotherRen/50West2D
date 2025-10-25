using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float offScreenX = -15f;

    [Header("References")]
    public ObjectPool objectPool;
    
    private ObjectPool pool;
    private float timeAlive;

    // Initialize is called by the ObjectPool when an object is taken from the pool
    public void Initialize(ObjectPool objectPool)
    {
        pool = objectPool;
    }

    // OnEnable is called every time the GameObject is activated (either initially or from the pool)
    void OnEnable()
    {
        timeAlive = 0f;
        Debug.Log($"[ObstacleController] Obstacle {gameObject.name} ENABLED at position {transform.position}");
    }

    void Update()
    {
        timeAlive += Time.deltaTime;

        // Move the object to the left based on moveSpeed - SAME AS PooledObjectMover
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);

        // Check if the object has moved far enough off-screen to the left
        if (transform.position.x <= offScreenX)
        {
            Debug.Log($"[ObstacleController] Obstacle {gameObject.name} returned to pool: Off-screen at X={transform.position.x}");
            ReturnToPool();
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
            
            Debug.Log($"[ObstacleController] Obstacle {gameObject.name} damaged Player ({other.gameObject.name})");
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
                Debug.LogWarning($"[ObstacleController] Obstacle {gameObject.name} has no pool reference. Destroying object to prevent leak.");
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

        // If the object uses a Rigidbody2D, reset its velocity and angular velocity
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    // Legacy method for compatibility - now just calls ReturnToPool
    public void DestroyObstacle()
    {
        ReturnToPool();
    }
}

