using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("Common (70%) Prefabs")]
    public List<GameObject> commonPrefabs;

    [Header("Rare (30%) Prefabs - Optional")]
    public List<GameObject> rarePrefabs;

    public int poolSize = 10;
    public float spawnInterval = 1.5f;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private float timer = 0f;

    void Start()
    {
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
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;

            Vector2 spawnPos = new Vector2(11f, Random.Range(-1f, 1.4f));
            GetFromPool(spawnPos);
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
            mover.ResetState(); // ← You’ll implement this
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

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
