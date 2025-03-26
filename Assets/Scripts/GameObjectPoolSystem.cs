using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPoolSystem : MonoBehaviour
{
    [SerializeField]
    private GameObjectPoolReference[] pools;

    void Awake()
    {
        foreach (var pool in pools)
        {
            pool.CreatePool();
        }
    }

    void OnDestroy()
    {
        foreach (var pool in pools)
        {
            pool.ClearPool();
        }
    }
}
