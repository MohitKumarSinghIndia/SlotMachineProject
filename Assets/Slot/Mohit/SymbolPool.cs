using System.Collections.Generic;
using UnityEngine;

public class SymbolPool
{
    private readonly Queue<SymbolView>
        pool =
            new Queue<SymbolView>();

    private readonly SymbolView prefab;

    private readonly Transform parent;

    private readonly bool autoExpand;

    public SymbolPool(
        SymbolView prefab,
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

    private SymbolView Create()
    {
        SymbolView obj =
            Object.Instantiate(
                prefab,
                parent
            );

        obj.gameObject.SetActive(false);

        pool.Enqueue(obj);

        return obj;
    }

    public SymbolView Get()
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
                    $"Pool Empty : {prefab.name}"
                );

                return null;
            }
        }

        SymbolView obj =
            pool.Dequeue();

        obj.gameObject.SetActive(true);

        return obj;
    }

    public void Return(
        SymbolView obj)
    {
        obj.ResetState();

        obj.transform.SetParent(
            parent,
            false
        );

        pool.Enqueue(obj);
    }
}