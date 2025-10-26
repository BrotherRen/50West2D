using UnityEngine;

public class GasTankPickup : MonoBehaviour
{
    [Header("References")]
    public ObjectPool objectPool;
    
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float offScreenX = -15f;
    
    [Header("Effects")]
    public EffectPool effectPool;
    
    private ObjectPool pool;

    // Initialize is called by the ObjectPool when an object is taken from the pool
    public void Initialize(ObjectPool objectPool)
    {
        pool = objectPool;
    }

    void Update()
    {
        // Move left
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);

        // Check if off-screen
        if (transform.position.x <= offScreenX)
        {
            Debug.Log($"[GasTankPickup] Gas tank {gameObject.name} returned to pool: Off-screen");
            ReturnToPool();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[GasTankPickup] Collision detected with: {other.gameObject.name}, Tag: {other.tag}");
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("[GasTankPickup] Player collision confirmed!");
            
            // Get FuelBarManager and add fuel
            FuelBarManager fuelManager = FindObjectOfType<FuelBarManager>();
            if (fuelManager != null)
            {
                Debug.Log("[GasTankPickup] FuelBarManager found! Adding fuel...");
                fuelManager.AddFuel();
            }
            else
            {
                Debug.LogError("[GasTankPickup] FuelBarManager NOT FOUND in scene!");
            }
            
            // Get BusController and increase speed
            BusController busController = other.GetComponent<BusController>();
            if (busController != null)
            {
                Debug.Log("[GasTankPickup] BusController found! Increasing speed...");
                busController.IncreaseSpeed();
            }
            else
            {
                Debug.LogError("[GasTankPickup] BusController NOT FOUND on player!");
            }

            // Spawn effect
            if (effectPool != null)
            {
                effectPool.GetEffect(transform.position);
            }

            Debug.Log($"[GasTankPickup] Gas tank collected successfully!");
            ReturnToPool();
        }
        else
        {
            Debug.Log($"[GasTankPickup] Collision with non-player object: {other.tag}");
        }
    }

    public void ReturnToPool()
    {
        if (gameObject.activeSelf)
        {
            if (pool != null)
            {
                pool.ReturnToPool(gameObject);
            }
            else
            {
                Debug.LogWarning($"[GasTankPickup] No pool reference. Destroying object.");
                Destroy(gameObject);
            }
        }
    }

    public void ResetState()
    {
        // Reset visual components
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }
}

