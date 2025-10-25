using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("Common (70%) Prefabs")]
    public List<GameObject> commonPrefabs;

    [Header("Rare (30%) Prefabs - Optional")]
    public List<GameObject> rarePrefabs;

    [Header("Obstacle Prefabs")]
    public List<GameObject> obstaclePrefabs;
    public float obstacleSpawnInterval = 4f;

    public int poolSize = 10;
    public float spawnInterval = 1.5f;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private Queue<GameObject> obstaclePool = new Queue<GameObject>();
    private float timer = 0f;
    private float obstacleTimer = 0f;

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

        // Initialize obstacle pool
        if (obstaclePrefabs.Count > 0)
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
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
                    Debug.Log("SimpleBouncingTire found and initialized!");
                }
                
                // Check for BouncingTireController (legacy)
                var bouncingTire = obj.GetComponent<BouncingTireController>();
                if (bouncingTire != null)
                    bouncingTire.Initialize(this);

                obstaclePool.Enqueue(obj);
            }
        }
    }

    void Update()
    {
        // Food spawning timer
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;

            Vector2 spawnPos = new Vector2(11f, Random.Range(-1f, 1.4f));
            GetFromPool(spawnPos);
        }

        // Obstacle spawning timer (slower rate)
        if (obstaclePrefabs.Count > 0)
        {
            obstacleTimer += Time.deltaTime;
            if (obstacleTimer >= obstacleSpawnInterval)
            {
                obstacleTimer = 0f;

                Vector2 spawnPos = new Vector2(11f, Random.Range(-1f, 1.4f));
                GetObstacleFromPool(spawnPos);
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

    public GameObject GetObstacleFromPool(Vector2 position)
    {
        if (obstaclePool.Count > 0)
        {
            GameObject obj = obstaclePool.Dequeue();
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

            return obj;
        }
        else
        {
            // Create new obstacle if pool is empty
            GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
            GameObject obj = Instantiate(obstaclePrefab, position, Quaternion.identity);
            
            // Initialize the appropriate controller
            obj.GetComponent<ObstacleController>()?.Initialize(this);
            obj.GetComponent<BouncingTireController>()?.Initialize(this);
            obj.GetComponent<SimpleBouncingTire>()?.Initialize(this);
            
            return obj;
        }
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
}
