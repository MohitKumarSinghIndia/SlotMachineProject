using System.Collections.Generic;
using UnityEngine;

public class LooperPool
{
    private readonly Queue<GameObject>
        pool =
            new Queue<GameObject>();

    private readonly GameObject prefab;

    private readonly Transform parent;

    private readonly bool autoExpand;

    public LooperPool(
        GameObject prefab,
        int preloadCount,
        Transform parent,
        bool autoExpand)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.autoExpand = autoExpand;

        for (int i = 0;
             i < preloadCount;
             i++)
        {
            Create();
        }
    }

    private GameObject Create()
    {
        GameObject obj =
            Object.Instantiate(
                prefab,
                parent
            );

        obj.SetActive(false);

        pool.Enqueue(obj);

        return obj;
    }

    public GameObject Get()
    {
        if (pool.Count == 0)
        {
            if (autoExpand)
            {
                Create();
            }
            else
            {
                Debug.LogError(
                    "Looper Pool Empty"
                );

                return null;
            }
        }

        GameObject obj =
            pool.Dequeue();

        obj.SetActive(true);

        return obj;
    }

    public void Return(
        GameObject obj)
    {
        obj.SetActive(false);

        obj.transform.SetParent(
            parent,
            false
        );

        pool.Enqueue(obj);
    }
}