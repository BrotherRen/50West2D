using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("Common (70%) Prefabs")]
    public List<GameObject> commonPrefabs;

    [Header("Rare (30%) Prefabs - Optional")]
    public List<GameObject> rarePrefabs;

    [Header("Early Obstacles (0-25s) - Available from Start")]
    public List<GameObject> earlyObstaclePrefabs;

    [Header("Mid-Game Obstacles (25s+) - Added to Pool at 25s")]
    public List<GameObject> midGameObstaclePrefabs;

    [Header("Late-Game Obstacles (50s+) - Added to Pool at 50s")]
    public List<GameObject> lateGameObstaclePrefabs;

    [Header("Obstacle Spawn Settings")]
    public float obstacleSpawnInterval = 3f;
    
    [Header("Multiple Spawn Milestones")]
    [Tooltip("After 1 minute, spawn 2 obstacles per interval")]
    public float doubleSpawnTime = 60f;
    [Tooltip("After 2 minutes, spawn 3 obstacles per interval")]
    public float tripleSpawnTime = 120f;
    
    [Header("Spawn Position Variance")]
    [Tooltip("Minimum Y distance between multiple spawned obstacles")]
    public float minYDistance = 0.8f;
    [Tooltip("Optional X position variance for multiple spawns (0 = same X)")]
    public float spawnXVariance = 0.5f;
    
    [Header("Legacy Obstacle Prefabs (Deprecated)")]
    public List<GameObject> obstaclePrefabs;

    public int poolSize = 10;
    public float spawnInterval = 1.5f;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private Queue<GameObject> obstaclePool = new Queue<GameObject>();
    private List<GameObject> activeObstaclePrefabs = new List<GameObject>(); // Current available obstacles
    private float timer = 0f;
    private float obstacleTimer = 0f;
    private float gameTime = 0f;
    private bool midGameUnlocked = false;
    private bool lateGameUnlocked = false;
    private int obstaclesPerSpawn = 1; // How many obstacles to spawn per interval
    private GameObject lastSpawnedObstacle = null; // Track last spawned obstacle to avoid duplicates

    void Start()
    {
        // Initialize food object pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject prefabToUse = GetRandomPrefab();
            GameObject obj = Instantiate(prefabToUse, transform);
            obj.SetActive(false);

            var mover = obj.GetComponent<PooledObjectMover>();
            if (mover != null)
                mover.Initialize(this);

            pool.Enqueue(obj);
        }

        // Start with early obstacles only
        activeObstaclePrefabs.AddRange(earlyObstaclePrefabs);
        
        // Initialize obstacle pool with early obstacles
        InitializeObstaclePool(activeObstaclePrefabs, obstaclePool, "Initial");

        // Initialize legacy obstacle pool (for backward compatibility)
        if (obstaclePrefabs.Count > 0)
        {
            Debug.LogWarning("[ObjectPool] Legacy obstacle prefabs detected. Please migrate to the new system (Early/Mid/Late obstacles).");
        }
        
        Debug.Log($"[ObjectPool] Starting with {activeObstaclePrefabs.Count} early obstacle types");
        Debug.Log("[ObjectPool] Mid-game obstacles unlock at 25s, Late-game at 50s");
        Debug.Log($"[ObjectPool] Spawn multipliers: x2 at {doubleSpawnTime}s, x3 at {tripleSpawnTime}s");
    }

    void Update()
    {
        // Track game time
        gameTime += Time.deltaTime;
        
        // Unlock mid-game obstacles at 25 seconds
        if (!midGameUnlocked && gameTime >= 25f && midGameObstaclePrefabs.Count > 0)
        {
            midGameUnlocked = true;
            activeObstaclePrefabs.AddRange(midGameObstaclePrefabs);
            Debug.Log($"[ObjectPool] Mid-game obstacles UNLOCKED! Now {activeObstaclePrefabs.Count} obstacle types available");
        }
        
        // Unlock late-game obstacles at 50 seconds
        if (!lateGameUnlocked && gameTime >= 50f && lateGameObstaclePrefabs.Count > 0)
        {
            lateGameUnlocked = true;
            activeObstaclePrefabs.AddRange(lateGameObstaclePrefabs);
            Debug.Log($"[ObjectPool] Late-game obstacles UNLOCKED! Now {activeObstaclePrefabs.Count} obstacle types available");
        }
        
        // Update obstacles per spawn based on time milestones
        if (gameTime >= tripleSpawnTime && obstaclesPerSpawn < 3)
        {
            obstaclesPerSpawn = 3;
            Debug.Log("[ObjectPool] TRIPLE SPAWN ACTIVATED! Now spawning 3 obstacles per interval!");
        }
        else if (gameTime >= doubleSpawnTime && obstaclesPerSpawn < 2)
        {
            obstaclesPerSpawn = 2;
            Debug.Log("[ObjectPool] DOUBLE SPAWN ACTIVATED! Now spawning 2 obstacles per interval!");
        }
        
        // Food spawning timer
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;

            Vector2 spawnPos = new Vector2(11f, Random.Range(-1f, 1.4f));
            GetFromPool(spawnPos);
        }

        // Obstacle spawning timer (spawns multiple obstacles based on time)
        if (activeObstaclePrefabs.Count > 0)
        {
            obstacleTimer += Time.deltaTime;
            if (obstacleTimer >= obstacleSpawnInterval)
            {
                obstacleTimer = 0f;

                // Generate well-spaced spawn positions
                List<Vector2> spawnPositions = GenerateSpawnPositions(obstaclesPerSpawn);
                
                // Spawn multiple obstacles at different positions
                for (int i = 0; i < obstaclesPerSpawn; i++)
                {
                    GetObstacleFromPool(spawnPositions[i], i > 0); // Avoid duplicates after first spawn
                }
            }
        }
    }

    GameObject GetRandomPrefab()
    {
        float roll = Random.value;

        if (roll <= 0.7f && commonPrefabs.Count > 0)
        {
            return commonPrefabs[Random.Range(0, commonPrefabs.Count)];
        }
        else if (rarePrefabs.Count > 0)
        {
            return rarePrefabs[Random.Range(0, rarePrefabs.Count)];
        }

        // Fallback
        return commonPrefabs.Count > 0 ? commonPrefabs[0] : null;
    }

    public GameObject GetFromPool(Vector2 position)
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.transform.position = position;
            obj.SetActive(true);

            var mover = obj.GetComponent<PooledObjectMover>();
            if (mover != null)
            {
                mover.Initialize(this);

                // Reset movement-related state
                mover.ResetState(); // ← You'll implement this
            }

            return obj;
        }
        else
        {
            GameObject prefabToUse = GetRandomPrefab();
            GameObject obj = Instantiate(prefabToUse, position, Quaternion.identity);
            obj.GetComponent<PooledObjectMover>()?.Initialize(this);
            return obj;
        }
    }

    // Helper method to generate well-spaced spawn positions for multiple obstacles
    List<Vector2> GenerateSpawnPositions(int count)
    {
        List<Vector2> positions = new List<Vector2>();
        List<float> usedYPositions = new List<float>();
        
        float minY = -1f;
        float maxY = 1.4f;
        float baseX = 11f;
        
        for (int i = 0; i < count; i++)
        {
            float yPos = 0f;
            int attempts = 0;
            bool validPosition = false;
            
            // Try to find a Y position that's far enough from existing positions
            while (!validPosition && attempts < 20)
            {
                yPos = Random.Range(minY, maxY);
                validPosition = true;
                
                // Check distance from all previously used Y positions
                foreach (float usedY in usedYPositions)
                {
                    if (Mathf.Abs(yPos - usedY) < minYDistance)
                    {
                        validPosition = false;
                        break;
                    }
                }
                
                attempts++;
            }
            
            usedYPositions.Add(yPos);
            
            // Add X variance for more spread
            float xPos = baseX + Random.Range(-spawnXVariance, spawnXVariance);
            
            positions.Add(new Vector2(xPos, yPos));
        }
        
        return positions;
    }

    public GameObject GetObstacleFromPool(Vector2 position, bool avoidDuplicate = false)
    {
        if (activeObstaclePrefabs.Count == 0)
        {
            Debug.LogWarning("[ObjectPool] No active obstacle prefabs available!");
            return null;
        }

        if (obstaclePool.Count > 0)
        {
            GameObject obj = obstaclePool.Dequeue();
            
            // If avoiding duplicates and this is the same as last spawned, try to find a different one
            if (avoidDuplicate && lastSpawnedObstacle != null && obstaclePool.Count > 0)
            {
                // Check if this object matches the last spawned prefab
                bool isDuplicate = obj.name.Replace("(Clone)", "").Trim() == lastSpawnedObstacle.name;
                
                if (isDuplicate)
                {
                    // Put it back and try to get a different one
                    obstaclePool.Enqueue(obj);
                    
                    // Try up to 3 times to get a different obstacle
                    for (int attempt = 0; attempt < 3 && obstaclePool.Count > 0; attempt++)
                    {
                        obj = obstaclePool.Dequeue();
                        isDuplicate = obj.name.Replace("(Clone)", "").Trim() == lastSpawnedObstacle.name;
                        
                        if (!isDuplicate)
                            break;
                        else
                            obstaclePool.Enqueue(obj);
                    }
                }
            }
            
            obj.transform.position = position;
            
            // Reset state BEFORE activating the object
            // Initialize ObstacleController
            var obstacle = obj.GetComponent<ObstacleController>();
            if (obstacle != null)
            {
                obstacle.Initialize(this);
                obstacle.ResetState();
            }

            // Initialize BouncingTireController
            var bouncingTire = obj.GetComponent<BouncingTireController>();
            if (bouncingTire != null)
            {
                bouncingTire.Initialize(this);
                bouncingTire.ResetState();
            }

            // Initialize SimpleBouncingTire
            var simpleTire = obj.GetComponent<SimpleBouncingTire>();
            if (simpleTire != null)
            {
                simpleTire.Initialize(this);
                simpleTire.ResetState();
            }
            
            // Activate the object AFTER resetting state
            obj.SetActive(true);

            // Track for duplicate avoidance
            lastSpawnedObstacle = activeObstaclePrefabs.Find(prefab => 
                obj.name.Replace("(Clone)", "").Trim() == prefab.name);

            return obj;
        }
        else
        {
            // Create new obstacle if pool is empty - select from active obstacles
            GameObject obstaclePrefab = SelectObstaclePrefab(avoidDuplicate);
            GameObject obj = Instantiate(obstaclePrefab, position, Quaternion.identity);
            
            // Initialize the appropriate controller
            obj.GetComponent<ObstacleController>()?.Initialize(this);
            obj.GetComponent<BouncingTireController>()?.Initialize(this);
            obj.GetComponent<SimpleBouncingTire>()?.Initialize(this);
            
            lastSpawnedObstacle = obstaclePrefab;
            return obj;
        }
    }
    
    // Helper method to select an obstacle prefab, optionally avoiding duplicates
    GameObject SelectObstaclePrefab(bool avoidDuplicate)
    {
        if (activeObstaclePrefabs.Count == 0)
        {
            Debug.LogError("[ObjectPool] No active obstacle prefabs!");
            return null;
        }
        
        // If we only have 1 obstacle type or not avoiding duplicates, just pick randomly
        if (activeObstaclePrefabs.Count == 1 || !avoidDuplicate || lastSpawnedObstacle == null)
        {
            return activeObstaclePrefabs[Random.Range(0, activeObstaclePrefabs.Count)];
        }
        
        // Try to pick a different obstacle than the last one
        GameObject selectedPrefab;
        int attempts = 0;
        
        do
        {
            selectedPrefab = activeObstaclePrefabs[Random.Range(0, activeObstaclePrefabs.Count)];
            attempts++;
        }
        while (selectedPrefab == lastSpawnedObstacle && attempts < 10);
        
        return selectedPrefab;
    }

    public void ReturnToPool(GameObject obj)
    {
        // Check if it's an obstacle (any type) or food item
        if (obj.GetComponent<ObstacleController>() != null || 
            obj.GetComponent<BouncingTireController>() != null || 
            obj.GetComponent<SimpleBouncingTire>() != null)
        {
            obj.SetActive(false);
            obstaclePool.Enqueue(obj);
        }
        else
        {
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    // Helper method to initialize obstacle pools
    void InitializeObstaclePool(List<GameObject> prefabs, Queue<GameObject> pool, string poolName)
    {
        if (prefabs.Count > 0)
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obstaclePrefab = prefabs[Random.Range(0, prefabs.Count)];
                GameObject obj = Instantiate(obstaclePrefab, transform);
                obj.SetActive(false);

                // Check for ObstacleController first
                var obstacle = obj.GetComponent<ObstacleController>();
                if (obstacle != null)
                    obstacle.Initialize(this);
                
                // Check for SimpleBouncingTire
                var simpleBouncingTire = obj.GetComponent<SimpleBouncingTire>();
                if (simpleBouncingTire != null)
                {
                    simpleBouncingTire.Initialize(this);
                    Debug.Log($"{poolName} SimpleBouncingTire found and initialized!");
                }
                
                // Check for BouncingTireController (legacy)
                var bouncingTire = obj.GetComponent<BouncingTireController>();
                if (bouncingTire != null)
                    bouncingTire.Initialize(this);

                pool.Enqueue(obj);
            }
            Debug.Log($"{poolName} obstacle pool initialized with {poolSize} objects");
        }
    }

}
