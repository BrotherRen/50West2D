using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnEffectToPool : MonoBehaviour
{
    private EffectPool pool;

    public void Initialize(EffectPool effectPool)
    {
        pool = effectPool;

        // You can tweak this to match your effect’s duration
        Invoke(nameof(Deactivate), 0.5f);
    }

    void Deactivate()
    {
        if (pool != null)
        {
            pool.ReturnEffect(gameObject);
        }
    }
}
