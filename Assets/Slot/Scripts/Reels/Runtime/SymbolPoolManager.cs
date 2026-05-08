using System.Collections.Generic;
using SlotMachine.Reels.Data;
using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    public class SymbolPoolManager : MonoBehaviour
    {
        [Header("Symbol Definitions")]
        [Tooltip("Assign all SymbolDefinition objects here. SymbolDefinition.SymbolId is used as the lookup key.")]
        [SerializeField] private List<SymbolDefinition> symbolDefinitions = new List<SymbolDefinition>();

        [Header("Pooling")]
        [SerializeField] private Transform poolRoot;

        [Min(0)]
        [SerializeField] private int initialPoolCountPerSymbol = 5;

        private readonly Dictionary<int, SymbolDefinition> definitionById = new Dictionary<int, SymbolDefinition>();
        private readonly Dictionary<int, Queue<SymbolView>> poolById = new Dictionary<int, Queue<SymbolView>>();

        private void Awake()
        {
            EnsurePoolRoot();
            CacheDefinitions();
            PrewarmPools();
        }

        private void OnValidate()
        {
            CacheDefinitions();
        }

        public SymbolView Acquire(int symbolId, Transform parent)
        {
            EnsurePoolRoot();
            CacheDefinitions();

            if (!definitionById.ContainsKey(symbolId))
            {
                Debug.LogError($"[{name}] No SymbolDefinition assigned for symbol id {symbolId}.");
                return null;
            }

            if (!poolById.TryGetValue(symbolId, out Queue<SymbolView> queue))
            {
                queue = new Queue<SymbolView>();
                poolById[symbolId] = queue;
            }

            SymbolView instance = null;

            while (queue.Count > 0 && instance == null)
            {
                instance = queue.Dequeue();
            }

            if (instance == null)
            {
                instance = CreateClone(symbolId, parent);
            }
            else
            {
                Transform targetParent = parent != null ? parent : poolRoot;
                instance.transform.SetParent(targetParent, false);
            }

            if (instance == null)
            {
                return null;
            }

            instance.SetSymbolIdOnly(symbolId);
            instance.gameObject.SetActive(true);

            return instance;
        }

        public void Release(SymbolView instance)
        {
            if (instance == null)
            {
                return;
            }

            EnsurePoolRoot();

            int symbolId = instance.CurrentSymbolId;

            if (!poolById.TryGetValue(symbolId, out Queue<SymbolView> queue))
            {
                queue = new Queue<SymbolView>();
                poolById[symbolId] = queue;
            }

            instance.gameObject.SetActive(false);
            instance.transform.SetParent(poolRoot, false);
            queue.Enqueue(instance);
        }

        private void CacheDefinitions()
        {
            definitionById.Clear();

            for (int i = 0; i < symbolDefinitions.Count; i++)
            {
                SymbolDefinition definition = symbolDefinitions[i];

                if (definition == null)
                {
                    Debug.LogError($"[{name}] SymbolDefinitions element {i} is NULL.");
                    continue;
                }

                int symbolId = definition.SymbolId;

                if (definitionById.ContainsKey(symbolId))
                {
                    Debug.LogWarning(
                        $"[{name}] Duplicate SymbolDefinition for symbol id {symbolId}. " +
                        $"Only the first one will be used."
                    );

                    continue;
                }

                definitionById.Add(symbolId, definition);
            }
        }

        private void PrewarmPools()
        {
            foreach (KeyValuePair<int, SymbolDefinition> pair in definitionById)
            {
                int symbolId = pair.Key;

                if (!poolById.TryGetValue(symbolId, out Queue<SymbolView> queue))
                {
                    queue = new Queue<SymbolView>();
                    poolById[symbolId] = queue;
                }

                while (queue.Count < initialPoolCountPerSymbol)
                {
                    SymbolView clone = CreateClone(symbolId, poolRoot);

                    if (clone == null)
                    {
                        break;
                    }

                    clone.gameObject.SetActive(false);
                    clone.transform.SetParent(poolRoot, false);
                    queue.Enqueue(clone);
                }
            }
        }

        private SymbolView CreateClone(int symbolId, Transform parent)
        {
            if (!definitionById.TryGetValue(symbolId, out SymbolDefinition definition) || definition == null)
            {
                Debug.LogError($"[{name}] Missing SymbolDefinition for symbol id {symbolId}.");
                return null;
            }

            Transform targetParent = parent != null ? parent : poolRoot;

            GameObject cloneObject = Instantiate(definition.gameObject, targetParent, false);
            cloneObject.name = $"{definition.name}_Clone_ID_{symbolId}";

            SymbolView symbolView = cloneObject.GetComponent<SymbolView>();

            if (symbolView == null)
            {
                symbolView = cloneObject.AddComponent<SymbolView>();
            }

            symbolView.SetSymbolIdOnly(symbolId);

            return symbolView;
        }

        private void EnsurePoolRoot()
        {
            if (poolRoot == null)
            {
                poolRoot = transform;
            }
        }
    }
}