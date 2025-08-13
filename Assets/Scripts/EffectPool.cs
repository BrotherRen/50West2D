using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectPool : MonoBehaviour
{
    public GameObject effectPrefab;
    public int poolSize = 10;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(effectPrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetEffect(Vector2 position)
    {
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(effectPrefab);
        obj.transform.position = position;
        obj.SetActive(true);

        obj.GetComponent<ReturnEffectToPool>()?.Initialize(this);

        return obj;
    }

    public void ReturnEffect(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
