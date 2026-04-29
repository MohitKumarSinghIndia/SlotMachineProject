using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [System.Serializable]
    public class SymbolEntry
    {
        public SymbolType type;

        public SymbolView prefab;

        public int preloadCount = 10;
    }

    [Header("SYMBOLS")]
    [SerializeField]
    private List<SymbolEntry>
        symbolEntries;

    [SerializeField]
    private Transform
        pooledSymbolsParent;

    [Header("LOOPERS")]
    [SerializeField]
    private GameObject
        looperPrefab;

    [SerializeField]
    private int
        looperPreloadCount = 20;

    [SerializeField]
    private Transform
        pooledLoopersParent;

    [SerializeField]
    private bool autoExpand = true;

    private readonly Dictionary<
        SymbolType,
        SymbolPool> symbolPools =
            new Dictionary<
                SymbolType,
                SymbolPool>();

    private LooperPool looperPool;

    private void Awake()
    {
        Instance = this;

        Initialize();
    }

    private void Initialize()
    {
        for (int i = 0;
             i < symbolEntries.Count;
             i++)
        {
            SymbolEntry entry =
                symbolEntries[i];

            SymbolPool pool =
                new SymbolPool(
                    entry.prefab,
                    entry.preloadCount,
                    pooledSymbolsParent,
                    autoExpand
                );

            symbolPools.Add(
                entry.type,
                pool
            );
        }

        looperPool =
            new LooperPool(
                looperPrefab,
                looperPreloadCount,
                pooledLoopersParent,
                autoExpand
            );
    }

    public SymbolView GetSymbol(
        SymbolType type)
    {
        return symbolPools[type].Get();
    }

    public void ReturnSymbol(
        SymbolView symbol)
    {
        symbolPools[
            symbol.SymbolType]
            .Return(symbol);
    }

    public GameObject GetLooper()
    {
        return looperPool.Get();
    }

    public void ReturnLooper(
        GameObject obj)
    {
        looperPool.Return(obj);
    }
}